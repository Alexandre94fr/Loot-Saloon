using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class S_WeaponSlot : NetworkBehaviour
{
    Camera _camera;

    private bool weaponIsActive;
    private SO_WeaponProperties heldWeapon;

    public S_PlayerInteract interact;

    [SerializeField] private string weaponName = "";
    [SerializeField] private float damage;
    [SerializeField] private int nbBullet;
    [SerializeField] private int nbBulletMax;
    [SerializeField] private float cooldown;

    private float _lastShotTime;

    [SerializeField] private GameObject weaponObject;

    [SerializeField] [Range(1f, 10f)] private float _angleSpread = 5;


    private void Start()
    {
        if (!interact.transform.parent.parent.GetComponent<NetworkObject>().IsOwner)
            return;

        S_PlayerInputsReciever.OnShoot += Shoot;
        _camera = Camera.main;

        if (heldWeapon != null)
            SetWeaponSlot(interact.transform, heldWeapon.prefab.GetComponent<S_Weapon>());

        interact.OnWeaponPickUp.AddListener(SetWeaponSlot);
        interact.OnPickUp.AddListener(OnGenericPickUp);

        _lastShotTime = -cooldown;
    }

    public void SetWeaponSlot(Transform p_parent, S_Weapon p_newWeapon)
    {
        if (p_newWeapon == null)
            return;

        if (p_newWeapon.isHeld)
            return;

        NetworkObject weaponNetworkObject = p_newWeapon.GetComponent<NetworkObject>();

        if (weaponNetworkObject == null)
        {
            Debug.LogWarning("Le NetworkObject de l'arme est manquant.");
            return;
        }

        if (weaponObject != null)
            DropWeapon(weaponObject.GetComponent<S_Weapon>());

        SO_WeaponProperties properties = p_newWeapon.properties;
        heldWeapon = properties;
        weaponName = properties.weaponName;
        damage = properties.damage;
        nbBullet = properties.nbBullet;
        nbBulletMax = properties.nbBulletMax;
        cooldown = properties.cooldown;

        EnableWeapon(p_newWeapon.gameObject);

        p_newWeapon.isHeld = true;
    }

    public void OnGenericPickUp(S_Pickable p_pickable)
    {
        if (p_pickable != null)
        {
            DisableWeapon();
        }
        else if (weaponObject != null)
        {
            EnableWeapon(weaponObject);
        }
    }

    public void EnableWeapon(GameObject p_newWeaponObject)
    {
        weaponIsActive = true;
        weaponObject = p_newWeaponObject;
        weaponObject.GetComponent<MeshRenderer>().enabled = true;
    }

    public void DisableWeapon()
    {
        weaponIsActive = false;
        if (weaponObject != null)
            weaponObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void DropWeapon(S_Weapon p_weapon)
    {
        p_weapon.PutDown();
        p_weapon.isHeld = false;

        weaponObject.transform.SetParent(null);
        weaponObject.transform.position = _camera.transform.position + _camera.transform.forward * 1.5f;
        weaponObject.SetActive(true);

        if (weaponObject.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = false;

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

        Vector3 rayOrigin = _camera.transform.position + _camera.transform.forward * 0.2f;

        float xAngle = S_Utils.RandomFloat(-_angleSpread, _angleSpread);
        float yAngle = S_Utils.RandomFloat(-_angleSpread, _angleSpread);

        Vector3 raycastDirection = Quaternion.Euler(xAngle, yAngle, 0) * _camera.transform.forward;

        StartCoroutine(DebugShoot(rayOrigin, raycastDirection, 2f));

        if (Physics.Raycast(rayOrigin, raycastDirection, out RaycastHit hit))
        {
            if (hit.transform.TryGetComponent(out S_PlayerCharacter target))
            {
                var playerRoot = target.GetComponentInParent<NetworkObject>();
                if (playerRoot != null)
                {
                    OnHitServerRpc(playerRoot.NetworkObjectId, damage);
                }
                else
                {
                    Debug.LogWarning("No NetworkObject found on target's p_parent!");
                }
            }
        }

        nbBullet--;
    }

    [ServerRpc]
    public void OnHitServerRpc(ulong p_targetNetworkId, float p_damage)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(p_targetNetworkId, out NetworkObject targetNetObj))
        {
            var targetCharacter = targetNetObj.GetComponentInChildren<S_PlayerCharacter>();
            if (targetCharacter != null && targetCharacter.lifeManager != null)
            {
                ulong targetClientId = targetNetObj.OwnerClientId;


                OnHitClientRpc(p_damage, targetClientId);
            }
            else
            {
                Debug.LogWarning("Target has no S_PlayerCharacter or LifeManager! " + targetNetObj.name);
            }
        }
        else
        {
            Debug.LogWarning("Invalid target network object.");
        }
    }


    [ClientRpc]
    public void OnHitClientRpc(float p_damage, ulong p_targetClientId)
    {
        print("OnHitClientRpc" + p_targetClientId);
        if (NetworkManager.Singleton.LocalClientId != p_targetClientId)
            return;

        var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var character = localPlayer.GetComponentInChildren<S_PlayerCharacter>();
        if (character != null && character.lifeManager != null)
        {
            character.lifeManager.TakeDamage(p_damage);
            Debug.Log($"You took {p_damage} p_damage!");
        }
    }


    public void Reload()
    {
        nbBullet = nbBulletMax;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        S_PlayerInputsReciever.OnInteract -= Shoot;
    }

    private IEnumerator DebugShoot(Vector3 p_origin, Vector3 p_direction, float p_duration)
    {
        if (!Physics.Raycast(p_origin, p_direction, out RaycastHit hit))
            yield break;

        Vector3 end = hit.point;

        float t = 0;

        while (t < p_duration)
        {
            Debug.DrawLine(p_origin, end);
            t += Time.deltaTime;
            yield return null;
        }
    }

}