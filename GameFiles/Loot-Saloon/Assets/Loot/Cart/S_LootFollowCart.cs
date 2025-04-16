using UnityEngine;

public class S_LootFollowCart : MonoBehaviour
{
    public Transform target;

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
