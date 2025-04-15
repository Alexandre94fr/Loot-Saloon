using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class S_GameTimerTexts : MonoBehaviour
{
    public TextMeshProUGUI timer1Text;
    public TextMeshProUGUI timer2Text;
    public TextMeshProUGUI timer3Text;
    public TextMeshProUGUI timer4Text;

    private string _timer;

    private void Start()
    {
        S_GameTimer.OnStart();
    }

    private void Update()
    {
        var ts = TimeSpan.FromSeconds(S_GameTimer.GetCountdownTimer());
        string timer = $"{(int)ts.Minutes:00}:{ts.Seconds:00}";

        timer1Text.text = timer;
        timer2Text.text = timer;
        timer3Text.text = timer;
        timer4Text.text = timer;
    }
}