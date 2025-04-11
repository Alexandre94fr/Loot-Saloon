using System;
using UnityEngine;
using UnityEngine.UI;

public class S_LobbyUINetworkTest : MonoBehaviour
{
    public Text lobbyIdText;

    private void Start()
    {
        lobbyIdText.text = "Lobby ID: " + S_GameLobbyManager.instance.GetLobbyCode();
    }
}
