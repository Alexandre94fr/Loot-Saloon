using System.Collections;
using UnityEngine;

public class S_WeaponSlot : MonoBehaviour
{
    Camera _camera;

    public Transform weaponParent;
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

    [SerializeField][Range(1f, 10f)] protected float _angleSpread = 5;

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

    public void SetWeaponSlot(S_Weapon newWeapon)
    {
        if (newWeapon == null)
            return;

        // Drop the currently held weapon, if any
        if (weaponObject != null)
        {
            DropWeapon(weaponObject.GetComponent<S_Weapon>());
        }

        // Update with the new weapon's properties
        SO_WeaponProperties properties = newWeapon.properties;
        heldWeapon = properties;
        weaponName = properties.weaponName;
        damage = properties.damage;
        nbBullet = properties.nbBullet;
        nbBulletMax = properties.nbBulletMax;
        cooldown = properties.cooldown;

        //// Mark it as held and parent it
        //newWeapon.isHeld = true;
        newWeapon.transform.SetParent(weaponParent);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        EnableWeapon(newWeapon.gameObject);

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

    public void DropWeapon(S_Weapon weapon)
    {
        weapon.isHeld = false;

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

        weaponName = "";
        damage = 0;
        nbBullet = 0;
        nbBulletMax = 0;
        cooldown = 0;
    }


    public void Shoot()
    {
        if (!weaponIsActive || Time.time - _lastShotTime < cooldown || nbBullet <= 0)
            return;

        _lastShotTime = Time.time;

        Vector3 rayOrigin = _camera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));

        float xAngle = S_Utils.RandomFloat(-_angleSpread, _angleSpread);
        float yAngle = S_Utils.RandomFloat(-_angleSpread, _angleSpread);

        Vector3 raycastDirection = Quaternion.Euler(xAngle, yAngle, 0) * _camera.transform.forward;

        StartCoroutine(DebugShoot(rayOrigin, raycastDirection, 2f));

        if (Physics.Raycast(rayOrigin, raycastDirection, out RaycastHit hit))
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
    public SO_WeaponProperties _weaponProperties2;

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

    private void OnDestroy()
    {
        S_PlayerInputsReciever.OnInteract -= Shoot;
    }




    private IEnumerator DebugShoot(Vector3 origin, Vector3 direction, float duration)
    {
        if (!Physics.Raycast(origin, direction, out RaycastHit hit))
            yield break;

        Vector3 end = hit.point;

        float t = 0;

        while (t < duration)
        {
            Debug.DrawLine(origin, end);
            t += Time.deltaTime;
            yield return null;
        }
    }
}
