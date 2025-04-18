using System;
using System.Collections;
using UnityEngine;

public class S_GameTimer : MonoBehaviour
{
    public static Action OnStart;
    public static Func<bool> IsRunning;
    public static Func<float> GetTimer;
    public static Action<float> UpdateTimer;
    public static Func<float> GetCountdownTimer;
    public static Action OnPause;
    public static Action OnReset;
    public static Action OnEnd;

    private float _timer;
    private bool _isRunning = false;
    private float _gameMaxTime = 20 * 60;
    private void Awake()
    {
        OnStart += () => StartCoroutine(StartTimer());
        IsRunning += () => { return _isRunning;};
        OnPause += () => StopTimer();
        OnReset += () => ResetTimer();
        GetTimer += () => { return _timer; };
        GetCountdownTimer += () => { return _gameMaxTime - _timer; };
    }

    private void Start()
    {
        OnStart();
    }

    private IEnumerator StartTimer() 
    { 
        if (_isRunning)
            yield break;

        _isRunning = true;

        while (_isRunning) 
        {
            _timer += Time.deltaTime;
            UpdateTimer?.Invoke(_timer/_gameMaxTime);
            if (_gameMaxTime - _timer <= 0.0f)
            {
                OnEnd?.Invoke();
            }
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
