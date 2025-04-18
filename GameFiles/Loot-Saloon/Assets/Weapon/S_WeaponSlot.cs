using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class S_WeaponSlot : NetworkBehaviour
{
    [Header(" External references :")]
    [SerializeField] private S_PlayerInteract _playerInteractComponent;

    [Space]
    [ReadOnlyInInspector] [SerializeField] private string _weaponName = "";
    [ReadOnlyInInspector] [SerializeField] private float _damage;
    [ReadOnlyInInspector] [SerializeField] private int _remainingBullet;
    [ReadOnlyInInspector] [SerializeField] private int _maxBulletNumber;
    [ReadOnlyInInspector] [SerializeField] private float _cooldown;

    [ReadOnlyInInspector] [SerializeField] private GameObject _weaponObject;

    [ReadOnlyInInspector] [SerializeField] [Range(1f, 10f)] private float _angleSpread = 5;

    [ReadOnlyInInspector] [SerializeField] private bool _isReloading = false;
    [ReadOnlyInInspector] [SerializeField] private float _reloadTime = 20f;

    Camera _camera;

    private bool _weaponIsActive;
    private SO_WeaponProperties _heldWeapon;

    private float _lastShotTime;

    private void DropWeaponOnDeath()
    {
        if (_weaponObject != null)
            DropWeapon(_weaponObject.GetComponent<S_Weapon>());
    }


    private void Start()
    {
        if (!S_VariablesChecker.AreVariablesCorrectlySetted(name, null,
            (_playerInteractComponent, nameof(_playerInteractComponent))
        )) return;

        if (!_playerInteractComponent.transform.parent.parent.GetComponent<NetworkObject>().IsOwner)
            return;

        S_PlayerInputsReciever.OnShoot += Shoot;
        _camera = Camera.main;

        if (_heldWeapon != null)
            SetWeaponSlot(_playerInteractComponent.transform, _heldWeapon.prefab.GetComponent<S_Weapon>());

        _playerInteractComponent.OnWeaponPickUp.AddListener(SetWeaponSlot);
        _playerInteractComponent.OnPickUp.AddListener(OnGenericPickUp);

        _lastShotTime = -_cooldown;

        if (IsOwner)
            S_LifeManager.OnDie += DropWeaponOnDeath;
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

        if (_weaponObject != null)
            DropWeapon(_weaponObject.GetComponent<S_Weapon>());

        SO_WeaponProperties properties = p_newWeapon.properties;
        _heldWeapon = properties;
        _weaponName = properties.weaponName;
        _damage = properties.damage;
        _remainingBullet = properties.nbBullet;
        _maxBulletNumber = properties.nbBulletMax;
        _cooldown = properties.cooldown;

        EnableWeapon(p_newWeapon.gameObject);

        p_newWeapon.isHeld = true;
    }

    public void OnGenericPickUp(S_Pickable p_pickable)
    {
        if (p_pickable != null)
        {
            DisableWeapon();
        }
        else if (_weaponObject != null)
        {
            EnableWeapon(_weaponObject);
        }
    }

    public void EnableWeapon(GameObject p_newWeaponObject)
    {
        _weaponIsActive = true;
        _weaponObject = p_newWeaponObject;
        _weaponObject.GetComponent<MeshRenderer>().enabled = true;
    }

    public void DisableWeapon()
    {
        _weaponIsActive = false;
        if (_weaponObject != null)
            _weaponObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void DropWeapon(S_Weapon p_weapon)
    {
        p_weapon.PutDown();
        p_weapon.isHeld = false;

        _weaponObject.transform.SetParent(null);
        _weaponObject.transform.position = _camera.transform.position + _camera.transform.forward * 1.5f;
        _weaponObject.SetActive(true);

        if (_weaponObject.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = false;

        _weaponObject = null;
        _heldWeapon = null;
        _weaponIsActive = false;

        _weaponName = "";
        _damage = 0;
        _remainingBullet = 0;
        _maxBulletNumber = 0;
        _cooldown = 0;
    }

    public void Shoot()
    {
        if (!_playerInteractComponent.controller.activeInputs)
            return;

        if (!_weaponIsActive || Time.time - _lastShotTime < _cooldown)
            return;

        if (_remainingBullet <= 0)
        {
            if (!_isReloading)
                Reload();
            return;
        }

        print("SHOOT");

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
                    OnHitServerRpc(playerRoot.NetworkObjectId, _damage);
                }
                else
                {
                    Debug.LogWarning("No NetworkObject found on target's p_parent!");
                }
            }
        }

        _remainingBullet--;
        if (_remainingBullet <= 0)
            Reload();
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
        if (!_isReloading)
            StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        _isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(_reloadTime);

        _remainingBullet = _maxBulletNumber;
        Debug.Log("Reload complete");

        _isReloading = false;
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