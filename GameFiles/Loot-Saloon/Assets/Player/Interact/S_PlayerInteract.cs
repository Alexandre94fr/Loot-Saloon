#region
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
#endregion

[RequireComponent(typeof(SphereCollider))]
public class S_PlayerInteract : MonoBehaviour
{
    // [SerializeField] private GameObject _interactPanel;

    private Transform _transform;
    private Transform _cameraTransform;
    public Transform _armTransform;
    private S_Pickable _pickableHeld = null;

    public UnityEvent<S_Pickable> OnPickUp = new();

    public UnityEvent<Transform, S_Weapon> OnWeaponPickUp = new();
    [Tooltip("When pickung up a pickable, collisions between the pickable's colliders and these colliders will be disabled.")]
    public List<Collider> pickableIgnoresColliders = new();

    [SerializeField] [Range(0, 20)] private float _throwForce = 10;
    [SerializeField] private float _throwAngle = 0f;

    public LayerMask objectLayer;

    private Material _lastRenderer;

    private void Awake()
    {
        _transform = transform;
        _cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        if (!transform.parent.parent.GetComponent<NetworkObject>().IsOwner)
            return;
        

        S_PlayerInputsReciever.OnInteract += Interact;
        S_PlayerInputsReciever.OnThrow += Throw;
        S_LifeManager.OnDie += PutDownPickable;
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
        if (_pickableHeld == null) return;
        if (_pickableHeld is S_Weapon) return;

        _pickableHeld.PutDown();
        _pickableHeld = null;
        OnPickUp.Invoke(null);
    }

    private S_Pickable CheckObjectRaycast()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, 2f, objectLayer))
        {
            return hit.collider.GetComponent<S_Pickable>();
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
        if (_pickableHeld == null)
            return;

        S_Pickable pickable = _pickableHeld;
        PutDownPickable();

        Vector3 throwDirection = _cameraTransform.forward.normalized;

        throwDirection = Quaternion.AngleAxis(_throwAngle, _cameraTransform.right) * throwDirection;

        Rigidbody rb = pickable.GetComponent<Rigidbody>();
        rb.AddForce(throwDirection * _throwForce, ForceMode.Impulse);
    }
}