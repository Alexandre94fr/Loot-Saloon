#region
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
#endregion

public class S_Cart : S_Pickable
{
    public int total { get; private set; } = 0;

    [SerializeField] private List<S_PlayerInteract> players;
    private HashSet<S_Loot> _inCart = new();

    [SerializeField] private GameObject slot;
    [SerializeField] private UnityEvent OnLootAdded = new();
    [SerializeField] private UnityEvent OnLootRemoved = new();

    public bool KnowPlayer(S_PlayerInteract player)
    {
        return players.Contains(player);
    }

    private void OnTriggerEnter(Collider p_collider)
    {
        if (p_collider.TryGetComponent(out S_Loot loot) && !_inCart.Contains(loot))
        {
            _inCart.Add(loot);
            total += loot.properties.moneyValue;
            // loot.transform.SetParent(slot.transform, true);
            OnLootAdded.Invoke();
            loot.SetCart(this);
        }
    }

    private void OnTriggerExit(Collider p_collider)
    {
        if (p_collider.TryGetComponent(out S_Loot loot) && _inCart.Contains(loot))
        {
            _inCart.Remove(loot);
            total -= loot.properties.moneyValue;

            if (loot.transform.parent == _transform)
                loot.transform.SetParent(null, true);

            OnLootRemoved.Invoke();

            loot.SetCart(null);
        }
    }

    public void SetTextToTotal(TMP_Text text)
    {
        text.text = total.ToString();
    }
}