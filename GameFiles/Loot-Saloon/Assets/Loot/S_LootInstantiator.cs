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
        if (IsServer)
        {
            SpawnVaults();
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

        foreach (Transform t in vaultSpawnPoints)
        {
            S_BankVault vault = Instantiate(pb_vault, t).GetComponent<S_BankVault>();
            vault.lootInstantiator = this;

            if (!IsServer) continue;
            if (vault.TryGetComponent(out NetworkObject networkObject))
            {
                networkObject.Spawn();
            }
        }

    }

    public void UpdateQuota(S_BankVault p_vault)
    {
        OnVaultFilled.Invoke(p_vault);
    }
}
