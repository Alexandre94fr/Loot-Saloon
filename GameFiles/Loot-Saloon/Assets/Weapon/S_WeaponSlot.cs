using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlot : MonoBehaviour
{
    Camera _camera;

    public bool weaponIsActive;
    public SO_WeaponProperties heldWeapon;

    public S_PlayerInteract interact;

    public string weaponName = "";
    public float damage;
    public int nbBullet;
    public int nbBulletMax;
    public float cooldown;

    private void Start()
    {
        S_PlayerInputsReciever.OnInteract += Shoot;
        _camera = Camera.main;

        if (heldWeapon != null)
            SetWeaponStats(heldWeapon.prefab.GetComponent<S_Weapon>());

        interact.OnWeaponPickUp.AddListener(SetWeaponStats);

        TestInstantiateWeapon();

    }

    public void SetWeaponStats(S_Weapon weapon)
    {
        weaponIsActive = true;
        SO_WeaponProperties properties = weapon.properties;
        heldWeapon = properties;
        weaponName = properties.weaponName;
        damage = properties.damage;
        nbBullet = properties.nbBullet;
        nbBulletMax = properties.nbBulletMax;
        cooldown = properties.cooldown;
    }

    public void Shoot()
    {
        if (weaponIsActive)
        {
            if (nbBullet > 0)
            {
                Vector3 rayOrigin = _camera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
                if (Physics.Raycast(rayOrigin, _camera.transform.forward, out RaycastHit hit))
                {

                    if (hit.transform.GetComponent<TempTarget>())
                    {
                        print(hit.transform.gameObject.name);
                    }
                }
                nbBullet--;

            }
        }

    }

    public void Reload()
    {

    }

    public Transform testTransform;
    public SO_WeaponProperties _weaponProperties;

    void TestInstantiateWeapon()
    {
        SO_WeaponProperties properties = _weaponProperties;
        GameObject weaponObject = Instantiate(properties.prefab, testTransform.position, Quaternion.identity);

        S_Weapon weapon = weaponObject.GetComponent<S_Weapon>();
        weapon.properties = Instantiate(properties);

    }
}
