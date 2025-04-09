using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class S_MainMenuManagerTestNetwork : MonoBehaviour
{
    public Button hostBtn;
    public Button joinBtn;
    public string sceneToLoad;
    public InputField codeInputField;

    private void Start()
    {
        hostBtn.onClick.AddListener(OnHostButtonClicked);
        joinBtn.onClick.AddListener(OnJoinButtonClicked);
    }

    private async void OnHostButtonClicked()
    {
        Debug.Log("Host button clicked.");
        bool succeeded = await S_GameLobbyManager.instance.CreateLobby();
        if(succeeded)
        {
            //TODO : Change Scene To Load
            await SceneManager.LoadSceneAsync(sceneToLoad);
            Debug.Log("Scene Switched");
        }
        else
        {
            Debug.LogError("Failed to create lobby.");
        }
    }
    private async void OnJoinButtonClicked()
    {
        Debug.Log("Join button clicked.");
        string code = codeInputField.text.Trim();
        bool succedeed = await S_GameLobbyManager.instance.JoinLobby(code);
        if (succedeed)
        {
            await SceneManager.LoadSceneAsync(sceneToLoad);
        }
        else
        {
            Debug.LogError("Failed to join lobby.");
        }
    }
}
