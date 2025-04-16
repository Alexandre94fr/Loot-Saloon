#region
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
#endregion

public class S_GameLobbyManager : MonoBehaviour
{
    public static S_GameLobbyManager instance;
    [SerializeField] private string gameSceneName;

    private List<S_LobbyPlayerData> _lobbyPlayerDatas = new List<S_LobbyPlayerData>();

    private List<S_LobbyPlayerData> _previousLobbyPlayerDatas = new List<S_LobbyPlayerData>();
    public List<S_LobbyPlayerData> LobbyPlayerDatas => _lobbyPlayerDatas;

    private S_LobbySettings _lobbySettings = new S_LobbySettings();

    private S_LobbyPlayerData _localLobbyPlayerData;

    private S_LobbyData _lobbyData;
    [SerializeField] private bool _inGame;
    private bool _wasDisconnected;
    private string _previousRelayCode;

    public bool IsHost => _localLobbyPlayerData != null && _localLobbyPlayerData.Id == S_LobbyManager.instance.GetHostId();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        S_LobbyEvents.OnLobbyUpdatedWithParam += OnLobbyUpdated;
    }

    private void OnDisable()
    {
        S_LobbyEvents.OnLobbyUpdatedWithParam -= OnLobbyUpdated;
    }

    public async Task<bool> CreateLobby()
    {
        S_LobbyPlayerData playerData = new S_LobbyPlayerData();
        playerData.Initialize(AuthenticationService.Instance.PlayerId, PlayerPrefs.GetString("PlayerName", "Host"));
        _localLobbyPlayerData = playerData;

        _lobbyData = new S_LobbyData();
        bool succeeded = await S_LobbyManager.instance.CreateLobbyAsync(
            _lobbySettings.maxPlayers,
            _lobbySettings.isPrivate,
            playerData.Serialize(),
            _lobbyData.Serialize()
        );

        return succeeded;
    }

    public async Task<bool> JoinLobby(string p_code)
    {
        S_LobbyPlayerData playerData = new S_LobbyPlayerData();
        playerData.Initialize(AuthenticationService.Instance.PlayerId, PlayerPrefs.GetString("PlayerName", "Guest"));
        bool succeeded = await S_LobbyManager.instance.JoinLobby(p_code, playerData.Serialize());
        return succeeded;
    }

    public async Task<bool> JoinLobbyById(string p_id)
    {
        S_LobbyPlayerData playerData = new S_LobbyPlayerData();
        playerData.Initialize(AuthenticationService.Instance.PlayerId, PlayerPrefs.GetString("PlayerName", "Guest"));
        bool succeeded = await S_LobbyManager.instance.JoinLobbyById(p_id, playerData.Serialize());

        return succeeded;
    }

    private async void OnLobbyUpdated(Lobby p_lobby)
    {
        List<Dictionary<string, PlayerDataObject>> playerData = S_LobbyManager.instance.GetPlayerData();
        _lobbyPlayerDatas.Clear();
        int nbPlayerReady = 0;
        List<string> currentPlayerIds = new List<string>();

        foreach (Dictionary<string, PlayerDataObject> data in playerData)
        {
            S_LobbyPlayerData lobbyPlayerData = new S_LobbyPlayerData();
            lobbyPlayerData.Initialize(data);

            currentPlayerIds.Add(lobbyPlayerData.Id);

            if (lobbyPlayerData.IsReady)
            {
                nbPlayerReady++;
            }

            if (lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
            {
                _localLobbyPlayerData = lobbyPlayerData;
            }

            if (lobbyPlayerData.KickPlayer && !IsHost)
            {
                LeaveLobby();
            }

            _lobbyPlayerDatas.Add(lobbyPlayerData);
        }

        foreach (S_LobbyPlayerData previousPlayer in _previousLobbyPlayerDatas)
        {
            if (!currentPlayerIds.Contains(previousPlayer.Id))
            {
                if (previousPlayer.PrefabInstance != null && previousPlayer.PrefabInstance.gameObject != null) // Check if the GameObject is not null
                {
                    previousPlayer.PrefabInstance.SetActive(false);
                    Debug.Log($"Player disconnected: {previousPlayer.GamerTag} (ID: {previousPlayer.Id})");
                }
                else
                {
                    Debug.LogWarning($"PrefabInstance for player {previousPlayer.GamerTag} (ID: {previousPlayer.Id}) is already destroyed or null.");
                }
            }
        }

        _previousLobbyPlayerDatas = new List<S_LobbyPlayerData>(_lobbyPlayerDatas);

        _lobbyData = new S_LobbyData();
        _lobbyData.Initialize(p_lobby.Data);

        S_LobbyEvents.OnLobbyUpdated?.Invoke();

        Debug.Log($"nb player ready  : {nbPlayerReady} / {_lobbyPlayerDatas.Count}");
        bool teamsBalanced = await SameNbPlayerInEachTeam();
        if (teamsBalanced && nbPlayerReady == _lobbyPlayerDatas.Count)
        {
            S_LobbyEvents.OnLobbyReady?.Invoke();
        }
        else
        {
            S_LobbyEvents.OnLobbyUnready?.Invoke();
        }

        if (IsHost)
        {
            if (!teamsBalanced || nbPlayerReady != _lobbyPlayerDatas.Count)
            {
                S_LobbyEvents.OnLobbyUnready?.Invoke();
            }
        }
        if (_lobbyData.RelayJoinCode != default && !_inGame && !IsHost)
        {
            if (_wasDisconnected)
            {
                if (_lobbyData.RelayJoinCode != _previousRelayCode)
                {
                    await JoinRelayServer(_lobbyData.RelayJoinCode);
                }
            }
            else
            {
                await JoinRelayServer(_lobbyData.RelayJoinCode);
                await SceneManager.LoadSceneAsync(gameSceneName);
            }
        }
    }


    public string GetLobbyCode()
    {
        return S_LobbyManager.instance.GetLobbyCode();
    }

    public List<S_LobbyPlayerData> GetPlayers()
    {
        return _lobbyPlayerDatas;
    }

    public void SetLobbySettings(S_LobbySettings p_lobbySettings)
    {
        _lobbySettings = p_lobbySettings;
    }

    public async Task<bool> SetPlayerReady()
    {
        _localLobbyPlayerData.IsReady = true;
        return await S_LobbyManager.instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize());
    }

    public async Task<bool> SetPlayerUnready()
    {
        _localLobbyPlayerData.IsReady = false;
        return await S_LobbyManager.instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize());
    }

    public async Task<bool> SetPlayerTeam(E_PlayerTeam p_team)
    {
        _localLobbyPlayerData.Team = p_team;
        _localLobbyPlayerData.IsReady = false;
        return await S_LobbyManager.instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize());
    }

    public async Task StartGame()
    {
        string joinRelayCode = await S_RelayManager.instance.CreateRelay(_lobbySettings.maxPlayers);
        _inGame = true;
        _lobbyData.RelayJoinCode = joinRelayCode;
        _localLobbyPlayerData.IsReady = false;
        await S_LobbyManager.instance.UpdateLobbyData(_localLobbyPlayerData.Id, _lobbyData.Serialize());
        string allocationId = S_RelayManager.instance.GetAllocationId();
        string connectionData = S_RelayManager.instance.GetConnectionData();
        await S_LobbyManager.instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);

        await SceneManager.LoadSceneAsync(gameSceneName);
    }

    public async Task<bool> SameNbPlayerInEachTeam()
    {
        int redTeamCount = 0;
        int blueTeamCount = 0;
        foreach (S_LobbyPlayerData playerData in LobbyPlayerDatas)
        {
            if (playerData.Team == E_PlayerTeam.RED)
            {
                redTeamCount++;
            }
            else if (playerData.Team == E_PlayerTeam.BLUE)
            {
                blueTeamCount++;
            }
        }

        return redTeamCount == blueTeamCount;
    }

    private async Task<bool> JoinRelayServer(string p_lobbyDataRelayJoinCode)
    {
        _inGame = true;
        await S_RelayManager.instance.JoinRelay(p_lobbyDataRelayJoinCode);
        string allocationId = S_RelayManager.instance.GetAllocationId();
        string connectionData = S_RelayManager.instance.GetConnectionData();
        _localLobbyPlayerData.IsReady = false;
        await S_LobbyManager.instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);
        return true;
    }

    public E_PlayerTeam GetPlayerTeam()
    {
        return _localLobbyPlayerData.Team;
    }

    public async void LeaveLobby()
    {
        _inGame = false;
        _wasDisconnected = true;
        if (_wasDisconnected)
        {
            _previousRelayCode = _lobbyData.RelayJoinCode;
        }

        _localLobbyPlayerData.IsReady = false;
        await S_LobbyManager.instance.UpdateLobbyData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize());
    }

    public Task<E_PlayerTeam> GetPlayerTeamAsync()
    {
        return Task.FromResult(_localLobbyPlayerData.Team);
    }
}