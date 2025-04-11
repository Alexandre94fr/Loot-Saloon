using System.Collections;
using UnityEditor.ShaderGraph.Internal;
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

    [SerializeField] [Range(1f, 10f)] protected float _angleSpread = 5;

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
