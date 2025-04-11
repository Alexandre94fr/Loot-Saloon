using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class S_LootInstantiator : MonoBehaviour
{
    [SerializeField] private List<SO_LootProperties> _lootProperties = new();

    public UnityEvent<S_Loot> OnLootSpawned = new();
    public UnityEvent<S_BankVault> OnVaultFilled = new();

    public Transform[] vaultSpawnPoints;
    public GameObject pb_vault;

    public S_Quota quota;

    private void Start()
    {
        SpawnVaults();
    }

    public int GetLootPrice(int index)
    {
        SO_LootProperties properties = GetLootProperties(index);
        S_Loot loot = properties.PB_prefab.GetComponent<S_Loot>();
        return loot.properties.moneyValue;
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
        SO_LootProperties properties = GetLootProperties(p_index);
        GameObject lootObject = Instantiate(properties.PB_prefab, p_where.position, Quaternion.identity);

        S_Loot loot = lootObject.GetComponent<S_Loot>();
        loot.properties = Instantiate(properties);
    }

    public void SpawnVaults()
    {
        foreach (Transform t in vaultSpawnPoints)
        {
            S_BankVault vault = Instantiate(pb_vault, t).GetComponent<S_BankVault>();
            vault.lootInstantiator = this;
        }

    }

    public void UpdateQuota(S_BankVault vault)
    {
        OnVaultFilled.Invoke(vault);
    }
}
