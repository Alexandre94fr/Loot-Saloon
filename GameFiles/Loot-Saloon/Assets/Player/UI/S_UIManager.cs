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

    private void Awake()
    {
        S_LifeManager.OnDie += StartRespawnCountdown;
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
}
