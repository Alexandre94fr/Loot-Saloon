using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class S_WeaponSlot : MonoBehaviour
{
    Camera _camera;

    public Transform weaponParent;
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

    [SerializeField][Range(1f, 10f)] protected float _angleSpread = 5;

    public S_LifeManager lifeManager;

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

    public void SetWeaponSlot(Transform parent, S_Weapon newWeapon)
    {
        if (newWeapon == null)
            return;

        NetworkObject weaponNetworkObject = newWeapon.GetComponent<NetworkObject>();

        if (weaponNetworkObject == null)
        {
            Debug.LogWarning("Le NetworkObject de l'arme est manquant.");
            return;
        }

        if (weaponObject != null)
            DropWeapon(weaponObject.GetComponent<S_Weapon>());

        SO_WeaponProperties properties = newWeapon.properties;
        heldWeapon = properties;
        weaponName = properties.weaponName;
        damage = properties.damage;
        nbBullet = properties.nbBullet;
        nbBulletMax = properties.nbBulletMax;
        cooldown = properties.cooldown;

        EnableWeapon(newWeapon.gameObject);
    }

    [ServerRpc]
    public void ServerSpawnRpc(NetworkObject obj)
    {
        obj.Spawn(true);
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
        weaponObject.GetComponent<MeshRenderer>().enabled = true;
    }

    public void DisableWeapon()
    {
        weaponIsActive = false;
        if (weaponObject != null)
            weaponObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void DropWeapon(S_Weapon weapon)
    {
        weapon.PutDown();
        weapon.isHeld = false;

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

        Vector3 rayOrigin = _camera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));

        float xAngle = S_Utils.RandomFloat(-_angleSpread, _angleSpread);
        float yAngle = S_Utils.RandomFloat(-_angleSpread, _angleSpread);

        Vector3 raycastDirection = Quaternion.Euler(xAngle, yAngle, 0) * _camera.transform.forward;

        StartCoroutine(DebugShoot(rayOrigin, raycastDirection, 2f));

        if (Physics.Raycast(rayOrigin, raycastDirection, out RaycastHit hit))
        {
            Debug.Log("HIT " + hit.transform.name);

            if (hit.transform.TryGetComponent(out S_PlayerCharacter target))
            {
                // Récupération du NetworkObject du parent (PB_Player)
                var playerRoot = target.GetComponentInParent<NetworkObject>();
                if (playerRoot != null)
                {
                    OnHitServerRpc(playerRoot, damage);
                }
                else
                {
                    Debug.LogWarning("No NetworkObject found on target's parent!");
                }
            }
        }

        nbBullet--;
    }

    [ServerRpc]
    public void OnHitServerRpc(NetworkObjectReference targetRef, float damage)
    {
        Debug.Log("OnHitServerRpc");

        if (targetRef.TryGet(out var netObj))
        {
            // On cherche le S_PlayerCharacter dans les enfants
            var targetCharacter = netObj.GetComponentInChildren<S_PlayerCharacter>();
            if (targetCharacter != null && targetCharacter.LifeManager != null)
            {
                // Appliquer les dégâts côté serveur
                targetCharacter.LifeManager.TakeDamage(damage);

                // Envoi d’un feedback côté client touché
                ulong targetClientId = netObj.OwnerClientId;

                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { targetClientId }
                    }
                };

                OnHitClientRpc(damage, clientRpcParams);
            }
            else
            {
                Debug.LogWarning("Target has no S_PlayerCharacter or LifeManager!");
            }
        }
        else
        {
            Debug.LogWarning("Invalid NetworkObjectReference in OnHitServerRpc.");
        }
    }

    [ClientRpc]
    public void OnHitClientRpc(float damage, ClientRpcParams clientRpcParams = default)
    {
        // On est sûr que ce RPC ne s’exécute que pour le client visé
        if (NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject() is NetworkObject localPlayer)
        {
            var character = localPlayer.GetComponentInChildren<S_PlayerCharacter>();
            if (character != null && character.LifeManager != null)
            {
                character.LifeManager.TakeDamage(damage);
                Debug.Log($"You took {damage} damage!");
            }
            else
            {
                Debug.LogWarning("No S_PlayerCharacter or LifeManager on local player.");
            }
        }
        else
        {
            Debug.LogWarning("Local player not found.");
        }
    }





    public void Reload()
    {
        nbBullet = nbBulletMax;
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
