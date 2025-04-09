using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_BankVault : MonoBehaviour
{
    public S_LootRandomizer lootRandomizer;
    public Transform[] spawnPoints;

    public List<TempLootType> currentLoots = new();
    public int vaultValue;

    public float unlockTime = 6f;

    private void Start()
    {
        GenerateLoots();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.U)) // for test
        {
            Interact();
        }
    }

    public void Interact()
    {
        StartCoroutine(UnlockSequence());
    }

    public void GenerateLoots()
    {
        currentLoots.Clear();
        foreach (Transform point in spawnPoints)
        {
            TempLootType randomLoot = lootRandomizer.GetRandomLootType();
            currentLoots.Add(randomLoot);
            // Loot instantiator
        }
    }

    public int GetVaultValue()
    {
        return vaultValue;
    }

    IEnumerator UnlockSequence()
    {
        float timer = 0f;

        while (timer < unlockTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Vault in unlocked");
    }

}
