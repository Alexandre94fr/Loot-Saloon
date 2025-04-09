using System.Collections;
using UnityEngine;

public class S_BankVault : MonoBehaviour
{
    public S_LootInstantiator lootInstantiator;
    public Transform[] spawnPoints;
    public int moneyValue;

    public float unlockTime = 6f;


    private void Start()
    {
        GenerateLoots();
    }

    public void Interact()
    {
        StartCoroutine(UnlockSequence());
    }

    public void GenerateLoots()
    {
        foreach (Transform point in spawnPoints)
        {
            int lootIndex = lootInstantiator.GetRandomLootPropertiesIndex(SO_LootProperties.Size.Medium);
            lootInstantiator.SpawnLoot(this, lootIndex, point);
        }

        lootInstantiator.UpdateQuota(this);
    }

    IEnumerator UnlockSequence()
    {
        float timer = 0f;

        while (timer < unlockTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }

}
