using UnityEngine;

public class S_PlayerCharacter : MonoBehaviour
{
    [Header(" Internal references :")]
    public S_LifeManager lifeManager;
    public S_PlayerInteract playerInteract;

    private void Start()
    {
        if (!S_VariablesChecker.AreVariablesCorrectlySetted(name, null,
            (lifeManager, nameof(lifeManager)),
            (playerInteract, nameof(playerInteract))
        )) return;
    }
}