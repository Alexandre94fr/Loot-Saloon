#region
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

public abstract class S_Pickable : S_Interactable
{
    [SerializeField] private Vector3 _onPickUpOffset = Vector3.forward;
    public bool parentIsPlayerInteract = false;

    [SerializeField] private float _pickUpTime = 2f;
    private bool _isPickUp = false;
    [Range(0f, 20f)] public float weight = 0f;

    private List<Collider> _ignoredColliders = new();

    public bool throwable = true;


    public S_Cart cart { get; private set; }

    public void SetCart(S_Cart p_cart)
    {
        this.cart = p_cart;
    }

    public bool IsEasyToPickUp(S_PlayerInteract p_player)
    {
        return cart == null || cart.team == p_player.attributes.team;
    }

    public override void StopInteract(S_PlayerInteract p_playerInteract)
    {
        _isPickUp = false;
        S_CircleLoad.OnCircleChange(0);
    }

    public override void Interact(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        if (IsEasyToPickUp(p_playerInteract))
        {
            PickUp(p_playerInteract, p_parent);
            return;
        }

        StartCoroutine(InteractCoroutine(p_playerInteract, p_parent));
    }

    private IEnumerator InteractCoroutine(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        float timer = 0f;
        _isPickUp = true;

        while (timer < _pickUpTime)
        {
            if (!_isPickUp)
                yield break;

            S_CircleLoad.OnCircleChange?.Invoke(timer / _pickUpTime);

            timer += Time.deltaTime;
            yield return null;
        }

        PickUp(p_playerInteract, p_parent);
    }

    private void PickUp(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        if (!interactable)
            return;

        interactable = false;

        _body.isKinematic = true;

        Transform handTransform = p_parent;
        // _transform.SetParent(p_parent, false);
        _transform.localPosition = _onPickUpOffset;

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