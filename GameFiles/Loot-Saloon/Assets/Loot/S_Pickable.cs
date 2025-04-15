#region
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion
using Unity.Netcode;

public abstract class S_Pickable : S_Interactable
{
    [SerializeField] private Vector3 _onPickUpOffset = Vector3.forward;
    public bool parentIsPlayerInteract = false;

    [SerializeField] private float _pickUpTime = 2f;
    private bool _isPickUp = false;
    [Range(0f, 20f)] public float weight = 0f;

    private List<Collider> _ignoredColliders = new();

    public S_Cart cart { get; private set; }

    public void SetCart(S_Cart p_cart)
    {
        this.cart = p_cart;
    }

    public bool IsEasyToPickUp(S_PlayerInteract p_player)
    {
        return cart == null || cart.KnowPlayer(p_player);
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
            RequestPickupServerRpc(
                p_playerInteract.transform.parent.parent.GetComponent<NetworkObject>().OwnerClientId,
                p_playerInteract.transform.parent.parent.GetComponent<NetworkObject>().NetworkObjectId
            );
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

            S_CircleLoad.OnCircleChange(timer / _pickUpTime);
            timer += Time.deltaTime;
            yield return null;
        }

        RequestPickupServerRpc(
            p_playerInteract.transform.parent.parent.GetComponent<NetworkObject>().OwnerClientId,
            p_playerInteract.transform.parent.parent.GetComponent<NetworkObject>().NetworkObjectId
        );
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ulong clientId, ulong playerNetworkObjectId)
    {
        // Vérifie si l'objet est bien spawné
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            // Change ownership depuis le serveur
            netObj.ChangeOwnership(clientId);
        }

        // Notifie tous les autres clients de suivre visuellement
        FollowHandClientRpc(clientId, playerNetworkObjectId);
    }

    [ClientRpc]
    private void FollowHandClientRpc(ulong targetClientId, ulong playerNetworkObjectId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out NetworkObject playerNetObj))
        {
            // Change ce chemin si ton WeaponSlot est ailleurs
            //Transform handTransform = playerNetObj.transform.Find("Hand/WeaponSlot");
            Transform handTransform = Camera.main.transform.Find("Arms/Cube (1)");

            if (handTransform != null)
            {
                LocalPickUp(handTransform);
            }
            else
            {
                Debug.LogWarning("WeaponSlot non trouvé dans le joueur !");
            }
        }
    }

    private void LocalPickUp(Transform p_parent)
    {
        interactable = false;
        _body.isKinematic = true;

        if (!(this is S_Weapon))
        {
            _transform.localPosition = _onPickUpOffset;
        }

        StartCoroutine(FollowHandCoroutine(p_parent));
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
