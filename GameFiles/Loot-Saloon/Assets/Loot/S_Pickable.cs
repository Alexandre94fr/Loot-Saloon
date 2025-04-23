#region
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
#endregion

public abstract class S_Pickable : S_Interactable
{
    [SerializeField] private Vector3 _onPickUpOffset = Vector3.forward;
    public bool parentIsPlayerInteract;

    [SerializeField] private float _pickUpTime = 2f;
    [SerializeField] private Canvas _worldCanvas;
    private bool _isPickUp;
    [Range(0f, 20f)] public float weight;

    protected List<Collider> _ignoredColliders = new();

    public bool throwable = true;


    public S_Cart cart { get; private set; }

    private Camera _currentCamera;

    public void SetWorldTextVisibility(bool visible, Camera cam)
    {
        if (_worldCanvas == null)
            return;

        _worldCanvas.gameObject.SetActive(visible);
        _currentCamera = visible ? cam : null;
    }

    private void LateUpdate()
    {
        if (_worldCanvas != null && _currentCamera != null)
        {
            // Reste au-dessus de l'objet
            _worldCanvas.transform.position = transform.position + Vector3.up * 0.5f;

            // Regarde la caméra
            Vector3 direction = _currentCamera.transform.position - _worldCanvas.transform.position;
            direction.y = 0f; // Ignore la hauteur si tu veux un look plus stable
            _worldCanvas.transform.rotation = Quaternion.LookRotation(-direction);
        }
    }

    public void SetCart(S_Cart p_cart)
    {
        this.cart = p_cart;
    }

    public bool IsEasyToPickUp(S_PlayerInteract p_player)
    {
        return cart == null || cart.team == p_player.attributes.Team;
    }

    public override void StopInteract(S_PlayerInteract p_playerInteract)
    {
        _isPickUp = false;
        S_PlayerUseUI.OnCircleChange?.Invoke(0);
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

            S_PlayerUseUI.OnCircleChange?.Invoke(timer / _pickUpTime);

            timer += Time.deltaTime;
            yield return null;
        }

        PickUp(p_playerInteract, p_parent);
    }

    protected virtual void PickUp(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        if (!interactable)
            return;

        interactable = false;

        Transform handTransform = p_parent;
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

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        DisablePhysicsClientRpc();

        while (!interactable)
        {
            Vector3 targetPosition = p_handTransform.position + p_handTransform.TransformDirection(_onPickUpOffset);
            Quaternion targetRotation = p_handTransform.rotation;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);


            if (Time.frameCount % 2 == 0)
            {
                if (IsServer)
                {
                    UpdateTransformClientRpc(targetPosition, targetRotation);
                }
                else
                {
                    UpdateTransformServerRpc(targetPosition, targetRotation);
                }
            }

            yield return null;
        }
    }

    [ClientRpc]
    private void DisablePhysicsClientRpc()
    {
        if (NetworkManager.Singleton.IsServer)
            return;

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if(TryGetComponent(out Collider collider) && TryGetComponent(out S_Cart _) == false)
        {
            collider.enabled = false;
        }
    }

    [ClientRpc]
    private void UpdateTransformClientRpc(Vector3 position, Quaternion rotation)
    {
        if (NetworkManager.Singleton.IsServer)
            return;

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = false;
        }
        if(TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }

        transform.position = position;
        transform.rotation = rotation;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateTransformServerRpc(Vector3 position, Quaternion rotation)
    {
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = false;
        }
        if(TryGetComponent(out Collider collider)  && TryGetComponent(out S_Cart _)==false)
        {
            collider.enabled = false;
        }
        // Update the transform on the server
        transform.position = position;
        transform.rotation = rotation;
    }

    public virtual void PutDown()
    {
        interactable = true;
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        foreach (Collider colliderToIgnore in _ignoredColliders)
        {
            foreach (Collider collider in _colliders)
                Physics.IgnoreCollision(colliderToIgnore, collider, false);
        }

        _ignoredColliders.Clear();

        ResetRigidbodyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetRigidbodyServerRpc()
    {
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        if(TryGetComponent(out Collider collider) && TryGetComponent(out S_Cart _)==false)
        {
            collider.enabled = true;
        }

        ResetRigidbodyClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    private void ResetRigidbodyClientRpc()
    {
        if (NetworkManager.Singleton.IsServer)
            return;

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = true;
        }
        if(TryGetComponent(out Collider collider) && TryGetComponent(out S_Cart _)==false)
        {
            collider.enabled = true;
        }
    }
}
