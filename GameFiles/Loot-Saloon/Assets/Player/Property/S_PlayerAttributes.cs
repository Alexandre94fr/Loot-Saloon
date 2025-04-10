using System;
using UnityEngine;

public class S_PlayerAttributes : MonoBehaviour
{
    public S_PlayerProperties properties;
    public float Speed;
    public float Life;
    public float MaxLife;
    public float Strengh;

    public void Initialize()
    {
        Speed = properties.Speed;
        Life = properties.Life;
        MaxLife = Life;
        Strengh = properties.Strengh;
    }

    private void Awake()
    {
        Initialize();
        S_LifeManager.OnDie += Initialize;
    }
}