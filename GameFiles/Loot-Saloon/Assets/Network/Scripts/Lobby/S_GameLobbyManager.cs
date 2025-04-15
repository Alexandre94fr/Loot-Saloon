#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
#endregion

public class S_GameLobbyManager : MonoBehaviour
{
    public static S_GameLobbyManager instance;
    public string gameSceneName;

    private List<S_LobbyPlayerData> _lobbyPlayerDatas = new List<S_LobbyPlayerData>();
    private S_LobbySettings _lobbySettings = new S_LobbySettings();

    private S_LobbyPlayerData _localLobbyPlayerData;

    private S_LobbyData _lobbyData;
    [SerializeField] private bool _inGame;

    public bool IsHost => _localLobbyPlayerData != null && _localLobbyPlayerData.Id == S_LobbyManager.instance.GetHostId();


    private int _nbPlayersInBlueTeam = 0;
    private int _nbPlayersInRedTeam = 0;
    private bool _correctNumberinEachTeam = false;

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
        playerData.Initialize(AuthenticationService.Instance.PlayerId, PlayerPrefs.GetString("PlayerName", "Host"), 0);
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
        playerData.Initialize(AuthenticationService.Instance.PlayerId, PlayerPrefs.GetString("PlayerName", "Guest"), 0);
        bool succeeded = await S_LobbyManager.instance.JoinLobby(p_code, playerData.Serialize());
        return succeeded;
    }

    public async Task<bool> JoinLobbyById(string p_id)
    {
        S_LobbyPlayerData playerData = new S_LobbyPlayerData();
        playerData.Initialize(AuthenticationService.Instance.PlayerId, PlayerPrefs.GetString("PlayerName", "Guest"), 0);
        bool succeeded = await S_LobbyManager.instance.JoinLobbyById(p_id, playerData.Serialize());

        return succeeded;
    }

    private async void OnLobbyUpdated(Lobby p_lobby)
    {
        List<Dictionary<string, PlayerDataObject>> playerData = S_LobbyManager.instance.GetPlayerData();
        _lobbyPlayerDatas.Clear();
        int nbPlayerReady = 0;
        foreach (Dictionary<string, PlayerDataObject> data in playerData)
        {
            S_LobbyPlayerData lobbyPlayerData = new S_LobbyPlayerData();
            lobbyPlayerData.Initialize(data);


            if (lobbyPlayerData.IsReady)
            {
                nbPlayerReady++;
            }

            if (lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
            {
                _localLobbyPlayerData = lobbyPlayerData;
            }
            


            _lobbyPlayerDatas.Add(lobbyPlayerData);
        }

     

        _lobbyData = new S_LobbyData();
        _lobbyData.Initialize(p_lobby.Data);
        
        S_LobbyEvents.OnLobbyUpdated?.Invoke();

        Debug.Log($"nb player ready  : {nbPlayerReady} / {_lobbyPlayerDatas.Count}");
        if (nbPlayerReady == _lobbyPlayerDatas.Count)
        {
            S_LobbyEvents.OnLobbyReady?.Invoke();
        }
        else
        {
            S_LobbyEvents.OnLobbyUnready?.Invoke();
        }

        if (_lobbyData.RelayJoinCode != default && !_inGame && !IsHost)
        {
            Debug.Log($"OnLobbyUpdated called by player: {_localLobbyPlayerData?.Id ?? "Unknown"}");
            await JoinRelayServer(_lobbyData.RelayJoinCode);
            await SceneManager.LoadSceneAsync(gameSceneName);
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
    
    public async Task<bool> SetPlayerTeam(E_PlayerTeam p_team)
    {
        _localLobbyPlayerData.Team = p_team;
        return await S_LobbyManager.instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize());
    }

    public async Task StartGame()
    {
        string joinRelayCode = await S_RelayManager.instance.CreateRelay(_lobbySettings.maxPlayers);
        _inGame = true;
        _lobbyData.RelayJoinCode = joinRelayCode;
        await S_LobbyManager.instance.UpdateLobbyData(_lobbyData.Serialize());

        string allocationId = S_RelayManager.instance.GetAllocationId();
        string connectionData = S_RelayManager.instance.GetConnectionData();
        await S_LobbyManager.instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);

        await SceneManager.LoadSceneAsync(gameSceneName);
    }

    private async Task<bool> JoinRelayServer(string p_lobbyDataRelayJoinCode)
    {
        _inGame = true;
        await S_RelayManager.instance.JoinRelay(p_lobbyDataRelayJoinCode);
        string allocationId = S_RelayManager.instance.GetAllocationId();
        string connectionData = S_RelayManager.instance.GetConnectionData();
        await S_LobbyManager.instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);
        return true;
    }

    public E_PlayerTeam GetPlayerTeam()
    {
        return _localLobbyPlayerData.Team;
    }

    public async Task<E_PlayerTeam> GetPlayerTeamAsync()
    {
        return _localLobbyPlayerData.Team;
    }
}