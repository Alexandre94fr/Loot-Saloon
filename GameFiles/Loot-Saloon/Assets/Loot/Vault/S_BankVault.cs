using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class S_BankVault : S_Interactable
{
    public enum VaultState
    {
        Closed,
        InUse,
        Opened
    }

    [Header(" Debugging :")]
    [SerializeField] bool _isDebuggingModeOn;

    [Space]
    [ReadOnlyInInspector] [SerializeField] 
    private NetworkVariable<VaultState> _vaultState = new(
        VaultState.Closed,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [ReadOnlyInInspector] [SerializeField] private int _moneyValue;
    [ReadOnlyInInspector] [SerializeField] private S_LootInstantiator _lootInstantiator;
    [ReadOnlyInInspector] [SerializeField] private S_VaultInstantiator _vaultInstantiator;


    [Header(" Properties :")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] [Range(0, 10)] private float _unlockTime = 6f;


    private S_PlayerInteract _currentPlayerInteractComponent;
    private List<int> _lootIndices = new();


    #region Getter setter methods

    public int GetMoneyValue() { return _moneyValue; }

    public S_LootInstantiator GetLootInstantiator() { return _lootInstantiator; }
    public void SetLootInstantiator(in S_LootInstantiator p_lootInstantiator) { _lootInstantiator = p_lootInstantiator; }

    public S_VaultInstantiator GetVaultInstantiator() { return _vaultInstantiator; }
    public void SetVaultInstantiator(in S_VaultInstantiator p_vaultInstantiator) { _vaultInstantiator = p_vaultInstantiator; }
    #endregion

    [ClientRpc]
    public void UpdateQuotaClientRpc(int value)
    {
        _moneyValue = value;
        _vaultInstantiator.UpdateQuota(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (_spawnPoints.Length <= 0)
        {
            Debug.LogError($"ERROR ! The '{name}' GameObject's (Vault) variable '{nameof(_spawnPoints)}' does not contains any values. " +
                "Please set it throw the inspector."
            );

            return;
        }
    }

    private void OnEnable()
    {
        _vaultState.OnValueChanged += OnVaultStateChanged;
    }

    private void OnDisable()
    {
        _vaultState.OnValueChanged -= OnVaultStateChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetVaultStateServerRPC(VaultState p_vaultState)
    {
        _vaultState.Value = p_vaultState;
    }

    [ClientRpc]
    public void SetInstantiatorsClientRpc(ulong p_vaultInstantiatorNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(p_vaultInstantiatorNetworkId, out NetworkObject vaultInstantiatorObject))
        {
            _vaultInstantiator = vaultInstantiatorObject.GetComponent<S_VaultInstantiator>();
            _lootInstantiator = _vaultInstantiator.LootInstanciator;
        }
    }

    public void GenerateLoots()
    {
        foreach (Transform spawnPoint in _spawnPoints)
        {
            int lootIndex = _lootInstantiator.GetRandomLootPropertiesIndex(SO_LootProperties.Size.Medium);

            _lootIndices.Add(lootIndex);
            _moneyValue += _lootInstantiator.GetLootPrice(lootIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnLootServerRpc()
    {
        SpawnLoot();
    }

    public void SpawnLoot()
    {
        for (int i = 0; i < _spawnPoints.Length; i++)
        {
            _lootInstantiator.SpawnLoot(_lootIndices[i], _spawnPoints[i]);
        }
    }

    IEnumerator UnlockSequence()
    {
        if (_isDebuggingModeOn)
            Debug.Log($"Entering '{nameof(UnlockSequence)}' method.");

        SetVaultStateServerRPC(VaultState.InUse);

        float timer = 0f;
        NetworkObject playerNetworkObject = _currentPlayerInteractComponent.GetComponentInParent<NetworkObject>();

        while (timer < _unlockTime)
        {
            if (_vaultState.Value != VaultState.InUse)
                yield break;

            if (_isDebuggingModeOn)
                Debug.Log("Opening the Vault...");

            if (_currentPlayerInteractComponent == null)
                Debug.LogWarning($"The '{name}' GameObject's (Vault) variable '{nameof(_currentPlayerInteractComponent)}' is null. Check it.");

            CircleProgressClientRpc(timer/ _unlockTime, playerNetworkObject.OwnerClientId);

            timer += Time.deltaTime;

            yield return null;
        }

        SetVaultStateServerRPC(VaultState.Opened);

        if (_isDebuggingModeOn)
            Debug.Log("Vault is Open");

        RequestSpawnLootServerRpc();
    }

    [ClientRpc]
    private void CircleProgressClientRpc(float p_progress, ulong p_targetClientId)
    {
        if (_isDebuggingModeOn)
            Debug.Log($"Entering '{nameof(CircleProgressClientRpc)}' method.");

        if (NetworkManager.Singleton.LocalClientId != p_targetClientId)
            return;

        if (_isDebuggingModeOn)
            Debug.Log("Circle progression : {p_progress} / {p_targetClientId}");

        S_PlayerUseUI.OnCircleChange(p_progress);
    }

    public override void Interact(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        RequestUnlockServerRpc(p_playerInteract.GetComponentInParent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestUnlockServerRpc(NetworkObjectReference p_playerRef)
    {
        if (!p_playerRef.TryGet(out NetworkObject networkObject))
        {
            Debug.LogError("ERROR ! Can't find the player's NetworkObject.");
            return;
        }

        S_PlayerInteract playerInteractComponent = networkObject.GetComponentInChildren<S_PlayerCharacter>().playerInteract;
        
        if (playerInteractComponent == null)
        {
            Debug.LogError("ERROR ! Can't find the player's S_PlayerInteract component.");
            return;
        }

        if (_vaultState.Value == VaultState.Opened) 
            return;

        if (_currentPlayerInteractComponent != null &&
            _currentPlayerInteractComponent != playerInteractComponent &&
            _vaultState.Value == VaultState.InUse)
        {
            return;
        }

        _currentPlayerInteractComponent = playerInteractComponent;

        StartCoroutine(UnlockSequence());
    }

    public override void StopInteract(S_PlayerInteract p_playerInteractComponent)
    {
        if (p_playerInteractComponent == null)
        {
            Debug.LogError($"ERROR ! The given '{nameof(p_playerInteractComponent)}' is null.");
            return;
        }

        NetworkObject networkObject = p_playerInteractComponent.GetComponentInParent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError("ERROR ! Can't find the player's NetworkObject.");
            return;
        }

        StopInteractServerRpc(networkObject.OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopInteractServerRpc(ulong p_playerClientId)
    {
        if (_currentPlayerInteractComponent == null)
        {
            if (_isDebuggingModeOn)
                Debug.LogError($"The player stopped breaking the vault.");

            return;
        }

        NetworkObject currentNetworkObject = _currentPlayerInteractComponent.GetComponentInParent<NetworkObject>();

        if (currentNetworkObject.OwnerClientId != p_playerClientId)
        {
            Debug.LogWarning($"[StopInteractServerRpc] Unauthorized client tried to stop interaction: {p_playerClientId}");
            return;
        }

        SetVaultStateServerRPC(_vaultState.Value == VaultState.Opened ? VaultState.Opened : VaultState.Closed);

        CircleProgressClientRpc(0, p_playerClientId);

        if (_isDebuggingModeOn)
            Debug.Log($"The player '{p_playerClientId}' has stopped opening Vault.");

        _currentPlayerInteractComponent = null;
    }

    private void OnVaultStateChanged(VaultState p_oldVaultState, VaultState p_newVaultState)
    {
        if (_isDebuggingModeOn)
        {
            Debug.Log(
                $"The '{name}' GameObject's (Vault) variable '{nameof(_vaultState)}' " +
                $"(may have) changed from '{p_oldVaultState}' to '{p_newVaultState}'."
            );
        }

        // Here you can add for example particles, sound, and other stuff
    }
}