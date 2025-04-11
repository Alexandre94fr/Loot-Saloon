#region
using UnityEngine;
using UnityEngine.UI;
#endregion

public class S_LobbyUINetworkTest : MonoBehaviour
{
    public Text lobbyIdText;
    public Button startGameButton;
    public Button readyButton;


    private void OnEnable()
    {
        if (S_GameLobbyManager.instance)
        {
            lobbyIdText.text = "Lobby ID: " + S_GameLobbyManager.instance.GetLobbyCode();
            Debug.Log(S_GameLobbyManager.instance.IsHost ? "Host" : "Guest");
            if (S_GameLobbyManager.instance.IsHost)
            {
                S_LobbyEvents.OnLobbyReady += OnLobbyReady;
                startGameButton.onClick.AddListener(OnStartButtonClicked);
            }

            readyButton.onClick.AddListener(OnReadyPressed);
        }
    }

    private void OnDisable()
    {
        S_LobbyEvents.OnLobbyReady -= OnLobbyReady;
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

    private async void OnReadyPressed()
    {
        var succeeded = await S_GameLobbyManager.instance.SetPlayerReady();
        if (succeeded) readyButton.interactable = false;
    }
}