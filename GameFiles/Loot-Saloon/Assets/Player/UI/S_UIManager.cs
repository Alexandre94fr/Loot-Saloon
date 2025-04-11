using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_UIManager : MonoBehaviour
{
    [SerializeField] private Canvas _playerCanvas;
    
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private Image _respawningCountdownImage;
    
    [SerializeField] private Image _finishMenuImage;
    
    

    private void Awake()
    {
        S_LifeManager.OnDie += StartRespawnCountdown;
        S_Extract.OnExtract += FinishMenu;
    }

    IEnumerator RespawnCountdown()
    {
        for (int i = 5; i > 0; i--)
        {
            _countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        _respawningCountdownImage.gameObject.SetActive(false); 

    }

    private void StartRespawnCountdown()
    {
        _respawningCountdownImage.gameObject.SetActive(true); 
        StartCoroutine(RespawnCountdown());
    }

    private void FinishMenu()
    {
        _finishMenuImage.gameObject.SetActive(true);
    }
}
