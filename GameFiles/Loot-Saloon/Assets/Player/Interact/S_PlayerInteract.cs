#region
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#endregion

[RequireComponent(typeof(SphereCollider))]
public class S_PlayerInteract : MonoBehaviour
{
    // [SerializeField] private GameObject _interactPanel;

    private Transform _transform;
    private Transform _cameraTransform;
    private S_Pickable _pickableHeld = null;
    private S_Interactable _currentInteraction = null;

    [SerializeField] private UnityEvent<S_Pickable> _OnPickUp = new();

    [Tooltip("When pickung up a pickable, collisions between the pickable's colliders and these colliders will be disabled.")]
    public List<Collider> pickableIgnoresColliders = new();

    [SerializeField] [Range(0, 20)] private float _throwForce = 10;
    [SerializeField] private Vector3 _throwAngle = new Vector3(0, 0.75f, 1);

    public LayerMask objectLayer;

    private Material _lastRenderer;

    private void Awake()
    {
        _transform = transform;
        _cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        S_PlayerInputsReciever.OnInteract += Interact;
        S_PlayerInputsReciever.OnStopInteract += StopInteract;
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

    private void StopInteract()
    {
        if (_currentInteraction == null)
            return;
        _currentInteraction.StopInteract(this);
        _currentInteraction = null;
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
        _pickableHeld = p_pickable;
        _OnPickUp.Invoke(p_pickable);
    }

    private void PutDownPickable()
    {
        if (_pickableHeld == null) return;
        _pickableHeld.PutDown();
        _pickableHeld = null;
        _OnPickUp.Invoke(null);
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
        if (_pickableHeld == null)
            return;

        S_Pickable pickable = _pickableHeld;
        PutDownPickable();

        Transform pickableTransform = pickable.transform;

        // since we rotate the object by 180 when picking it up,
        // rotate it back when throwing it
        // pickableTransform.rotation = Quaternion.Euler(pickableTransform.rotation.eulerAngles + new Vector3(0, 180, 0));
        pickable.GetComponent<Rigidbody>().AddForce(pickableTransform.rotation * _throwAngle * _throwForce, ForceMode.Impulse);
    }
}