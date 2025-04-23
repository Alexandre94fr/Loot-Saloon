using System;
using System.Collections;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LoadScreen : MonoBehaviour
{
    [SerializeField] private RectTransform _rectComponent;
    [SerializeField] private float _rotateSpeed = 200f;

    private void OnEnable()
    {
        S_LobbyEvents.OnLobbyUpdatedWithParam += OnLobbyLoaded;
    }

    private void OnLobbyLoaded(Lobby p_lobby)
    {
        if (p_lobby != null)
        {
            StartCoroutine(LoadScene());
        }
    }

    private IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
        S_LobbyEvents.OnLobbyUpdatedWithParam -= OnLobbyLoaded;
    }

    private void Update()
    {
        _rectComponent.Rotate(0f, 0f, _rotateSpeed * Time.deltaTime);
    }
}