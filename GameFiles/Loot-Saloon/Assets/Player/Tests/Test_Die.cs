using System;
using UnityEngine;

public class Test_Die : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponentInChildren<S_LifeManager>().TakeDamage(100);
        Destroy(gameObject);
    }
}
