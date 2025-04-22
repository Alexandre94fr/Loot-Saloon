using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProperties", menuName = "Scriptable Objects/PlayerProperties")]
public class S_PlayerProperties : ScriptableObject
{
    [Header(" Movement :")]
    public float walkingMovementSpeed;
    public float runningMovementSpeed;

    [Header(" Health :")]
    public int maxHealthPoints;

    [Header(" Lifting :")]
    public int liftingStrengh;
}