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

    private IEnumerator MoveCoroutine()
    {
        Debug.Log($"Je suis {NetworkManager.Singleton.LocalClientId} et le owner est {GetComponent<NetworkObject>().OwnerClientId}");


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

            if (IsOwner) 
            {
                SyncCartPositionServerRpc(rb.position, rb.rotation); 
            }

            yield return null;
        }
    }

    [ServerRpc]
    private void SyncCartPositionServerRpc(Vector3 position, Quaternion rotation) 
    {
        transform.SetPositionAndRotation(position, rotation); 
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
    private void PutDownClientRpc(ClientRpcParams rpcParams = default)
    {
        Debug.Log(" PutDownClientRpc received");
        Debug.Log($"[Client] Received PutDownClientRpc - IsOwner: {IsOwner}, IsClient: {IsClient}");

        var netObj = GetComponent<NetworkObject>();
        Debug.Log($"[Client] This Cart is owned by {netObj.OwnerClientId}, and I'm {NetworkManager.Singleton.LocalClientId}");

        var playerObject = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (playerObject == null)
        {
            Debug.LogError(" Local PlayerObject is null.");
            return;
        }

        var playerController = playerObject.GetComponentInChildren<S_PlayerController>();
        if (playerController == null)
        {
            Debug.LogError(" PlayerController is null.");
            return;
        }

        playerController.EnableCartModeClientRpc(false, GetComponent<NetworkObject>());
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestPutDownServerRpc()
    {
        Debug.Log($"[Server] Received PutDown request from client {OwnerClientId}");
        Debug.Log($"[Server] PutDown called on Cart by {OwnerClientId}. IsServer: {IsServer}");

        base.PutDown();

        if (!_isCarried) return;

        Debug.Log("Putting down cart");

        _isCarried = false;

        if (_cartRb == null)
            _cartRb = GetComponent<Rigidbody>();

        if (_cartRb != null)
        {
            _cartRb.isKinematic = false;
            _cartRb.useGravity = true;
        }

        S_PlayerInputsReciever.OnMove -= MoveCart;

        StopAllCoroutines();

        _parent = null;

        ClientRpcParams clientRpcParams = new()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };
        Debug.Log($"[Server] Sending PutDownClientRpc to OwnerClientId: {OwnerClientId}");
        PutDownClientRpc(clientRpcParams);
    }

    public override void PutDown()
    {
        RequestPutDownServerRpc();
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
    private void MoveCart(Vector3 dir)
    {
        if (!_isCarried) return;
    }
}