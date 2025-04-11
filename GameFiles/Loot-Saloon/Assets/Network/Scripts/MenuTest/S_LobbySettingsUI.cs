using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_LobbySettingsUI : MonoBehaviour
{

    public Slider maxPlayersSlider;
    public TextMeshProUGUI maxPlayersText;
    public Toggle privateToggle;
    private S_LobbySettings _lobbySettings = new S_LobbySettings();

    private void Start()
    {
        maxPlayersSlider.onValueChanged.AddListener(OnMaxPlayersSliderChanged);
        privateToggle.onValueChanged.AddListener(OnPrivateToggleChanged);

    }

    private void OnPrivateToggleChanged(bool p_value)
    {
        _lobbySettings.isPrivate = p_value;
        S_GameLobbyManager.instance.SetLobbySettings(_lobbySettings);
    }

    private void OnMaxPlayersSliderChanged(float p_value)
    {
        int maxPlayers = Mathf.RoundToInt(p_value);
        if (maxPlayers % 2 != 0)
            maxPlayers--;
        maxPlayersSlider.value = maxPlayers;
        maxPlayersText.text = maxPlayers.ToString();
        _lobbySettings.maxPlayers = maxPlayers;
        S_GameLobbyManager.instance.SetLobbySettings(_lobbySettings);
    }
}