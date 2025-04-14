#region
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
#endregion

public class S_LobbySpawner : MonoBehaviour
{
    [SerializeField] private List<S_LobbyPlayer> _players;
    public Material RedMaterial;
    public Material BlueMaterial;

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
            if (playerData.Team == E_PlayerTeam.RED)
            {
                _players[i].GetComponent<MeshRenderer>().material = RedMaterial;
            }
            else if (playerData.Team == E_PlayerTeam.BLUE)
            {
                _players[i].GetComponent<MeshRenderer>().material = BlueMaterial;
            }
        }
    }
}