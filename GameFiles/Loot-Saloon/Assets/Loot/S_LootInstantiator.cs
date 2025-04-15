using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class S_LootInstantiator : NetworkBehaviour
{
    [SerializeField] private List<SO_LootProperties> _lootProperties = new();

    public UnityEvent<S_Loot> OnLootSpawned = new();
    public UnityEvent<S_BankVault> OnVaultFilled = new();

    public Transform[] vaultSpawnPoints;
    public GameObject pb_vault;

    public S_Quota quota;

    public override void OnNetworkSpawn()
    {
        print("Je viens de spawn");

        NetworkManager.Singleton.OnClientConnectedCallback += StartGame;
    }

    private void StartGame(ulong _) 
    {
        if(PlayerPrefs.GetInt("NbrOfPlayer") == NetworkManager.Singleton.ConnectedClients.Count)
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
                // Récupérer lootInstantiator via son ID réseau
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(lootInstantiatorNetworkId, out var lootInstantiatorObject))
                {
                    var lootInstantiator = lootInstantiatorObject.GetComponent<S_LootInstantiator>();
                    vault.lootInstantiator = lootInstantiator;
                }
            }
        }
    }



    public int GetLootPrice(int p_index)
    {
        SO_LootProperties properties = GetLootProperties(p_index);
        return properties.moneyValue;
    }

    public int GetRandomLootPropertiesIndex(SO_LootProperties.Size? p_size)
    {
        int tries = 100;
        int index;

        SO_LootProperties properties;
        System.Random rand = new();

        while (tries-- > 0)
        {
            index = rand.Next(_lootProperties.Count);
            properties = _lootProperties[index];

            if (p_size == null || p_size == properties.size)
                return index;
        }

        throw new Exception($"could not find a correct loot - enum value is {Enum.GetName(typeof (SO_LootProperties.Size), p_size)}");
    }

    public SO_LootProperties GetLootProperties(int p_index)
    {
        return _lootProperties[p_index];
    }

    public void SpawnLoot(int p_index, Transform p_where)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        SO_LootProperties properties = GetLootProperties(p_index);
        GameObject lootObject = Instantiate(properties.PB_prefab, p_where.position, Quaternion.identity);

        S_Loot loot = lootObject.GetComponent<S_Loot>();
        loot.properties = Instantiate(properties);

        if (!IsServer) return;
        if (lootObject.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Spawn();
        }
    }

    public void SpawnVaults()
    {
        if (!IsServer) return; 

        foreach (Transform t in vaultSpawnPoints)
        {
            S_BankVault vault = Instantiate(pb_vault, t).GetComponent<S_BankVault>();
            Debug.Log(vault.name + " : Set loot Instanciator with : " + this.name);
            vault.lootInstantiator = this;

            vault.GenerateLoots();

            ulong vaultNetworkId = vault.GetComponent<NetworkObject>().NetworkObjectId;
            ulong lootInstantiatorNetworkId = this.GetComponent<NetworkObject>().NetworkObjectId;

            SetLootInstantiatorServerRpc(vaultNetworkId, lootInstantiatorNetworkId);

            if (vault.TryGetComponent(out NetworkObject networkObject))
            {
                networkObject.Spawn();
                vault.SetLootInstantiatorClientRpc(lootInstantiatorNetworkId);
            }
        }
    }



    public void UpdateQuota(S_BankVault p_vault)
    {
        OnVaultFilled.Invoke(p_vault);
    }
}
