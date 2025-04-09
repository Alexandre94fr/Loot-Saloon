using UnityEngine;

public class S_Quota : MonoBehaviour
{
    [SerializeField] private S_LootInstantiator _instantiator;
    [Tooltip("How much of the total sum the quota is equal to")]
    [SerializeField] [Range(0f, 1f)] private float _fraction = 0.67f;

    public int total {get; private set;}
    public int quota {get; private set;}

    private void Start()
    {
        _instantiator.onLootSpawned.AddListener(OnLootSpawned);
    }

    private void OnLootSpawned(S_Loot loot)
    {
        total += loot.properties.moneyValue;
        quota = (int) (total * _fraction);


        // print($"total = {total} | quota = {quota}");
    }
}
