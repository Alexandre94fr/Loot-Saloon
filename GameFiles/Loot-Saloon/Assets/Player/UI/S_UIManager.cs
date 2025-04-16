using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_UIManager : MonoBehaviour
{
    [SerializeField] private Canvas _playerCanvas;


    [SerializeField] private Image _finishMenuImage;

    #region Respawn Cool Down
    [SerializeField] private TextMeshProUGUI _countdownText;

    [SerializeField] private Image _respawningCountdownImage;
    [SerializeField] private int _timeBeforeRespawn = 5;
    #endregion


    #region Event
    private void OnEnable()
    {
        S_LifeManager.OnDie += StartRespawnCountdown;
    }

    private void OnDisable()
    {
        S_LifeManager.OnDie -= StartRespawnCountdown;
    }
    #endregion


    IEnumerator RespawnCountdown()
    {
        _respawningCountdownImage.gameObject.SetActive(true);
        for (int i = _timeBeforeRespawn; i > 0; i--)
        {
            _countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }
        _respawningCountdownImage.gameObject.SetActive(false); 
    }

    private void StartRespawnCountdown()
    {
        StartCoroutine(RespawnCountdown());
    }
}
