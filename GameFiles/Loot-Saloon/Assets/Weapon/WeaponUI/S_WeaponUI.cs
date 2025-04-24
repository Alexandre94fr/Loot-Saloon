using System;
using TMPro;
using UnityEngine;

public class S_WeaponUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentbulletsText;


    private void OnEnable()
    {
        S_WeaponSlot.OnBulletCountChanged += UpdateBulletUI;
        S_WeaponSlot.OnWeaponChanged += UpdateUiVisibility;
        UpdateUiVisibility(false, 0, 0);
    }

    private void OnDisable()
    {
        S_WeaponSlot.OnBulletCountChanged -= UpdateBulletUI;
        S_WeaponSlot.OnWeaponChanged -= UpdateUiVisibility;
    }

    private void UpdateBulletUI(int p_currentBullets, int p_maxBullets)
    {
        _currentbulletsText.text = $"{p_currentBullets} / {p_maxBullets}";
    }

    private void UpdateUiVisibility(bool p_isVisible, int p_currentBullets, int p_maxBullets)
    {
        UpdateBulletUI(p_currentBullets, p_maxBullets);
        _currentbulletsText.gameObject.SetActive(p_isVisible);
    }
}