using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class S_SettingsMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button quitBtn;
    public Button resetBtn;

    [Header("Panels")]
    public GameObject audioPanel;
    public GameObject displayPanel;
    public GameObject controlsPanel;

    [Header("Audio")]
    float masterVolume;
    float SFXVolume;
    float ambienceVolume;
    float UIVolume;
    float musicVolume;

    [Header("Display")]
    int displayMode;

    [Header("Controls")]
    string moveForwardInput;
    string moveBackwardInput;
    string moveLeftInput;
    string moveRightInput;
    string InteractInput;

    private void Start()
    {
        quitBtn.onClick.AddListener(OnQuitClicked);
        resetBtn.onClick.AddListener(OnResetClicked);
    }

    private void OnQuitClicked()
    {
        
    }
    
    private void OnResetClicked()
    {
        
    }
}
