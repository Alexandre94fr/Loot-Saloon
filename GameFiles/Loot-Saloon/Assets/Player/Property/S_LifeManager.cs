using System;
using UnityEngine;

public class S_LifeManager : MonoBehaviour
{
    private S_PlayerAttributes _playerAttributes;
    public static event Action OnDie;

    private void Awake()
    {
        //_playerAttributes = GameObject.Find("Attributes").GetComponent<S_PlayerAttributes>();
        _playerAttributes = GetComponent<S_PlayerAttributes>();
    }

    public void TakeDamage(float damageToTake)
    {
        _playerAttributes.Life -= damageToTake;
        print("test");
        CheckDie();
    }

    private void CheckDie()
    {
        if (_playerAttributes.Life <= 0)
        {
            _playerAttributes.Life = 0;
            OnDie?.Invoke();
        }
    }
    
    
}
