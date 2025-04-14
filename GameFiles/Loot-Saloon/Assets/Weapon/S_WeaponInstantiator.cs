using UnityEngine;

public class S_WeaponInstantiator : MonoBehaviour
{
    public Transform testTransform;
    public SO_WeaponProperties _weaponProperties;
    public Transform testTransform2;
    public SO_WeaponProperties _weaponProperties2;

    private void Start()
    {
        TestInstantiateWeapon();
    }

    void TestInstantiateWeapon()
    {
        SO_WeaponProperties properties = _weaponProperties;
        SO_WeaponProperties properties2 = _weaponProperties2;
        GameObject weaponObject1 = Instantiate(properties.prefab, testTransform.position, Quaternion.identity);
        GameObject weaponObject2 = Instantiate(properties2.prefab, testTransform2.position, Quaternion.identity);

        S_Weapon weapon1 = weaponObject1.GetComponent<S_Weapon>();
        weapon1.properties = Instantiate(properties);

        S_Weapon weapon2 = weaponObject2.GetComponent<S_Weapon>();
        weapon2.properties = Instantiate(properties2);

    }
}
