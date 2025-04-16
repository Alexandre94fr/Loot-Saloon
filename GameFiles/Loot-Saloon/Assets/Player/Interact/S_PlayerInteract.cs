#region
 using System.Collections.Generic;
 using System.Linq;
 using Unity.Netcode;
 using UnityEngine;
 using UnityEngine.Events;
#endregion

[RequireComponent(typeof(SphereCollider))]
public class S_PlayerInteract : NetworkBehaviour
{
    // [SerializeField] private GameObject _interactPanel;

    private Transform _transform;
    [SerializeField] private Transform _cameraTransform;
    public Transform _armTransform;
    private S_Pickable _pickableHeld = null;
    private S_Interactable _currentInteraction = null;
    public Transform _armTransform;


    public S_PlayerAttributes attributes { get; private set; }

    public UnityEvent<Transform, S_Weapon> OnWeaponPickUp = new();
    [SerializeField] private UnityEvent<S_Pickable> _onPickUp = new();

    public UnityEvent<Transform, S_Weapon> OnWeaponPickUp = new();
    [Tooltip("When pickung up a pickable, collisions between the pickable's colliders and these colliders will be disabled.")]
    public List<Collider> pickableIgnoresColliders = new();

    [SerializeField][Range(0, 20)] private float _throwForce = 10;
    [SerializeField] private float _throwAngle = 0f;

    public LayerMask objectLayer;

    private Material _lastRenderer;

    private void Awake()
    {
        _transform = transform;
        _cameraTransform = GetComponentInParent<NetworkObject>().GetComponentInChildren<Camera>().transform;
        attributes = transform.parent.GetComponentInChildren<S_PlayerAttributes>();
    }

    private void Start()
    {
        if (GetComponentInParent<NetworkObject>().IsOwner)
        {
            S_PlayerInputsReciever.OnInteract += Interact;
            S_PlayerInputsReciever.OnStopInteract += StopInteract;
            S_PlayerInputsReciever.OnThrow += Throw;
            S_LifeManager.OnDie += PutDownPickable;
        }
    }
    
    private void StopInteract()
    {
        if (_currentInteraction == null)
            return;
            
        _currentInteraction.StopInteract(this);
        _currentInteraction = null;
    }

    private void Interact()
    {
        if (_pickableHeld != null)
        {
            PutDownPickable();
            return;
        }

        if (_pickableHeld != null)
        {
            foreach (var collider in _pickableHeld.GetComponents<Collider>())
            {
                collider.enabled = false;
            }
        }

        InteractWith(CheckObjectRaycast());

        if (_pickableHeld != null)
        {
            foreach (var collider in _pickableHeld.GetComponents<Collider>())
            {
                collider.enabled = true;
            }
        }
    }

    private void InteractWith(S_Interactable p_interactable)
    {
        if (p_interactable == null)
            return;

        Transform interactParent = _transform;
        _currentInteraction = p_interactable;
        
        if (p_interactable is S_Pickable pickable)
        {
            if (_pickableHeld != null)
                return;

            PickUp(pickable);
            interactParent = pickable.parentIsPlayerInteract ? _transform : _cameraTransform;

            if (pickable is S_Weapon)
                interactParent = pickable.parentIsPlayerInteract ? _transform : _armTransform;
        }

        p_interactable.Interact(this, interactParent);
        S_PlayerInputsReciever.OnLook += CheckLookInteraction;
    }

    private void CheckLookInteraction(Vector2 _)
    {
        if (CheckObjectRaycast() == null)
        {
            StopInteract();
            S_PlayerInputsReciever.OnLook -= CheckLookInteraction;
        }
    }

    private void PickUp(S_Pickable p_pickable)
    {
        if (p_pickable is S_Weapon weapon)
        {
            OnWeaponPickUp.Invoke(_transform, weapon);
            return;
        }

        _pickableHeld = p_pickable;
        OnPickUp.Invoke(p_pickable);
    }

    private void PutDownPickable()
    {
        if (_pickableHeld == null)
            return;

        if (_pickableHeld is S_Weapon)
            return;

        _pickableHeld.PutDown();
        _pickableHeld = null;
        OnPickUp.Invoke(null);
    }

    private S_Interactable CheckObjectRaycast()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, 2f, objectLayer))
        {
            return hit.collider.GetComponent<S_Interactable>();
        }

        return null;
    }

    void Update()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, 2f, objectLayer))
        {
            MeshRenderer renderer = hit.collider.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material[] materials = renderer.materials;

                if (materials.Length > 1)
                {
                    if (materials[1].HasProperty("_Scale"))
                    {
                        materials[1].SetFloat("_Scale", 1.05f);
                        if (_lastRenderer != null && materials[1] != _lastRenderer)
                        {
                            _lastRenderer.SetFloat("_Scale", 1f);
                        }

                        _lastRenderer = materials[1];
                    }
                }
            }
        }
        else if (_lastRenderer != null)
        {
            _lastRenderer.SetFloat("_Scale", 1f);
            _lastRenderer = null;
        }
    }

    private void Throw()
    {
        if (_pickableHeld == null || !_pickableHeld.throwable)
            return;

        Vector3 throwDirection = _cameraTransform.forward.normalized;
        throwDirection = Quaternion.AngleAxis(_throwAngle, _cameraTransform.right) * throwDirection;

        ulong objectId = _pickableHeld.NetworkObjectId;

        if (_pickableHeld.TryGetComponent(out Rigidbody clientRb))
        {
            clientRb.AddForce(throwDirection * _throwForce, ForceMode.Impulse);
        }

        PutDownPickable();

        ApplyImpulseServerRpc(objectId, throwDirection);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyImpulseServerRpc(ulong p_objectId, Vector3 p_throwDirection)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(p_objectId, out NetworkObject networkObject))
        {
            if (networkObject.TryGetComponent(out Rigidbody rb))
            {
                if (networkObject.TryGetComponent(out S_Pickable pickable))
                {
                    pickable.PutDown();
                }

                rb.AddForce(p_throwDirection * _throwForce, ForceMode.Impulse);
                ThrowObjectClientRpc(p_objectId, p_throwDirection);
            }
            else
            {
                Debug.LogWarning($"Object with ID {p_objectId} does not have a Rigidbody component.");
            }
        }
    }

    [ClientRpc]
    private void ThrowObjectClientRpc(ulong p_objectId, Vector3 p_throwDirection)
    {
        if (NetworkManager.Singleton.IsServer || GetComponentInParent<NetworkObject>().IsOwner) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(p_objectId, out NetworkObject networkObject))
        {
            if (networkObject.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(p_throwDirection * _throwForce, ForceMode.Impulse);
            }
        }
    }
}