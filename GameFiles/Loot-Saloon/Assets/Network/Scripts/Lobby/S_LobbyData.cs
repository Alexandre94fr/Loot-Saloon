#region
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
#endregion

public class S_LobbyData
{
    private string _relayJoinCode;

    public string RelayJoinCode
    {
        get => _relayJoinCode;
        set => _relayJoinCode = value;
    }


    public void Initialize(Dictionary<string, DataObject> p_lobbyData)
    {
        UpdateState(p_lobbyData);
    }

    public void UpdateState(Dictionary<string, DataObject> p_lobbyData)
    {
        if (p_lobbyData.ContainsKey("RelayJoinCode"))
        {
            _relayJoinCode = p_lobbyData["RelayJoinCode"].Value;
        }
        else
        {
            Debug.LogWarning("RelayJoinCode key not found in lobby data.");
        }
    }

    public Dictionary<string, string> Serialize()
    {
        return new Dictionary<string, string>()
        {
            { "RelayJoinCode", _relayJoinCode },
        };
    }
}