using UnityEngine;

public class S_PlayerCharacter : MonoBehaviour
{
    [Header(" Internal references :")]
    public S_PlayerAttributes playerAttributes;
    public S_PlayerInteract playerInteract;
    public S_PlayerController playerController;

    private void Start()
    {
        if (!S_VariablesChecker.AreVariablesCorrectlySetted(name, null,
            (playerAttributes, nameof(playerAttributes)),
            (playerInteract, nameof(playerInteract)),
            (playerController, nameof(playerController))
        )) return;
    }
}