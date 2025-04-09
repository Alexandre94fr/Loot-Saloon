using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class S_LobbyManager : MonoBehaviour
{
    private Lobby _lobby;
    private Coroutine _heartbeatCoroutine;
    private Coroutine _refreshLobbyCoroutine;
    public static S_LobbyManager instance;

    private void Start()
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

    public string GetLobbyCode()
    {
        return _lobby?.LobbyCode;
    }

    private void OnDisable()
    {
        if (_heartbeatCoroutine != null)
        {
            StopCoroutine(_heartbeatCoroutine);
            _heartbeatCoroutine = null;
        }

        if (_refreshLobbyCoroutine != null)
        {
            StopCoroutine(_refreshLobbyCoroutine);
            _refreshLobbyCoroutine = null;
        }
    }

    public async Task<bool> CreateLobbyAsync(int p_maxPlayer, bool p_isPrivate, Dictionary<string, string> p_data)
    {
        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(p_data);
        Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);
        CreateLobbyOptions lobbyOption = new CreateLobbyOptions
        {
            IsPrivate = p_isPrivate,
            Player = player,
        };

        try
        {
            _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", p_maxPlayer, lobbyOption);
        }
        catch (Exception)
        {
            return false;
        }

        _heartbeatCoroutine = StartCoroutine(HearthbeatLobbyCoroutine(_lobby.Id, 6f));
        _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
        return true;
    }

    private IEnumerator HearthbeatLobbyCoroutine(string p_id, float p_waitTimeSeconds)
    {
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(p_id);
            yield return new WaitForSecondsRealtime(p_waitTimeSeconds);
        }
    }

    private IEnumerator RefreshLobbyCoroutine(string p_id, float p_waitTimeSeconds)
    {
        while (_lobby != null)
        {
            Task<Lobby> task = LobbyService.Instance.GetLobbyAsync(p_id);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Result != null && task.Result.LastUpdated > _lobby.LastUpdated)
            {
                _lobby = task.Result;
                S_LobbyEvents.onLobbyUpdated?.Invoke(_lobby);
            }

            yield return new WaitForSecondsRealtime(p_waitTimeSeconds);
        }
    }

    private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
    {
        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();
        foreach (var item in data)
        {
            playerData.Add(item.Key, new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member, value: item.Value));
        }

        return playerData;
    }

    public async Task<bool> JoinLobby(string code, Dictionary<string, string> playerData)
    {
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData))
        };


        try
        {
            _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);
        }
        catch (Exception)
        {
            return false;
        }

        StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
        return true;
    }

    public void OnApplicationQuit()
    {
        if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
        }
    }

    public List<Dictionary<string, PlayerDataObject>> GetPlayerData()
    {
        List<Dictionary<string, PlayerDataObject>> data = new List<Dictionary<string, PlayerDataObject>>();
        foreach (Player player in _lobby.Players)
        {
            data.Add(player.Data);
        }

        return data;
    }
}