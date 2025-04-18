#region
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#endregion

public class S_LobbyUINetworkTest : MonoBehaviour
{
    public Button lobbyIdButton;
    public Button startGameButton;
    public Button readyButton;
    public Button leaveButton;

    public Button goRedTeamButton;
    public Button goBlueTeamButton;

    private void OnEnable()
    {
        if (S_GameLobbyManager.instance)
        {
            lobbyIdButton.GetComponentInChildren<TextMeshProUGUI>().text = S_GameLobbyManager.instance.GetLobbyCode();
            lobbyIdButton.onClick.AddListener(() => StartCoroutine(CopyLobbyIdToClipboard()));

            Debug.Log(S_GameLobbyManager.instance.IsHost ? "Host" : "Guest");
            if (S_GameLobbyManager.instance.IsHost)
            {
                S_LobbyEvents.OnLobbyReady += OnLobbyReady;
                S_LobbyEvents.OnLobbyUnready += OnLobbyUnready;
                startGameButton.onClick.AddListener(OnStartButtonClicked);
            }

            leaveButton.onClick.AddListener(() => S_GameLobbyManager.instance.HandleHostDisconnection());
            readyButton.onClick.AddListener(OnReadyPressed);
            goRedTeamButton.onClick.AddListener(() => OnTeamBtnPressed(E_PlayerTeam.RED));
            goBlueTeamButton.onClick.AddListener(() => OnTeamBtnPressed(E_PlayerTeam.BLUE));
        }
    }

    private void OnDisable()
    {
        S_LobbyEvents.OnLobbyReady -= OnLobbyReady;
        S_LobbyEvents.OnLobbyUnready -= OnLobbyUnready;
        startGameButton.onClick.RemoveAllListeners();
    }

    private IEnumerator CopyLobbyIdToClipboard()
    {
        TextMeshProUGUI lobbyIdText = lobbyIdButton.GetComponentInChildren<TextMeshProUGUI>();
        string lobbyId = lobbyIdText.text;

        GUIUtility.systemCopyBuffer = lobbyId;

        lobbyIdText.text = "Copied!";
        yield return new WaitForSeconds(2f);
        lobbyIdText.text = lobbyId;
    }

    private async void OnStartButtonClicked()
    {
        startGameButton.interactable = false;
        PlayerPrefs.SetInt("NbrOfPlayer", S_GameLobbyManager.instance.LobbyPlayerDatas.Count);
        if (S_GameLobbyManager.instance == null)
            return;

        await S_GameLobbyManager.instance.StartGame();
    }

    private async void OnLobbyReady()
    {
        if (await S_GameLobbyManager.instance.SameNbPlayerInEachTeam())
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

        // Mise à jour des boutons d'équipe
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
            var succeededUnready = await S_GameLobbyManager.instance.SetPlayerUnready();
            if (succeededUnready)
            {
                readyButton.interactable = true;
                startGameButton.gameObject.SetActive(false);
            }
        }

        var succeededTeamChange = await S_GameLobbyManager.instance.SetPlayerTeam(p_choosedTeam);

        S_LobbyEvents.OnLobbyUnready?.Invoke();
    }
}