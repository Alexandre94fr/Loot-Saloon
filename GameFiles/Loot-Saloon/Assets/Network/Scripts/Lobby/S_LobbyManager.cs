#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
#endregion

public class S_LobbyManager : MonoBehaviour
{
    public static S_LobbyManager instance;

    private Lobby _lobby;
    private Coroutine _heartbeatCoroutine;
    private Coroutine _refreshLobbyCoroutine;

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

    public string GetLobbyCode()
    {
        return _lobby?.LobbyCode;
    }

    public void OnApplicationQuit()
    {
        if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
            LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
    }


    public async Task<bool> CreateLobbyAsync(int p_maxPlayer, bool p_isPrivate, Dictionary<string, string> p_data, Dictionary<string, string> p_lobbyData)
    {
        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(p_data);
        Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);

        CreateLobbyOptions lobbyOption = new CreateLobbyOptions
        {
            Data = SerializeLobbyData(p_lobbyData),
            IsPrivate = p_isPrivate,
            Player = player
        };

        try
        {
            _lobby = await LobbyService.Instance.CreateLobbyAsync($"{PlayerPrefs.GetString("PlayerName")}'s Lobby", p_maxPlayer, lobbyOption);
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
        while (true)
        {
            Task<Lobby> task = LobbyService.Instance.GetLobbyAsync(p_id);
            yield return new WaitUntil(() => task.IsCompleted);
            Lobby newLobby = task.Result;
            if (newLobby.LastUpdated > _lobby.LastUpdated)
            {
                _lobby = newLobby;
                S_LobbyEvents.OnLobbyUpdatedWithParam(_lobby);
            }


            yield return new WaitForSecondsRealtime(p_waitTimeSeconds);
        }
    }

    private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> p_data)
    {
        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();
        foreach (KeyValuePair<string, string> item in p_data)
            playerData.Add(item.Key, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, item.Value));

        return playerData;
    }

    private Dictionary<string, DataObject> SerializeLobbyData(Dictionary<string, string> p_lobbyData)
    {
        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>();
        foreach (KeyValuePair<string, string> item in p_lobbyData)
            lobbyData.Add(item.Key, new DataObject(DataObject.VisibilityOptions.Member, item.Value));

        return lobbyData;
    }

    public async Task<bool> JoinLobby(string p_code, Dictionary<string, string> p_playerData)
    {
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(p_playerData))
        };

        try
        {
            _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(p_code, options);
        }
        catch (Exception)
        {
            return false;
        }

        StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
        return true;
    }

    public async Task<bool> JoinLobbyById(string p_id, Dictionary<string, string> p_playerData)
    {
        JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();
        Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(p_playerData));

        options.Player = player;

        try
        {
            _lobby = await LobbyService.Instance.JoinLobbyByIdAsync(p_id, options);
        }
        catch (Exception)
        {
            return false;
        }

        StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
        return true;
    }

    public List<Dictionary<string, PlayerDataObject>> GetPlayerData()
    {
        List<Dictionary<string, PlayerDataObject>> data = new List<Dictionary<string, PlayerDataObject>>();
        foreach (Player player in _lobby.Players)
            data.Add(player.Data);

        return data;
    }

    public async Task<bool> UpdatePlayerData(string p_id, Dictionary<string, string> p_data, string p_allocationId = default, string p_connectionData = default)
    {
        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(p_data);

        UpdatePlayerOptions options = new UpdatePlayerOptions
        {
            Data = playerData,
            AllocationId = p_allocationId,
            ConnectionInfo = p_connectionData
        };
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, p_id, options);
        }
        catch (Exception)
        {
            return false;
        }

        S_LobbyEvents.OnLobbyUpdatedWithParam(_lobby);
        return true;
    }

    public async Task<bool> UpdateLobbyData(Dictionary<string, string> p_data)
    {
        Dictionary<string, DataObject> lobbyData = SerializeLobbyData(p_data);
        UpdateLobbyOptions options = new UpdateLobbyOptions
        {
            Data = lobbyData
        };

        try
        {
            _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, options);
        }
        catch (Exception)
        {
            return false;
        }

        S_LobbyEvents.OnLobbyUpdatedWithParam(_lobby);
        return true;
    }

    public string GetHostId()
    {
        if (_lobby != null)
            return _lobby.HostId;

        return string.Empty;
    }

    public async Task<QueryResponse> QueryLobbiesAsync()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(
                        QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"),
                    new QueryFilter(
                        QueryFilter.FieldOptions.IsLocked,
                        op: QueryFilter.OpOptions.EQ,
                        value: "0")
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(
                        field: QueryOrder.FieldOptions.Created,
                        asc: false)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
            return queryResponse;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to query lobbies: {ex.Message}");
            return null;
        }
    }
}