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
    public Button leaveButton;

    public Button goRedTeamButton;
    public Button goBlueTeamButton;

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

            leaveButton.onClick.AddListener(HandleHostDisconnection);
            readyButton.onClick.AddListener(OnReadyPressed);
            goRedTeamButton.onClick.AddListener(()=>OnTeamBtnPressed(E_PlayerTeam.RED));
            goBlueTeamButton.onClick.AddListener(()=>OnTeamBtnPressed(E_PlayerTeam.BLUE));
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
        
        startGameButton.interactable = false;
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

    private async void OnTeamBtnPressed(E_PlayerTeam p_choosedTeam)
    {
        readyButton.gameObject.SetActive(true);
        if (p_choosedTeam == E_PlayerTeam.RED)
        {
            goBlueTeamButton.interactable = true;
            goRedTeamButton.interactable = false;
        }
        else if (p_choosedTeam == E_PlayerTeam.BLUE)
        {
            goRedTeamButton.interactable = true;
            goBlueTeamButton.interactable = false;
        }

        if (await S_GameLobbyManager.instance.GetPlayerTeamAsync() != E_PlayerTeam.NONE)
        {
            var succeeded1 = await S_GameLobbyManager.instance.SetPlayerUnready();
            if (succeeded1)
            {
                readyButton.interactable = true;
                startGameButton.gameObject.SetActive(false);
            }

        }
        var succeeded = await S_GameLobbyManager.instance.SetPlayerTeam(p_choosedTeam);
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