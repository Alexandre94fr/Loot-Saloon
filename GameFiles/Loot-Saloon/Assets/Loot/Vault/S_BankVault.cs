using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;

public class S_BankVault : S_Interactable
{
    public S_LootInstantiator lootInstantiator;
    public Transform[] spawnPoints;
    public int moneyValue;

    private S_PlayerInteract _currentPlayer;
    private List<int> _lootIndeces = new List<int>();

    public float unlockTime = 6f;

    private bool _hasSpawnedLoot = false;

    public enum VaultState
    {
        Closed,
        InUse,
        Opened
    }

    public NetworkVariable<VaultState> vaultState = new NetworkVariable<VaultState>(VaultState.Closed,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);

    [ServerRpc(RequireOwnership = false)]
    private void SetVaultStateServerRPC(VaultState state)
    {
        vaultState.Value = state;
    }

    [ClientRpc]
    public void SetLootInstantiatorClientRpc(ulong lootInstantiatorNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(lootInstantiatorNetworkId, out var lootInstantiatorObject))
        {
            lootInstantiator = lootInstantiatorObject.GetComponent<S_LootInstantiator>();
        }
    }

    //public override void OnNetworkSpawn()
    //{
    //    if(Server)
    //    if (lootInstantiator == null)
    //    {
    //        Debug.LogError("lootInstantiator is not assigned on network spawn.");
    //        return;
    //    }
    //    GenerateLoots();
    //}

    public void GenerateLoots()
    {
        foreach (Transform point in spawnPoints)
        {
            int lootIndex = lootInstantiator.GetRandomLootPropertiesIndex(SO_LootProperties.Size.Medium);
            _lootIndeces.Add(lootIndex);
            moneyValue += lootInstantiator.GetLootPrice(lootIndex);
        }

        lootInstantiator.UpdateQuota(this);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnLootServerRpc()
    {
        SpawnLoot();
    }


    public void SpawnLoot()
    {
        //if (!IsServer || _hasSpawnedLoot) return;
        //_hasSpawnedLoot = true;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            lootInstantiator.SpawnLoot(_lootIndeces[i], spawnPoints[i]);
        }
    }

    IEnumerator UnlockSequence()
    {
        float timer = 0f;
        SetVaultStateServerRPC(VaultState.InUse);
        Debug.Log("Unlock Sequence Start");

        while (timer < unlockTime)
        {
            if (vaultState.Value != VaultState.InUse)
                yield break;
            Debug.Log("Opening the Vault .....");

            if (_currentPlayer == null)
                Debug.Log("Faut aller se faire enculé un jour");

            NetworkObject networkObject = _currentPlayer.GetComponentInParent<NetworkObject>();
            CircleProgressClientRpc(timer/ unlockTime, networkObject.OwnerClientId);
            timer += Time.deltaTime;
            yield return null;
        }
        SetVaultStateServerRPC(VaultState.Opened);
        Debug.Log("Vault is Open");
        RequestSpawnLootServerRpc();
    }

    [ClientRpc]
    private void CircleProgressClientRpc(float progress, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;
        S_CircleLoad.OnCircleChange(progress);
    }

    public override void Interact(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        RequestUnlockServerRpc(p_playerInteract.GetComponentInParent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestUnlockServerRpc(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject netObj))
        {
            Debug.Log("Player Interact is Null");
            return;
        }

        S_PlayerInteract player = netObj.GetComponentInChildren<S_PlayerInteract>();
        if (player == null)
        {
            Debug.Log("Player Interact is Null");
            return;
        }
        if (vaultState.Value == VaultState.Opened) return;

        if (_currentPlayer != null && _currentPlayer != player && vaultState.Value == VaultState.InUse)
            return;

        _currentPlayer = player;
        StartCoroutine(UnlockSequence());
    }

    private S_PlayerInteract FindPlayerByClientId(ulong clientId)
    {
        foreach (var p in FindObjectsOfType<S_PlayerInteract>())
        {
            NetworkObject netObj = p.GetComponent<NetworkObject>();
            if (netObj != null && netObj.OwnerClientId == clientId)
                return p;
        }
        return null;
    }

    public override void StopInteract(S_PlayerInteract p_playerInteract)
    {
        SetVaultStateServerRPC(vaultState.Value == VaultState.Opened ? VaultState.Opened : VaultState.Closed);
        NetworkObject networkObject = _currentPlayer.GetComponentInParent<NetworkObject>();
        CircleProgressClientRpc(0, networkObject.OwnerClientId);
        _currentPlayer = null;
        Debug.Log("Stop Open Vault");
    }

    private void OnEnable()
    {
        vaultState.OnValueChanged += OnVaultStateChanged;
    }

    private void OnDisable()
    {
        vaultState.OnValueChanged -= OnVaultStateChanged;
    }

    private void OnVaultStateChanged(VaultState oldState, VaultState newState)
    {
        Debug.Log($"Vault state changed from {oldState} to {newState}");
    }
}


