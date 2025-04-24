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
    [SerializeField] private bool _isInGame = false;
    [SerializeField] private float _maxWaitingTime = 30;
    private bool _gameStart = false;
    private float _startTime = 0;


    private void OnEnable()
    {
        if(!_isInGame)
        {
            S_LobbyEvents.OnLobbyUpdatedWithParam += OnLobbyLoaded;
            return;
        }
        S_PlayersConnection.OnStartGame += StartGame;
    }

    private void StartGame()
    {
        if (!_isInGame)
            return;
        StartCoroutine(LoadGame());
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

    private IEnumerator LoadGame()
    {
        while(!_gameStart && _startTime < _maxWaitingTime)
        {
            yield return new WaitForSecondsRealtime(1);
            _startTime += 1;
        }
        gameObject.SetActive(false);
    }

    private void Update()
    {
        _rectComponent.Rotate(0f, 0f, _rotateSpeed * Time.deltaTime);
    }
}