using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class S_BankVault : S_Interactable
{
    public S_LootInstantiator lootInstantiator;
    public Transform[] spawnPoints;
    public int moneyValue;

    private S_PlayerInteract _currentPlayer;
    private List<int> _lootIndeces = new List<int>();
    private bool isOpen = false;
    private bool isOpening = false;
    public bool isAvailable { get { return !isOpen && !isOpening; }  private set {} }

    public float unlockTime = 6f;



    private void Start()
    {
        GenerateLoots();
    }

    public void GenerateLoots()
    {
        foreach (Transform point in spawnPoints)
        {
            int lootIndex = lootInstantiator.GetRandomLootPropertiesIndex(SO_LootProperties.Size.Medium);
            _lootIndeces.Add(lootIndex);
            moneyValue += lootInstantiator.GetLootPrice(lootIndex);
        }

        lootInstantiator.UpdateQuota(this);
    }

    public void SpawnLoot()
    {
        for (int i = 0; i< spawnPoints.Length; i++)
        {
            lootInstantiator.SpawnLoot(_lootIndeces[i], spawnPoints[i]);
        }
    }

    IEnumerator UnlockSequence()
    {
        float timer = 0f;
        isOpening = true;

        while (timer < unlockTime)
        {
            if (!isOpening)
                yield break;
            Debug.Log("Opening the Vault .....");
            S_CircleLoad.OnCircleChange(timer/ unlockTime);
            timer += Time.deltaTime;
            yield return null;
        }
        isOpen = true;
        Debug.Log("Vault is Open");
        SpawnLoot();
    }

    public override void Interact(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        if (!isOpen && (isAvailable || (_currentPlayer == p_playerInteract && isOpening == true)))
        {
            _currentPlayer = p_playerInteract;
            StartCoroutine(UnlockSequence());
        }
    }

    public override void StopInteract(S_PlayerInteract p_playerInteract)
    {
        isOpening = false;
        _currentPlayer = null;
        Debug.Log("Stop Open Vault");
        S_CircleLoad.OnCircleChange(0);
    }
}
