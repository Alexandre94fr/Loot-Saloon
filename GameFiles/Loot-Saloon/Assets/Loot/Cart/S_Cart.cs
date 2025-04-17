#region
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
#endregion

public class S_Cart : S_Pickable
{
    public int total { get; private set; } = 0;
    public E_PlayerTeam team;

    public static event Action<E_PlayerTeam, int> GetCartValue;

    private HashSet<S_Loot> _inCart = new();

    [SerializeField] private GameObject slot;
    [SerializeField] private UnityEvent OnLootAdded = new();
    [SerializeField] private UnityEvent OnLootRemoved = new();

    private Rigidbody _cartRb;

    private bool _isCarried = false;
    private Transform _parent;

    [SerializeField] float followDistance = 3f;

    /// Header Modif
    /// On déplace uniquement le cart du côté du owner, et on synchronise la position via ServerRpc
    /// pour éviter les désynchronisations entre clients.
    /// End Modif

    private IEnumerator MoveCoroutine()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Cart Rigidbody is missing.");
            yield break;
        }

        float moveSpeed = 5f;
        float rotationSmoothness = 5f;

        while (_isCarried)
        {
            Vector3 forward = _parent.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 targetPosition = _parent.position + forward * followDistance;
            targetPosition.y = rb.position.y;

            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.deltaTime * moveSpeed));
            Quaternion targetRotation = Quaternion.LookRotation(-forward, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * rotationSmoothness));

            if (IsOwner) /**/
            {
                SyncCartPositionServerRpc(rb.position, rb.rotation); /**/
            }

            yield return null;
        }
    }

    /// Header Modif
    /// Cette méthode est appelée uniquement par le owner pour synchroniser la position du cart.
    /// Elle s'exécute côté serveur, et réplique la position/rotation à tous les autres clients.
    /// End Modif
    [ServerRpc]
    private void SyncCartPositionServerRpc(Vector3 position, Quaternion rotation) /**/
    {
        transform.SetPositionAndRotation(position, rotation); /**/
    }

    protected override void PickUp(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        Debug.Log("Cart physics");

        if (_isCarried)
            return;

        _parent = p_parent;
        _isCarried = true;

        p_parent.parent.GetComponentInChildren<S_PlayerController>().EnableCartMode(true, transform);

        S_PlayerInputsReciever.OnMove += MoveCart;
        StartCoroutine(MoveCoroutine());
        var playerController = _parent.parent.GetComponentInChildren<S_PlayerController>();
        playerController.EnableCartModeClientRpc(true, GetComponent<NetworkObject>());
    }

    [ClientRpc]
    private void PutDownClientRpc()
    {
        if (!IsOwner) return;
        var playerController = _parent?.parent.GetComponentInChildren<S_PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController is null.");
            return;
        }
        playerController.EnableCartMode(false);
    }

    public override void PutDown()
    {
        base.PutDown();

        Debug.Log("_isCarried: " + _isCarried);
        Debug.Log("_cartRb: " + _cartRb);
        Debug.Log("_parent: " + _parent);

        if (_cartRb != null)
        {
            _cartRb.isKinematic = false;  // Réactive la physique du cart
            _cartRb.useGravity = true;  // Permet au cart de tomber à nouveau
        }

        PutDownClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickUpServerRpc(ulong playerId)
    {
        var player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
        var interact = player.GetComponentInChildren<S_PlayerInteract>();
        var parent = interact.transform;

        PickUp(interact, parent);
    }

    public override void Interact(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        if (IsEasyToPickUp(p_playerInteract))
        {
            if (!IsServer)
            {
                ulong clientId = p_playerInteract.GetComponentInParent<NetworkObject>().OwnerClientId;
                RequestPickUpServerRpc(clientId);
                return;
            }

            PickUp(p_playerInteract, p_parent);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        S_Extract.OnExtract += EndGameEvent;
    }

    private void EndGameEvent(E_PlayerTeam winner)
    {
        GetCartValue?.Invoke(team, total);
    }

    private void OnTriggerEnter(Collider p_collider)
    {
        if (p_collider.TryGetComponent(out S_Loot loot) && !_inCart.Contains(loot))
        {
            _inCart.Add(loot);
            total += loot.properties.moneyValue;
            OnLootAdded.Invoke();
            loot.SetCart(this);
        }
    }

    private void OnTriggerExit(Collider p_collider)
    {
        if (p_collider.TryGetComponent(out S_Loot loot) && _inCart.Contains(loot))
        {
            _inCart.Remove(loot);
            total -= loot.properties.moneyValue;
            OnLootRemoved.Invoke();
            loot.SetCart(null);
        }
    }

    public void SetTextToTotal(TMP_Text text)
    {
        text.text = total.ToString();
    }

    /// Header Modif
    /// Méthode vide volontaire : uniquement utilisée pour s’abonner au move input mais le vrai mouvement est calculé dans MoveCoroutine.
    /// End Modif
    private void MoveCart(Vector3 dir)
    {
        if (!_isCarried) return;
    }
}