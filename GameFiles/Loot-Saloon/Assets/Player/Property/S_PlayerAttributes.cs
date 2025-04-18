using System;
using UnityEngine;
using UnityEngine.Serialization;

public class S_PlayerAttributes : MonoBehaviour
{
    public S_PlayerProperties properties;
    public float speed;
    public float life;
    public float maxLife;
    public float strengh;
    public E_PlayerTeam team;

    private void Awake()
    {
        ResetStat();
        S_LifeManager.OnDie += (attributes) => {
            if (attributes == this)
                ResetStat();
        };
    }

    public void SetTeam(E_PlayerTeam p_team)
    {
        this.team = p_team;
    }
    private void ResetStat()
    {
        speed = properties.speed;
        life = properties.life;
        maxLife = life;
        strengh = properties.strengh;
    }

}

public enum E_PlayerTeam
{
    NONE,
    BLUE,
    RED
}