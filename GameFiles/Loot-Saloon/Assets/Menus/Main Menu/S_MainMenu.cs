using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class S_MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button hostBtn;
    public Button joinBtn;
    public Button refreshListBtn;
    public Button quitBtn;

    public Button playHostBn;
    public Button returnFromHostBtn;

    public Button playGuestBtn;

    [Header("Scene settings")]
    public string sceneToLoad;
    public InputField codeInputField;

    [Header("Show Lobbies Panel")]
    public GameObject showLobbiesPanel;
    public RectTransform content;
    public GameObject lobbyButtonPrefab;

    public GameObject createLobbyPanel;

    private void Start()
    {
        hostBtn.onClick.AddListener(OnPlayCreatedClicked);
        joinBtn.onClick.AddListener(() =>
        {
            OnJoinButtonClicked();
        });
        refreshListBtn.onClick.AddListener(ShowLobbies);

        quitBtn.onClick.AddListener(OnQuitButtonClicked);

        playHostBn.onClick.AddListener(OnHostButtonClicked);
        returnFromHostBtn.onClick.AddListener(OnReturnClicked);

        playGuestBtn.onClick.AddListener(OnPlayGuestClicked);
    }

    private void OnPlayCreatedClicked()
    {
        createLobbyPanel.SetActive(true);
    }

    private void OnReturnClicked()
    {
        createLobbyPanel.SetActive(false);
    }

    private async void OnHostButtonClicked()
    {
        bool succeeded = await S_GameLobbyManager.instance.CreateLobby();
        if (succeeded)
        {
            await SceneManager.LoadSceneAsync(sceneToLoad);
        }
        else
        {
            Debug.LogError("Failed to create lobby.");
        }
    }

    private void OnJoinButtonClicked()
    {
        showLobbiesPanel.SetActive(true);
        ShowLobbies();
    }

    private async void ShowLobbies()
    {
        QueryResponse queryResponse = await S_LobbyManager.instance.QueryLobbiesAsync();

        // Clear previous lobby items
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        if (queryResponse.Results.Count == 0)
        {
            return;
        }
        foreach (Lobby lobby in queryResponse.Results)
        {
            GameObject lobbyItem = Instantiate(lobbyButtonPrefab, content);
            lobbyItem.GetComponentInChildren<Text>().text = lobby.Name;
            lobbyItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                JoinPublicLobby(lobby.Id);
                Debug.Log(lobby.Id);
            });
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, (lobbyButtonPrefab.GetComponent<RectTransform>().sizeDelta.y
                                                              + content.GetComponent<VerticalLayoutGroup>().spacing) * queryResponse.Results.Count);
        // Wait for a while before refreshing
        await System.Threading.Tasks.Task.Delay(5000);
    }

    private async void JoinPublicLobby(string p_lobbyId)
    {
        bool succedeed = await S_GameLobbyManager.instance.JoinLobbyById(p_lobbyId);
        if (succedeed)
        {
            await SceneManager.LoadSceneAsync(sceneToLoad);
        }
        else
        {
            Debug.LogError("Failed to join lobby.");
        }
    }

    private async void OnPlayGuestClicked()
    {
        Debug.Log("Join button clicked.");
        string code = codeInputField.text.Trim();
        bool succedeed = await S_GameLobbyManager.instance.JoinLobby(code);
        if (succedeed)
        {
            await SceneManager.LoadSceneAsync("Lobby");
        }
        else
        {
            Debug.LogError("Failed to join lobby.");
        }
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}