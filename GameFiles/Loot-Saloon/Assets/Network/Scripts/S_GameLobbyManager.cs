using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class S_GameLobbyManager : MonoBehaviour
{
    public static S_GameLobbyManager instance;
    private List<S_LobbyPlayerData> _lobbyPlayerDatas = new List<S_LobbyPlayerData>();
    private S_LobbyPlayerData _localPlayerData;
    private S_LobbySettings _lobbySettings = new S_LobbySettings();

    private void OnEnable()
    {
        S_LobbyEvents.onLobbyUpdated += OnLobbyUpdated;
    }


    private void OnDisable()
    {
        S_LobbyEvents.onLobbyUpdated -= OnLobbyUpdated;

    }

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

    public async Task<bool> CreateLobby()
    {
        S_LobbyPlayerData playerData = new S_LobbyPlayerData();
        playerData.Initialize(AuthenticationService.Instance.PlayerId,  PlayerPrefs.GetString("PlayerName", "Host"));
        bool succeeded = await S_LobbyManager.instance.CreateLobbyAsync(_lobbySettings.maxPlayers, _lobbySettings.isPrivate, playerData.Serialize());
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

    private void OnLobbyUpdated(Lobby p_lobby)
    {
        List<Dictionary<string,PlayerDataObject>> playerData = S_LobbyManager.instance.GetPlayerData();
        _lobbyPlayerDatas.Clear();
        foreach (Dictionary<string,PlayerDataObject> data in playerData)
        {
            S_LobbyPlayerData lobbyPlayerData = new S_LobbyPlayerData();
            lobbyPlayerData.Initialize(data);

            if(lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
            {
                _localPlayerData = lobbyPlayerData;
            }
            _lobbyPlayerDatas.Add(lobbyPlayerData);
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
}
