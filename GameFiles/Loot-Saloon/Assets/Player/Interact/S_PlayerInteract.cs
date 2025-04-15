#region
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
#endregion

[RequireComponent(typeof(SphereCollider))]
public class S_PlayerInteract : MonoBehaviour
{
    // [SerializeField] private GameObject _interactPanel;

    private Transform _transform;
    private Transform _cameraTransform;
    private S_Pickable _pickableHeld = null;
    private S_Interactable _currentInteraction = null;


    public S_PlayerAttributes attributes {get; private set;}

    [SerializeField] private UnityEvent<S_Pickable> _onPickUp = new();

    [Tooltip("When pickung up a pickable, collisions between the pickable's colliders and these colliders will be disabled.")]
    public List<Collider> pickableIgnoresColliders = new();

    [SerializeField] [Range(0, 20)] private float _throwForce = 10;
    [SerializeField] private float _throwAngle = 0f;

    public LayerMask objectLayer;

    private Material _lastRenderer;

    private void Awake()
    {
        _transform = transform;
        _cameraTransform = Camera.main.transform;
        attributes = GetComponent<S_PlayerAttributes>();
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connecté : {clientId}");

        // Exemple : assigner une référence à un composant réseau spécifique
        // S_LootInstantiator inst = ...
        // inst.AssignToClient(clientId);

        // Tu peux aussi stocker une référence à ton player, ou lier des objets spécifiques
    }

    private void Start()
    {
        S_PlayerInputsReciever.OnInteract += Interact;
        S_PlayerInputsReciever.OnStopInteract += StopInteract;
        S_PlayerInputsReciever.OnThrow += Throw;
        S_LifeManager.OnDie += PutDownPickable;
    }
    private void StopInteract()
    {
        if (_currentInteraction == null)
            return;
        _currentInteraction.StopInteract(this);
        _currentInteraction = null;
    }

    private void Interact()
    {
        if (_pickableHeld != null)
        {
            PutDownPickable();
            return;
        }

        if (_pickableHeld != null)
        {
            foreach (var collider in _pickableHeld.GetComponents<Collider>())
            {
                collider.enabled = false;
            }
        }

        InteractWith(CheckObjectRaycast());

        if (_pickableHeld != null)
        {
            foreach (var collider in _pickableHeld.GetComponents<Collider>())
            {
                collider.enabled = true;
            }
        }
    }

    private void InteractWith(S_Interactable p_interactable)
    {
        if (p_interactable == null)
            return;

        Transform interactParent = _transform;
        _currentInteraction = p_interactable;

        if (p_interactable is S_Pickable pickable)
        {
            if (_pickableHeld != null)
                return;

            PickUp(pickable);
            interactParent = pickable.parentIsPlayerInteract ? _transform : _cameraTransform;
        }

        p_interactable.Interact(this, interactParent);
        S_PlayerInputsReciever.OnLook += CheckLookInteraction;
    }

    private void CheckLookInteraction(Vector2 _)
    {
        if (CheckObjectRaycast() == null)
        {
            StopInteract();
            S_PlayerInputsReciever.OnLook -= CheckLookInteraction;
        }
    }

    private void PickUp(S_Pickable p_pickable)
    {
        _pickableHeld = p_pickable;
        _onPickUp.Invoke(p_pickable);
    }

    private void PutDownPickable()
    {
        if (_pickableHeld == null) return;
        _pickableHeld.PutDown();
        _pickableHeld = null;
        _onPickUp.Invoke(null);
    }

    private S_Interactable CheckObjectRaycast()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, 2f, objectLayer))
        {
            return hit.collider.GetComponent<S_Interactable>();
        }

        return null;
    }

    void Update()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, 2f, objectLayer))
        {
            MeshRenderer renderer = hit.collider.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material[] materials = renderer.materials;

                if (materials.Length > 1)
                {
                    if (materials[1].HasProperty("_Scale"))
                    {
                        materials[1].SetFloat("_Scale", 1.05f);
                        if (_lastRenderer != null && materials[1] != _lastRenderer)
                        {
                            _lastRenderer.SetFloat("_Scale", 1f);
                        }

                        _lastRenderer = materials[1];
                    }
                }
            }
        }
        else if (_lastRenderer != null)
        {
            _lastRenderer.SetFloat("_Scale", 1f);
            _lastRenderer = null;
        }
    }

    private void Throw()
    {
        if (_pickableHeld == null || !_pickableHeld.throwable)
            return;

        S_Pickable pickable = _pickableHeld;
        PutDownPickable();

        Vector3 throwDirection = _cameraTransform.forward.normalized;

        throwDirection = Quaternion.AngleAxis(_throwAngle, _cameraTransform.right) * throwDirection;

        Rigidbody rb = pickable.GetComponent<Rigidbody>();
        rb.AddForce(throwDirection * _throwForce, ForceMode.Impulse);
    }
}