using System.Collections.Generic;
using UnityEngine;

public class S_LootRandomizer : MonoBehaviour
{
    [System.Serializable]
    public struct LootEntry
    {
        public TempLootType type;
        public GameObject prefab;
    }

    public List<LootEntry> lootPrefabs;

    public TempLootType GetRandomLootType()
    {
        int count = System.Enum.GetValues(typeof(TempLootType)).Length;
        return (TempLootType)Random.Range(0, count);
    }
}

public enum TempLootType // temporaly declared here for compilation
{
    Loot1,
    Loot2,
    Loot3,
    Loot4,
    Loot5,
}
