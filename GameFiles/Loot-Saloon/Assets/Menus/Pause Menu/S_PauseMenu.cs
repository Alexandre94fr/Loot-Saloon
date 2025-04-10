using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class S_PauseMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button resumeBtn;
    public Button settingsBtn;
    public Button quitBtn;

    [Header("Panel")]
    public GameObject pausePanel;

    public string sceneToLoad;

    private void Start()
    {
        resumeBtn.onClick.AddListener(OnResumeClicked);
        settingsBtn.onClick.AddListener(OnSettingsClicked);
        quitBtn.onClick.AddListener(OnQuitClicked);
    }

    private void OnResumeClicked()
    {
        pausePanel.SetActive(false);
    }

    private void OnSettingsClicked()
    {

    }

    private void OnQuitClicked()
    {
        SceneManager.LoadSceneAsync(sceneToLoad);
    }

}
    
