using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof (SphereCollider))]
public class S_PlayerInteract : MonoBehaviour
{
    // [SerializeField] private GameObject _interactPanel;

    private Transform _transform;
    private Transform _cameraTransform;
    private S_Pickable _pickableHeld = null;

    [SerializeField] private UnityEvent<S_Pickable> _onPickUp = new();

    [Tooltip("When pickung up a pickable, collisions between the pickable's colliders and these colliders will be disabled.")]
    public List<Collider> pickableIgnoresColliders = new();

    public LayerMask objectLayer;

    private void Awake()
    {
        _transform = transform;
        _cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        S_PlayerInputsReciever.OnInteract += Interact;
        S_LifeManager.OnDie += PutDownPickable;
    }
    
    private void Interact()
    {
        if (_pickableHeld != null)
        {
            PutDownPickable();
        }
        else
        {
            InteractWith(CheckObjectRaycast());
        }
    }

    private void InteractWith(S_Interactable p_interactable)
    {
        if (p_interactable == null)
            return;

        if (p_interactable is S_Pickable pickable)
        {
            if (_pickableHeld != null)
                return;
            
            PickUp(pickable);
        }

        p_interactable.Interact(this);
    }

    private void PickUp(S_Pickable p_pickable)
    {
        _pickableHeld = p_pickable;
        _onPickUp.Invoke(p_pickable);
    }

    private void PutDownPickable()
    {
        if (_pickableHeld == null) return;
        _pickableHeld.PutDown();
        _pickableHeld = null;
        _onPickUp.Invoke(null);
    }

    private S_Pickable CheckObjectRaycast()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward,out RaycastHit hit, 2f, objectLayer))
        {
            return hit.collider.GetComponent<S_Pickable>();
        }

        return null;
    }
}