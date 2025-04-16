using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class S_LobbyPlayerData
{
    private string _id;
    private string _gamerTag;
    private bool _isReady;

    private GameObject _prefabInstance;
    private bool _kickPlayer;

    private E_PlayerTeam _team;

    public E_PlayerTeam Team
    {
        get => _team;
        set => _team = value;
    }

    public bool KickPlayer
    {
        get => _kickPlayer;
        set => _kickPlayer = value;
    }
    
    public string Id => _id;
    public string GamerTag => _gamerTag;

    public GameObject PrefabInstance
    {
        get => _prefabInstance;
        set => _prefabInstance = value;
    }
    public bool IsReady
    {
        get => _isReady;
        set => _isReady = value;
    }

    public void Initialize(string p_id, string p_gamerTag)
    {
        _id = p_id;
        _gamerTag = p_gamerTag;
        _isReady = false;
        _team = E_PlayerTeam.NONE;
    }

    public void Initialize(Dictionary<string,PlayerDataObject> p_playerData)
    {
        UpdateState(p_playerData);
    }

    public void UpdateState(Dictionary<string, PlayerDataObject> p_playerData)
    {
        if(p_playerData.ContainsKey("Id"))
        {
            _id = p_playerData["Id"].Value;
        }
        if(p_playerData.ContainsKey("GamerTag"))
        {
            _gamerTag = p_playerData["GamerTag"].Value;
        }
        if(p_playerData.ContainsKey("IsReady"))
        {
            _isReady = p_playerData["IsReady"].Value == "True";
        }
        if(p_playerData.ContainsKey("Team"))
        {
            if (p_playerData["Team"].Value == "BLUE") _team = E_PlayerTeam.BLUE;
            if (p_playerData["Team"].Value == "RED") _team = E_PlayerTeam.RED;
            if (p_playerData["Team"].Value == "NONE") _team = E_PlayerTeam.NONE;
        }
    }

    public Dictionary<string,string> Serialize()
    {
        return new Dictionary<string, string>()
        {
            { "Id", _id },
            { "GamerTag", _gamerTag },
            { "IsReady", _isReady.ToString() },
            { "Team", _team.ToString() },
        };
    }
}
