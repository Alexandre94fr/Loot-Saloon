using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class S_GameTimer : MonoBehaviour
{
    public static Action OnStart;
    public static Func<bool> IsRunning;
    public static Func<float> GetTimer;
    public static Action OnPause;
    public static Action OnReset;

    private float _timer;
    private bool _isRunning = false;

    private void Awake()
    {
        OnStart += () => StartCoroutine(StartTimer());
        IsRunning += () => { return _isRunning;};
        OnPause += () => StopTimer();
        OnReset += () => ResetTimer();
        GetTimer += () => { return _timer; };
    }

    private IEnumerator StartTimer() 
    { 
        if (_isRunning)
            yield break;

        _isRunning = true;

        while (_isRunning) 
        {
            _timer += Time.deltaTime;
            yield return null;
        }

    }

    private void StopTimer()
    {
        _isRunning = false;
    }

    private void ResetTimer()
    {
        _timer = 0;
    }
}
