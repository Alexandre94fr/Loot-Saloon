using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class S_VaultInstantiator : NetworkBehaviour
{
    [SerializeField] private S_Quota _quotaObject;
    [SerializeField] private S_LootInstantiator _lootInstantiatorInstance;
    public S_LootInstantiator LootInstanciator => _lootInstantiatorInstance;
    public UnityEvent<S_BankVault> OnVaultFilled = new();

    public Transform[] vaultSpawnPoints;
    public GameObject pb_vault;

    public override void OnNetworkSpawn()
    {
        if (_lootInstantiatorInstance == null)
            Debug.Assert(true," Loot Instanciator Reference should be set in the Vault Instanciator + " + this.name);

        print("Je viens de spawn");

        NetworkManager.Singleton.OnClientConnectedCallback += StartGame;
    }
    private void StartGame(ulong _)
    {
        if (PlayerPrefs.GetInt("NbrOfPlayer") == NetworkManager.Singleton.ConnectedClients.Count)
        {
            SpawnVaults();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetLootInstantiatorServerRpc(ulong vaultNetworkId, ulong lootInstantiatorNetworkId)
    {
        // Trouver l'objet sur le serveur
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(vaultNetworkId, out var vaultObject))
        {
            var vault = vaultObject.GetComponent<S_BankVault>();
            if (vault != null)
            {
                // R�cup�rer _lootInstantiator via son ID r�seau
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(lootInstantiatorNetworkId, out var lootInstantiatorObject))
                {
                    S_LootInstantiator lootInstantiator = lootInstantiatorObject.GetComponent<S_LootInstantiator>();
                    vault.SetLootInstantiator(lootInstantiator);
                }
            }
        }
    }

    public void SpawnVaults()
    {
        if (!IsServer) return;

        foreach (Transform t in vaultSpawnPoints)
        {
            S_BankVault vault = Instantiate(pb_vault, t).GetComponent<S_BankVault>();
            Debug.Log(vault.name + " : Set loot Instanciator with : " + this.name);

            NetworkObject vaultNetworkObject = vault.GetComponent<NetworkObject>();
            vaultNetworkObject.Spawn();

            vault.SetLootInstantiator(_lootInstantiatorInstance);
            vault.SetVaultInstantiator(this);

            ulong vaultNetworkId = vaultNetworkObject.NetworkObjectId;
            ulong vaultInstantiatorNetworkId = GetComponent<NetworkObject>().NetworkObjectId;

            SetLootInstantiatorServerRpc(vaultNetworkId, vaultInstantiatorNetworkId);
            vault.SetInstantiatorsClientRpc(vaultInstantiatorNetworkId);

            vault.GenerateLoots();
            vault.UpdateQuotaClientRpc(vault.GetMoneyValue());
        }
    }

    public void UpdateQuota(S_BankVault p_vault)
    {
        OnVaultFilled.Invoke(p_vault);
    }
}
