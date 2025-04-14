using System;
using TMPro;
using UnityEngine;

public class S_GameTimerTexts : MonoBehaviour
{
    public TextMeshProUGUI Timer1Text;
    public TextMeshProUGUI Timer2Text;
    public TextMeshProUGUI Timer3Text;
    public TextMeshProUGUI Timer4Text;

    private string timer;

    private void Start()
    {
        S_GameTimer.OnStart();
    }

    private void Update()
    {
        var ts = TimeSpan.FromSeconds(S_GameTimer.GetCountdownTimer());
        string timer = $"{(int)ts.Minutes:00}:{ts.Seconds:00}";
        
        Timer1Text.text = timer;
        Timer2Text.text = timer;
        Timer3Text.text = timer;
        Timer4Text.text = timer;
        
    }
}
