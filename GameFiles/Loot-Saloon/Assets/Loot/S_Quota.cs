using UnityEngine;

public class S_Quota : MonoBehaviour
{
    public int total { get; private set; }
    public int quota { get; private set; }

    [Header(" External references :")]
    [SerializeField] private S_LootInstantiator _lootInstantiatorInstance;

    [Header(" Properties :")]
    [Tooltip("How much of the total sum the quota is equal to")]
    [SerializeField] [Range(0f, 1f)] private float _extractionQuotaRatio = 0.67f;


    private void Start()
    {
        if (!_lootInstantiatorInstance)
        {
            Debug.LogError(
                $"ERROR ! The '{nameof(_lootInstantiatorInstance)}' variable of the '{nameof(name)}' GameObject was not set, the value is null. " +
                $"Stopping Start method execution"
            );
            return;
        }

        _lootInstantiatorInstance.OnVaultFilled.AddListener(OnVaultFilled);
    }
    
    public void OnVaultFilled(S_BankVault p_bankVault)
    {
        total += p_bankVault.moneyValue;
        quota = (int)(total * _extractionQuotaRatio);
    }
}
