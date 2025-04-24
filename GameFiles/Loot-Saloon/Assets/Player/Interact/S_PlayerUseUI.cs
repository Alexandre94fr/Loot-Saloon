using System;
using UnityEngine;

public class S_PlayerUseUI : MonoBehaviour
{
    public static Action<float> OnCircleChange;
    public S_CircleLoad loader;

    private void Awake()
    {
        OnCircleChange += (float p_value) => loader.OnCircleChange(p_value);
    }
}