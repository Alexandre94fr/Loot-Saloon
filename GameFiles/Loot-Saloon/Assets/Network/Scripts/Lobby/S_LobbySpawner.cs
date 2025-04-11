#region
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
#endregion

public class S_LobbySpawner : MonoBehaviour
{
    [SerializeField] private List<S_LobbyPlayer> _players;

    private void OnEnable()
    {
        S_LobbyEvents.OnLobbyUpdatedWithParam += OnLobbyUpdated;
    }

    private void OnDisable()
    {
        S_LobbyEvents.OnLobbyUpdatedWithParam -= OnLobbyUpdated;
    }

    private void OnLobbyUpdated(Lobby p_lobby)
    {
        List<S_LobbyPlayerData> playersData = S_GameLobbyManager.instance.GetPlayers();

        for (int i = 0; i < playersData.Count; i++)
        {
            S_LobbyPlayerData playerData = playersData[i];
            _players[i].SetData(playerData);
        }
    }
}