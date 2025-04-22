using System;
using UnityEngine;

public class S_PlayerShowTimer : MonoBehaviour
{
    public static Action<float> OnCircleChange;
    public S_CircleLoad loader;

    private void Awake()
    {
        S_GameTimer.UpdateTimer += loader.OnCircleChange;
    }
}