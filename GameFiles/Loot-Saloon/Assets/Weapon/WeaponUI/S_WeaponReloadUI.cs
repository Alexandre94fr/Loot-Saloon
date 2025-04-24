using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class S_WeaponReloadUI : MonoBehaviour
{
    [Header(" External references :")]
    [SerializeField] private S_WeaponSlot _weaponSlot;

    [Header(" Internal references :")]
    [SerializeField] private GameObject _reloadUIGameObject;
    [SerializeField] private Image _reloadFillBarImage;

    void Start()
    {
        if (!S_VariablesChecker.AreVariablesCorrectlySetted(name, null,
            (_weaponSlot, nameof(_weaponSlot)),
            (_reloadUIGameObject, nameof(_reloadUIGameObject)),
            (_reloadFillBarImage, nameof(_reloadFillBarImage))
        )) return;

        _weaponSlot.OnWeaponReloadEvent += ShowUpdatedUI;

        _reloadUIGameObject.SetActive(false);
    }

    void ShowUpdatedUI(float p_reloadingTimeInSeconds)
    {
        _reloadUIGameObject.SetActive(true);

        StartCoroutine(StartReloadAnimation(p_reloadingTimeInSeconds));
    }

    IEnumerator StartReloadAnimation(float p_reloadingTimeInSeconds)
    {
        float timeElapsed = 0f;

        _reloadFillBarImage.fillAmount = 0f;
        _reloadUIGameObject.SetActive(true);

        while (timeElapsed < p_reloadingTimeInSeconds)
        {
            timeElapsed += Time.deltaTime;

            _reloadFillBarImage.fillAmount = Mathf.Clamp01(timeElapsed / p_reloadingTimeInSeconds);

            yield return null;
        }

        // In case it's not full
        _reloadFillBarImage.fillAmount = 1f;

        _reloadUIGameObject.SetActive(false);
    }
}