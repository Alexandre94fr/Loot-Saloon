using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof (SphereCollider))]
public class S_PlayerInteract : MonoBehaviour
{
    // [SerializeField] private GameObject _interactPanel;

    private Transform _transform;
    private S_Pickable _pickableHeld = null;

    private List<S_Interactable> _interactables = new();
    private int _interactableIndex = -1;

    [SerializeField] private UnityEvent<S_Pickable> OnPickUp = new();

    public UnityEvent<S_Weapon> OnWeaponPickUp = new();

    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
        S_PlayerInputsReciever.OnInteract += Interact;
        S_PlayerInputsReciever.OnScroll   += Scroll;
    }

    private void Scroll(Vector2 p_value)
    {
        if (_interactableIndex == -1)
            return;

        int direction = p_value.y == 0 ? 0 : (int) Mathf.Sign(p_value.y);
        print($"scroll direction: {direction}");
        S_Utils.ScrollIndex(ref _interactableIndex, _interactables.Count, direction);
        print($"can interact with {_interactables[_interactableIndex]}");
    }

    private void Interact()
    {
        if (_pickableHeld != null)
        {
            PutDownPickable();
        }
        else
        {
            if (_interactableIndex != -1)
                InteractWith(_interactables[_interactableIndex]);
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
        SetCorrectIndex();
        p_interactable.Interact(_transform);
    }

    private void PickUp(S_Pickable p_pickable)
    {
        _pickableHeld = p_pickable;

        if(p_pickable is S_Weapon)
        {
            OnWeaponPickUp.Invoke(p_pickable.GetComponent<S_Weapon>());
        }
        else
        {
            OnPickUp.Invoke(p_pickable);
        }

    }

    private void PutDownPickable()
    {
        _pickableHeld.PutDown();
        _pickableHeld = null;
        OnPickUp.Invoke(null);
    }

    private void OnTriggerEnter(Collider p_collider)
    {
        foreach (S_Interactable interactable in p_collider.GetComponents<S_Interactable>())
        {
            if (interactable.interactInstantly)
                InteractWith(interactable);
            else
                _interactables.Add(interactable);
        }

        SetCorrectIndex();
    }

    private void OnTriggerExit(Collider p_collider)
    {
        foreach (S_Interactable interactable in p_collider.GetComponents<S_Interactable>())
            _interactables.Remove(interactable);

        SetCorrectIndex();
    }

    private void SetCorrectIndex()
    {
        int old = _interactableIndex;

        if (_interactableIndex == -1)
            _interactableIndex = _interactables.Count != 0 ? 0 : -1;
        else
            _interactableIndex = _interactables.Count == 0 ? -1 : _interactableIndex;
        
        if (_interactableIndex != -1)
        {
            if (_interactableIndex != old)
            {
                //print($"can interact with {_interactables[_interactableIndex]}");
            }
        }
        else
        {
            //print("can't interact wit anything");
        }
    }
}