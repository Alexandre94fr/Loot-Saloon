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

    private List<S_Interactable> _interactables = new();
    private int _interactableIndex = -1;

    [SerializeField] private UnityEvent<S_Pickable> onPickUp = new();

    public LayerMask objectLayer;
    
    private Renderer _lastRenderer;
    
    public Material normalMaterial;
    public Material outlineMaterial;

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
        if (p_interactable is S_Pickable pickable)
        {
            if (_pickableHeld != null)
                return;
            
            PickUp(pickable);
        }

        _interactables.Remove(p_interactable);
        p_interactable.Interact(_transform);
    }

    private void PickUp(S_Pickable p_pickable)
    {
        _pickableHeld = p_pickable;
        onPickUp.Invoke(p_pickable);
    }

    private void PutDownPickable()
    {
        if (_pickableHeld == null) return;
        _pickableHeld.PutDown();
        _pickableHeld = null;
        onPickUp.Invoke(null);
    }

    private S_Pickable CheckObjectRaycast()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward,out RaycastHit hit, 1f, objectLayer))
        {
            return hit.collider.GetComponent<S_Pickable>();
        }

        return null;
    }

    void Update()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, 1f, objectLayer))
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                if (_lastRenderer != null && _lastRenderer != rend)
                    _lastRenderer.material = normalMaterial;

                rend.material = outlineMaterial;
                _lastRenderer = rend;
            }
        }
        else
        {
            if (_lastRenderer != null)
            {
                _lastRenderer.material = normalMaterial;
                _lastRenderer = null;
            }
        }
    }
}
