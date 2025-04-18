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
    
    [SerializeField] private E_PlayerTeam _team;
    public static event Action<E_PlayerTeam> OnExtract;
    public static event Action<E_PlayerTeam, int> GetQuota;

    private int _totalEntityInExract = 0;
    private bool _cartInExtract = false;

    private string _quotaText;
    
    S_Quota quotaComponent;

    private void Awake()
    {
        quotaComponent = GetComponent<S_Quota>();
        MoneyRequiredText.text = "-";

        OnExtract += (winner) => GetQuota.Invoke(_team, quotaComponent.quota);
        quotaComponent.OnQuotaChanged += () => {
            _quotaText = "{0} - " + quotaComponent.quota + " $";
            MoneyRequiredText.text = string.Format(_quotaText, 0);
        };
    }

    private void Start()
    {
        S_GameTimer.OnEnd += () => OnExtract?.Invoke(E_PlayerTeam.NONE);
    }

    private void Update()
    {
        if (_canExtract)
        {
            timer += Time.deltaTime;
            if (timer >= TimeToExtract)
            {
                OnExtract?.Invoke(_team);
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
            if (!_cartInExtract && other.TryGetComponent(out S_Cart cart) && cart.team == _team)
            {
                print("quota: " + quotaComponent.quota);
                MoneyRequiredText.text = string.Format(_quotaText, cart.total, quotaComponent.quota);
                if (quotaComponent.quota <= cart.total)
                {
                    print("cart in extract");
                    _totalEntityInExract++;
                    _cartInExtract = true;
                }
            }
        }

        else if (other.gameObject.CompareTag("Player"))
        {
            _totalEntityInExract++;
            print("player in extract");
        }

        if (_totalEntityInExract >= 2 && _cartInExtract)
        {
            _canExtract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            if (_cartInExtract && other.TryGetComponent(out S_Cart cart) && cart.team == _team)
            {
                _cartInExtract = false;
                _totalEntityInExract--;
                MoneyRequiredText.text = string.Format(_quotaText, 0, quotaComponent.quota);
            }
        }

        else if (other.gameObject.CompareTag("Player")) 
            _totalEntityInExract--;

        if (_totalEntityInExract < 2 || !_cartInExtract)
        {
            _canExtract = false;
        }
    }
}