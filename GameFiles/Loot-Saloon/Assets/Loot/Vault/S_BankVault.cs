using System.Collections.Generic;
using UnityEngine;

public class S_BankVault : MonoBehaviour
{
    public S_LootRandomizer lootRandomizer;
    public Transform[] spawnPoints;
    public List<GameObject> currentLoots = new();

    private void Start()
    {
        GenerateLoots();

    }

    public void Interact()
    {
        
    }

    public void GenerateLoots()
    {
        currentLoots.Clear();
        foreach (Transform point in spawnPoints)
        {
            // call spawn method here
            SpawnLoot(point);
        }
    }

    public void SpawnLoot(Transform spawnPoint)
    {
        LootType randomLoot = lootRandomizer.GetRandomLootType();

        // get prefab here

        //if (prefab != null)
        //    Instantiate(prefab, spawnPoint.position, Quaternion.identity);
    }

    public int GetVaultValue()
    {
        int sum;
        return GetVaultValue();
    }

}
