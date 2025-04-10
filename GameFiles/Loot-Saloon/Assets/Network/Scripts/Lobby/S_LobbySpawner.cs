using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class S_LobbySpawner : MonoBehaviour
{
    [SerializeField] private List<S_LobbyPlayer> _players;

    private void OnEnable()
    {
        S_LobbyEvents.onLobbyUpdated += OnLobbyUpdated;
    }

    private void OnDisable()
    {
        S_LobbyEvents.onLobbyUpdated -= OnLobbyUpdated;
    }

    private void OnLobbyUpdated(Lobby p_lobby)
    {
        List<S_LobbyPlayerData> playersData = S_GameLobbyManager.instance.GetPlayers();

        for(int i = 0; i < playersData.Count; i++)
        {
            S_LobbyPlayerData playerData = playersData[i];
            _players[i].SetData(playerData);
        }
    }
}