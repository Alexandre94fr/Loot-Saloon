using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadScreen : MonoBehaviour
{
    [SerializeField] private Image _fillerLoadingBar;
    [SerializeField][Range(1.5f,4)] private float _loadingTime = 2f;


    private void Awake()
    {
        StartCoroutine(LoadingBarIncrement());
    }

    private IEnumerator LoadingBarIncrement()
    {
        float elapsedTime = 0f;
        while (elapsedTime < _loadingTime)
        {
            elapsedTime += Time.deltaTime;
            _fillerLoadingBar.fillAmount = Mathf.Clamp01(elapsedTime / _loadingTime);
            yield return null;
        }
        gameObject.SetActive(false);
        Debug.Log("Loading complete!");
    }
}
