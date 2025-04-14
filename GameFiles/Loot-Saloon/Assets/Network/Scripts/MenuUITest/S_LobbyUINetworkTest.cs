#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#endregion

public class S_LobbyUINetworkTest : MonoBehaviour
{
    public Text lobbyIdText;
    public Button startGameButton;
    public Button readyButton;
    public Button GoRedTeamButton;
    public Button GoBlueTeamButton;

    private void OnEnable()
    {
        if (S_GameLobbyManager.instance)
        {
            lobbyIdText.text = "Lobby ID: " + S_GameLobbyManager.instance.GetLobbyCode();
            Debug.Log(S_GameLobbyManager.instance.IsHost ? "Host" : "Guest");
            if (S_GameLobbyManager.instance.IsHost)
            {
                S_LobbyEvents.OnLobbyReady += OnLobbyReady;
                S_LobbyEvents.OnLobbyUnready += OnLobbyUnready;
                startGameButton.onClick.AddListener(OnStartButtonClicked);
            }

            readyButton.onClick.AddListener(OnReadyPressed);
            GoRedTeamButton.onClick.AddListener(OnRedBtnPressed);
            GoBlueTeamButton.onClick.AddListener(OnBlueBtnPressed);
        }
    }

    private void OnDisable()
    {
        S_LobbyEvents.OnLobbyReady -= OnLobbyReady;
        S_LobbyEvents.OnLobbyUnready -= OnLobbyUnready;
        startGameButton.onClick.RemoveAllListeners();
    }


    private async void OnStartButtonClicked()
    {
        await S_GameLobbyManager.instance.StartGame();
    }

    private void OnLobbyReady()
    {
        startGameButton.gameObject.SetActive(true);
    }

    private void OnLobbyUnready()
    {
        startGameButton.gameObject.SetActive(false);
    }

    private async void OnReadyPressed()
    {
        if (await S_GameLobbyManager.instance.GetPlayerTeam() != E_PlayerTeam.NONE)
        {
            var succeeded = await S_GameLobbyManager.instance.SetPlayerReady();
            if (succeeded) readyButton.interactable = false;
        }
    }
    
    private async void OnBlueBtnPressed()
    {
        var succeeded = await S_GameLobbyManager.instance.SetPlayerTeam(E_PlayerTeam.BLUE);
    }
    
    private async void OnRedBtnPressed()
    {
        var succeeded = await S_GameLobbyManager.instance.SetPlayerTeam(E_PlayerTeam.RED);
    }
}