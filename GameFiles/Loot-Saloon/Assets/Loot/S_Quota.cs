using UnityEngine;

public class S_Quota : MonoBehaviour
{
    [Header(" External references :")]
    [SerializeField] private S_LootInstantiator _instantiator;

    [Header(" Properties :")]
    [Tooltip("How much of the total sum the quota is equal to")]
    [SerializeField] [Range(0f, 1f)] private float _fraction = 0.67f;

    public int total {get; private set;}
    public int quota {get; private set;}

    private void Start()
    {
        if (!_instantiator)
        {
            Debug.LogError(
                $"ERROR ! The '{nameof(_instantiator)}' variable was not set, the value is null. " +
                $"Stopping Start method execution"
            );
            return;
        }

        _instantiator.OnVaultFilled.AddListener(OnVaultFilled);
    }
    
    public void OnVaultFilled(S_BankVault vault)
    {
        total += vault.moneyValue;
        quota = (int) (total * _fraction);

    }
}
