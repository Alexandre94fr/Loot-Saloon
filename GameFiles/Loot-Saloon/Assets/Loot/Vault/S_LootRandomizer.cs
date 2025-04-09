using System.Collections.Generic;
using UnityEngine;

public class S_LootRandomizer : MonoBehaviour
{
    [System.Serializable]
    public struct LootEntry
    {
        public LootType type;
        public GameObject prefab;
    }

    public List<LootEntry> lootPrefabs;

    public LootType GetRandomLootType()
    {
        int count = System.Enum.GetValues(typeof(LootType)).Length;
        return (LootType)Random.Range(0, count);
    }

    
}

public enum LootType // temporaly declared here for compilation
{
    Loot1,
    Loot2,
    Loot3,
    Loot4,
    Loot5,
}
