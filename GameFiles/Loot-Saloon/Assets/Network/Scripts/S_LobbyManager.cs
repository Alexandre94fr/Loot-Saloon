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

    public async Task<bool> CreateLobbyAsync(int maxPlayer, bool isPrivate, Dictionary<string, string> data)
    {
        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
        Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);
        CreateLobbyOptions lobbyOption = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = player,
        };

        try
        {
            _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", maxPlayer, lobbyOption);
        }
        catch (Exception)
        {
            return false;
        }

        Debug.Log("Lobby created successfully with id " + _lobby.Id);
        _heartbeatCoroutine = StartCoroutine(HearthbeatLobbyCoroutine(_lobby.Id, 6f));
        _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
        return true;
    }

    private IEnumerator HearthbeatLobbyCoroutine(string id, float waitTimeSeconds)
    {
        while (true)
        {
            Debug.Log("Sending heartbeat to lobby with id " + id);
            LobbyService.Instance.SendHeartbeatPingAsync(id);
            yield return new WaitForSecondsRealtime(waitTimeSeconds);
        }
    }

    private IEnumerator RefreshLobbyCoroutine(string id, float waitTimeSeconds)
    {
        while (true)
        {
            Task<Lobby> task = LobbyService.Instance.GetLobbyAsync(id);
            yield return new WaitUntil(() => task.IsCompleted);
            Lobby newLobby = task.Result;
            if (newLobby.LastUpdated > _lobby.LastUpdated)
            {
                _lobby = newLobby;
                Debug.Log("Lobby updated successfully with id " + _lobby.Id);
            }

            yield return new WaitForSecondsRealtime(waitTimeSeconds);
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
            Debug.Log("Leaving lobby with id " + _lobby.Id);
            LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
        }
    }
}