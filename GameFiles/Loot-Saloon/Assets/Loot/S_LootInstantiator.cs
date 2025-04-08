using System.Collections.Generic;
using System;
using UnityEngine;

public class S_LootInstantiator : MonoBehaviour
{
    [SerializeField] private List<SO_LootProperties> _lootProperties = new();

    public void SpawnLoot(int p_index, Transform p_where)
    {
        SO_LootProperties properties = GetLootProperties(p_index);
        GameObject lootObject = Instantiate(properties.PB_prefab, p_where.position, Quaternion.identity);
        lootObject.GetComponent<S_Loot>().properties = properties;
    }

    public int GetRandomLootPropertiesIndex(SO_LootProperties.Size p_size)
    {
        int tries = 100;
        int index;

        SO_LootProperties properties;
        System.Random rand = new();

        while (tries-- != 0)
        {
            index = rand.Next(_lootProperties.Count);
            properties = _lootProperties[index];

            if (properties.size == p_size)
                return index;
        }

        throw new Exception($"could not find a correct loot - enum value is {Enum.GetName(typeof (SO_LootProperties.Size), p_size)}");
    }

    public SO_LootProperties GetLootProperties(int p_index)
    {
        return _lootProperties[p_index];
    }
}
