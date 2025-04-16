using UnityEngine;

[CreateAssetMenu(fileName = "SO_WeaponProperties", menuName = "Scriptable Objects/SO_WeaponProperties")]
public class SO_WeaponProperties : ScriptableObject
{
    public string weaponName = "";
    public float damage;
    public int nbBullet;
    public int nbBulletMax;
    public float cooldown;


    public GameObject prefab;
}
