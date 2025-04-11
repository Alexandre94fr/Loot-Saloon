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
    public void SetLoader(float p_percentage)
    {
        p_percentage = Mathf.Clamp01(p_percentage);

       _image.fillAmount = p_percentage;
    }
}
