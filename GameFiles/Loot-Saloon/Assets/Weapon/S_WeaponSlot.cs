using UnityEngine;

public class S_WeaponSlot : MonoBehaviour
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

    private float _lastShotTime;

    public GameObject weaponObject;

    private void Start()
    {
        S_PlayerInputsReciever.OnShoot += Shoot;
        _camera = Camera.main;

        if (heldWeapon != null)
            SetWeaponSlot(heldWeapon.prefab.GetComponent<S_Weapon>());

        interact.OnWeaponPickUp.AddListener(SetWeaponSlot);
        interact.OnPickUp.AddListener(OnGenericPickUp);

        _lastShotTime = -cooldown;

        TestInstantiateWeapon();
    }

    public void SetWeaponSlot(S_Weapon weapon)
    {
        if (weapon == null)
        {
            DropWeapon();
            return;
        }

        //DropWeapon();

        if (weaponObject != null)
        {
            DropWeapon();
        }


        SO_WeaponProperties properties = weapon.properties;
        heldWeapon = properties;
        weaponName = properties.weaponName;
        damage = properties.damage;
        nbBullet = properties.nbBullet;
        nbBulletMax = properties.nbBulletMax;
        cooldown = properties.cooldown;

        EnableWeapon(weapon.gameObject);
    }

    public void OnGenericPickUp(S_Pickable pickable)
    {
        if (pickable != null)
        {
            DisableWeapon();
        }
        else if (weaponObject != null)
        {
            EnableWeapon(weaponObject);
        }

    }

    public void EnableWeapon(GameObject newWeaponObject)
    {
        weaponIsActive = true;
        weaponObject = newWeaponObject;
        weaponObject.SetActive(true);

    }

    public void DisableWeapon()
    {
        weaponIsActive = false;
        if (weaponObject != null)
            weaponObject.SetActive(false);
    }

    public void DropWeapon()
    {
        print(heldWeapon.name);
        if (weaponObject == null)
            return;

        weaponObject.transform.SetParent(null);
        weaponObject.transform.position = _camera.transform.position + _camera.transform.forward * 1.5f;
        weaponObject.SetActive(true);

        if (weaponObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        weaponObject = null;
        heldWeapon = null;
        weaponIsActive = false;
    }


    public void Shoot()
    {
        print("SHOOT");
        if (!weaponIsActive || Time.time - _lastShotTime < cooldown || nbBullet <= 0)
            return;

        _lastShotTime = Time.time;

        Vector3 rayOrigin = _camera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
        if (Physics.Raycast(rayOrigin, _camera.transform.forward, out RaycastHit hit))
        {
            if (hit.transform.TryGetComponent(out TempTarget target)) // temp condition for testing
            {
                print($"Hit: {target.name}");
                // apply damage
            }
        }

        nbBullet--;
    }

    public void Reload()
    {

    }

    public Transform testTransform;
    public Transform testTransform2;
    public SO_WeaponProperties _weaponProperties;

    void TestInstantiateWeapon()
    {
        SO_WeaponProperties properties = _weaponProperties;
        GameObject weaponObject1 = Instantiate(properties.prefab, testTransform.position, Quaternion.identity);
        GameObject weaponObject2 = Instantiate(properties.prefab, testTransform2.position, Quaternion.identity);

        S_Weapon weapon1 = weaponObject1.GetComponent<S_Weapon>();
        weapon1.properties = Instantiate(properties);

        S_Weapon weapon2 = weaponObject2.GetComponent<S_Weapon>();
        weapon2.properties = Instantiate(properties);

    }

    private void OnDestroy()
    {
        S_PlayerInputsReciever.OnInteract -= Shoot;
    }
}
