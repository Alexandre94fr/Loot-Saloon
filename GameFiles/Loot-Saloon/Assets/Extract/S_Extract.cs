using System;
using TMPro;
using UnityEngine;

public class S_Extract : MonoBehaviour
{
    public float TimeToExtract;
    private bool _canExtract;
    private float timer;
    public TextMeshProUGUI MoneyRequiredText;
    public TextMeshProUGUI TimeToExractText;
    
    public static event Action OnExtract;

    private int _totalEntityInExract = 0;
    private bool _cartInExtract = false;
    
    S_Quota quotaComponent;

    private void Awake()
    {
        quotaComponent = GetComponent<S_Quota>();
        MoneyRequiredText.text = quotaComponent.quota + " $";
    }

    private void Update()
    {
        //print(_totalEntityInExract);
        if (_canExtract)
        {
            timer += Time.deltaTime;
            if (timer >= TimeToExtract)
            {
                OnExtract?.Invoke();
                _canExtract = false;
            }
        }
        else
        {
            timer = 0;
        }
        TimeToExractText.text = (Mathf.Floor(TimeToExtract) - Mathf.Floor(timer)).ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            if (_cartInExtract == false && other.TryGetComponent<S_Cart>(out S_Cart cart))
            {
                if (quotaComponent.quota - cart.total <= 0)
                {
                    _totalEntityInExract++;
                    _cartInExtract = true;
                }
            }
        }
        if (other.gameObject.CompareTag("Player")) _totalEntityInExract++;
        if (_totalEntityInExract >= 2 && _cartInExtract)
        {
            _canExtract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            if (_cartInExtract && other.TryGetComponent<S_Cart>(out S_Cart cart))
            {
                _cartInExtract = false;
                _totalEntityInExract--;
            }
        }
        else if (other.gameObject.CompareTag("Player")) _totalEntityInExract--;
        if (_totalEntityInExract < 2 || !_cartInExtract)
        {
            _canExtract = false;
        }
    }
}
