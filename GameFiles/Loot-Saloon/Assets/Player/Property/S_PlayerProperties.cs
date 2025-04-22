using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProperties", menuName = "Scriptable Objects/PlayerProperties")]
public class S_PlayerProperties : ScriptableObject
{
    [Header(" Movement :")]
    public float walkingMovementSpeed = 4;
    public float runningMovementSpeed = 8;

    [Space]
    public float jumpPower = 5;

    [Header(" Health :")]
    public int maxHealthPoints = 100;

    [Header(" Lifting :")]
    public int liftingStrengh = 5;
}