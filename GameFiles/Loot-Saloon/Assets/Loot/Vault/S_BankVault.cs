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
        if (!IsServer) return;
        for (int i = 0; i< spawnPoints.Length; i++)
        {
            lootInstantiator.SpawnLoot(_lootIndeces[i], spawnPoints[i]);
        }
    }

    IEnumerator UnlockSequence()
    {
        float timer = 0f;
        SetVaultStateServerRPC(VaultState.InUse);

        while (timer < unlockTime)
        {
            if (vaultState.Value != VaultState.InUse)
                yield break;
            Debug.Log("Opening the Vault .....");
            S_CircleLoad.OnCircleChange(timer/ unlockTime);
            timer += Time.deltaTime;
            yield return null;
        }
        SetVaultStateServerRPC(VaultState.Opened);
        Debug.Log("Vault is Open");
        RequestSpawnLootServerRpc();
    }

    public override void Interact(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        ulong clientId = p_playerInteract.GetComponentInParent<NetworkObject>().OwnerClientId;
        RequestUnlockServerRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestUnlockServerRpc(ulong clientId)
    {
        S_PlayerInteract player = FindPlayerByClientId(clientId);

        if (vaultState.Value == VaultState.Opened) return;

        // Si c'est d�j� en cours et c'est un autre joueur qui interagit, on ne fait rien
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
        _currentPlayer = null;
        Debug.Log("Stop Open Vault");
        S_CircleLoad.OnCircleChange(0);
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


