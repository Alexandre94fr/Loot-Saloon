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
        
        _body.isKinematic = true;

        if (!(this is S_Weapon))
        {
            
            _transform.SetParent(p_playerInteract.transform, false);
        }
        

        _transform.localPosition = _onPickUpOffset;
        _transform.rotation = Quaternion.Euler(p_playerInteract.transform.rotation.eulerAngles + new Vector3(0, 180, 0));


        foreach (Collider colliderToIgnore in p_playerInteract.pickableIgnoresColliders)
        {
            Physics.IgnoreCollision(colliderToIgnore, _collider, true);
            _ignoredColliders.Add(colliderToIgnore);
        }
    }

    public virtual void PutDown()
    {
        interactable = true;
        _body.isKinematic = false;

        _transform.SetParent(null, true);

        foreach (Collider colliderToIgnore in _ignoredColliders)
            Physics.IgnoreCollision(colliderToIgnore, _collider, false);
        _ignoredColliders.Clear();
    }
}
