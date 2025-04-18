using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class S_PlayerShowTimer : MonoBehaviour
{
    public static Action<float> OnCircleChange;
    public S_CircleLoad loader;

    private void Awake()
    {
        S_GameTimer.UpdateTimer += loader.OnCircleChange;
    }

}
