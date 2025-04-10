using System.Collections.Generic;
using UnityEngine;

public abstract class S_Pickable : S_Interactable
{
    [SerializeField] private Vector3 _onPickUpOffset = Vector3.forward;
    [Range(0f, 20f)] public float weight = 0f;

    private List<Collider> _ignoredColliders = new();

    public override void Interact(S_PlayerInteract p_playerInteract)
    {
        if (!interactable)
            return;
        interactable = false;
        
        _trigger.enabled = false;
        _body.isKinematic = true;

        _transform.SetParent(p_playerInteract.transform, false);
        _transform.localPosition = _onPickUpOffset;
        _transform.rotation = p_playerInteract.transform.rotation;

        foreach (Collider colliderToIgnore in p_playerInteract.pickableIgnoresColliders)
        {
            Physics.IgnoreCollision(colliderToIgnore, _collider, true);
            Physics.IgnoreCollision(colliderToIgnore, _trigger, true);

            _ignoredColliders.Add(colliderToIgnore);
        }
    }

    public virtual void PutDown()
    {
        interactable = true;

        _trigger.enabled = true;
        _body.isKinematic = false;

        _transform.SetParent(null, true);

        foreach (Collider colliderToIgnore in _ignoredColliders)
        {
            Physics.IgnoreCollision(colliderToIgnore, _collider, false);
            Physics.IgnoreCollision(colliderToIgnore, _trigger, false);
        }
        _ignoredColliders.Clear();
    }
}
