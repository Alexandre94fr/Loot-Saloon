using System;
using Unity.Netcode;
using UnityEngine;

public class S_PlayersConnection : NetworkBehaviour
{
    public static S_PlayersConnection Instance { get; private set; }

    private int nbrOfPlayerReady = 0;

    public static event Action OnStartGame;
    
    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        PlayerIsReadyServerRpc();
    }

    public static void InvokePlayerReady()
    {
        Debug.LogWarning("Call my Event Player Ready");
        if (Instance != null)
        {
            Instance.PlayerIsReadyServerRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (Instance == this)
            Instance = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerIsReadyServerRpc()
    {
        nbrOfPlayerReady++;

        int nbrOfPlayer = PlayerPrefs.GetInt("NbrOfPlayer");

        Debug.Log($"Player Ready: {nbrOfPlayerReady}/{nbrOfPlayer}");

        if (nbrOfPlayerReady == nbrOfPlayer)
        {
            Debug.LogWarning("All players ready. Starting game...");
            StartGameClientRpc();
        }
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        Debug.LogWarning("Start Game!");
        OnStartGame?.Invoke();
    }
}
