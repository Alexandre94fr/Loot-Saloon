using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public abstract class S_Pickable : S_Interactable
{
    [SerializeField] private Vector3 _onPickUpOffset = Vector3.forward;
    [SerializeField] private float _pickUpTime = 2f;
    private bool _isPickUp = false;
    [Range(0f, 20f)] public float weight = 0f;

    private List<Collider> _ignoredColliders = new();

    public S_Cart cart { get; private set; }
    public void SetCart(S_Cart cart)
    {
        this.cart = cart;
    }
    public bool IsEasyToPickUp(S_PlayerInteract player)
    {
        if (cart == null || cart.KnowPlayer(player))
            return true;
        return false;
    }

    public override void StopInteract(S_PlayerInteract p_playerInteract) 
    {
        _isPickUp = false;
        S_CircleLoad.OnCircleChange(0);
    }
    public override void Interact(S_PlayerInteract p_playerInteract)
    {
        if (IsEasyToPickUp(p_playerInteract))
        {
            PickUp(p_playerInteract);
            return;
        }
        StartCoroutine(InteractCoroutine(p_playerInteract));
    }

    private IEnumerator InteractCoroutine(S_PlayerInteract p_playerInteract)
    {
        float timer = 0f;
        _isPickUp = true;

        while (timer < _pickUpTime)
        {
            if (!_isPickUp)
                yield break;

            S_CircleLoad.OnCircleChange(timer / _pickUpTime);
            timer += Time.deltaTime;
            yield return null;
        }
        PickUp(p_playerInteract);
    }

    private void PickUp(S_PlayerInteract p_playerInteract)
    {
        if (!interactable)
            return;
        interactable = false;

        _body.isKinematic = true;

        _transform.SetParent(p_playerInteract.transform, false);
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
