#region
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#endregion

public class S_LobbyUINetworkTest : MonoBehaviour
{
    public Text lobbyIdText;
    public Button startGameButton;
    public Button readyButton;
    public Button LeaveButton;

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

            LeaveButton.onClick.AddListener(HandleHostDisconnection);
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

        PlayerPrefs.SetInt("NbrOfPlayer", S_GameLobbyManager.instance.LobbyPlayerDatas.Count);

        if (S_GameLobbyManager.instance == null)
        {
            Debug.LogError("S_GameLobbyManager.instance is null !");
            return;
        }

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
        if (await S_GameLobbyManager.instance.GetPlayerTeamAsync() != E_PlayerTeam.NONE)
        {
            var succeeded = await S_GameLobbyManager.instance.SetPlayerReady();
            if (succeeded) readyButton.interactable = false;
        }
    }
    
    private async void OnBlueBtnPressed()
    {
        if (await S_GameLobbyManager.instance.GetPlayerTeamAsync() != E_PlayerTeam.NONE)
        {
            var succeeded1 = await S_GameLobbyManager.instance.SetPlayerUnready();
            if (succeeded1)
            {
                readyButton.interactable = true;
                startGameButton.gameObject.SetActive(false);
            }

        }
        var succeeded = await S_GameLobbyManager.instance.SetPlayerTeam(E_PlayerTeam.BLUE);
    }
    
    private async void OnRedBtnPressed()
    {
        if (await S_GameLobbyManager.instance.GetPlayerTeamAsync() != E_PlayerTeam.NONE)
        {
            var succeeded1 = await S_GameLobbyManager.instance.SetPlayerUnready();
            if (succeeded1)
            {
                readyButton.interactable = true;
                startGameButton.gameObject.SetActive(false);
            }

        }
        var succeeded = await S_GameLobbyManager.instance.SetPlayerTeam(E_PlayerTeam.RED);
    }

    private async void HandleHostDisconnection()
    {
        try
        {
            await S_LobbyManager.instance.LeaveLobbyAsync();
            Debug.Log("Player has left the lobby.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while leaving the lobby: {ex.Message}");
        }
        finally
        {
            await SceneManager.LoadSceneAsync("MainMenu");
        }
    }
}