using System;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Authentication;
using Steamworks;

public class S_NetworkInit : MonoBehaviour
{
    async void Start()
    {
        await UnityServices.InitializeAsync();

        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            AuthenticationService.Instance.SignedIn += OnSignedIn;

            if (AuthenticationService.Instance.IsSignedIn)
            {
                InitSteam();
            }
            else
            {
                throw new Exception("Failed to sign online.");
            }
        }
        else
        {
            throw new Exception("Failed to initialize UnityServices.");
        }

    }

    private void OnSignedIn()
    {
        Debug.Log("Signed in successfully. Player ID: " + AuthenticationService.Instance.PlayerId);
        Debug.Log("Signed in successfully. Player Token: " + AuthenticationService.Instance.AccessToken);
    }

    private void InitSteam()
    {
        if (!SteamAPI.Init())
        {
            throw new Exception("SteamAPI_Init() failed. Make sure Steam is running.");
        }

        SetPlayerNameBySteam();

        if (SteamAPI.RestartAppIfNecessary((AppId_t)480))
        {
            Application.Quit();
        }
    }
    private void SetPlayerNameBySteam()
    {
        string playerName = SteamFriends.GetPersonaName();
        PlayerPrefs.SetString("PlayerName", playerName);
    }
}