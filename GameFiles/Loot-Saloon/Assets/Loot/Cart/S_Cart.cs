using Steamworks;
using System.Collections.Generic;
using UnityEngine;

public class S_Cart : S_Pickable
{
    public int total {get; private set;} = 0;

    [SerializeField] private List<S_PlayerInteract> players;
    private HashSet<S_Loot> _inCart = new();

    [SerializeField] private GameObject slot;

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
            print("total cart: " + total);

            loot.transform.SetParent(slot.transform, true);
            loot.SetCart(this);
        }
    }

    private void OnTriggerExit(Collider p_collider)
    {
        if (p_collider.TryGetComponent(out S_Loot loot) && _inCart.Contains(loot))
        {
            _inCart.Remove(loot);
            total -= loot.properties.moneyValue;
            print("total cart: " + total);

            if (loot.transform.parent == _transform)
                loot.transform.SetParent(null, true);

            loot.SetCart(null);
        }
    }
}
