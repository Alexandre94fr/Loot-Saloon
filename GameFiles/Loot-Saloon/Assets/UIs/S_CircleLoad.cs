using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class S_CircleLoad : MonoBehaviour
{
    public static Action<float> OnCircleChange;

    [SerializeField] private Image _image;
    private float _timerValue;

    private void Awake()
    {
        OnCircleChange += SetLoader;
        SetLoader(0);
    }
    public void SetLoader(float percentage)
    {
        percentage = Mathf.Clamp01(percentage);

       _image.fillAmount = percentage;
    }
}
