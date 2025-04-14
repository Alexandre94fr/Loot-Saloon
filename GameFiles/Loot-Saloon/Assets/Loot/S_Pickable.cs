#region
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

public abstract class S_Pickable : S_Interactable
{
    [SerializeField] private Vector3 _onPickUpOffset = Vector3.forward;
    public bool parentIsPlayerInteract = false;

    [Range(0f, 20f)] public float weight = 0f;

    private List<Collider> _ignoredColliders = new();

    public override void Interact(S_PlayerInteract p_playerInteract, Transform p_transform)
    {
        if (!interactable)
            return;

        interactable = false;

        _body.isKinematic = true;

        Transform handTransform = p_transform;

        StartCoroutine(FollowHandCoroutine(handTransform));

        foreach (Collider colliderToIgnore in p_playerInteract.pickableIgnoresColliders)
        {
            foreach (Collider collider in _colliders)
                Physics.IgnoreCollision(colliderToIgnore, collider, true);
            _ignoredColliders.Add(colliderToIgnore);
        }
    }

    private IEnumerator FollowHandCoroutine(Transform p_handTransform)
    {
        while (!interactable)
        {
            transform.position = p_handTransform.position + p_handTransform.TransformDirection(_onPickUpOffset);
            transform.rotation = p_handTransform.rotation;
            yield return null;
        }
    }

    public virtual void PutDown()
    {
        interactable = true;
        _body.isKinematic = false;


        foreach (Collider colliderToIgnore in _ignoredColliders)
        {
            foreach (Collider collider in _colliders)
                Physics.IgnoreCollision(colliderToIgnore, collider, false);
        }
        _ignoredColliders.Clear();
    }
}