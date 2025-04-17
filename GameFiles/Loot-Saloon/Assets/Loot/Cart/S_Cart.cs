#region
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    private FixedJoint _joint;
    private GameObject _attachPoint;

    private Rigidbody _cartRb;
    private bool _isCarried = false;
    private Vector3 _direction;
    private Transform _parent;

    [SerializeField] float followDistance = 3f;

    [SerializeField] private Vector3 _cartOffset = new Vector3(1, 0, 2); // Ajuster selon la position souhait�e


    private void MoveCart(Vector3 dir)
    {
        Debug.Log("Move The Fucking cart");
        if (!_isCarried) return;

        //if (_cartRb != null)
        {
            // Appliquer un d�placement plus lisse en ajustant la vitesse
            _direction = dir;
            Debug.Log("with RB :: " + dir);
        }
        //else
        //{
        //    // D�placement avec interpolation si pas de Rigidbody
        //    transform.position = Vector3.Lerp(transform.position, transform.position + dir * 0.1f, Time.deltaTime * 10f);
        //    Debug.Log("without RB");
        //}
    }

    private IEnumerator MoveCoroutine()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Cart Rigidbody is missing.");
            yield break;
        }

        float moveSpeed = 5f;
        float turnSmoothness = 2f; // Plus petit = plus lent à tourner

        Vector3 smoothedForward = _parent.forward;

        while (_isCarried)
        {
            if (_direction.sqrMagnitude > 0.01f)
            {
                // Calcule direction du regard sans Y
                Vector3 targetForward = _parent.forward;
                targetForward.y = 0;
                targetForward.Normalize();

                // Interpolation douce de la direction
                smoothedForward = Vector3.Slerp(smoothedForward, targetForward, Time.deltaTime * turnSmoothness);

                // Calcule position cible avec offset devant
                Vector3 targetPos = _parent.position + smoothedForward * followDistance;
                targetPos.y = rb.position.y;

                rb.MovePosition(Vector3.Lerp(rb.position, targetPos, Time.deltaTime * moveSpeed));

                // Rotation en direction inverse pour que le cart regarde le joueur (cul vers joueur)
                Quaternion targetRot = Quaternion.LookRotation(-smoothedForward, Vector3.up);
                rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRot, Time.deltaTime * turnSmoothness));
            }

            yield return null;
        }
    }

    // Lors de la prise du cart
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


        //// Cr�ation du point d'attache pour le cart
        //_attachPoint = new GameObject("CartAttachPoint");
        //_attachPoint.transform.position = p_parent.position;
        //_attachPoint.transform.rotation = p_parent.rotation;

        //var rbAttach = _attachPoint.AddComponent<Rigidbody>();
        //rbAttach.isKinematic = true;

        // Attach le cart au joueur via un joint physique
        //_cartRb = GetComponent<Rigidbody>();
        //_cartRb.isKinematic = false;  // D�sactive la physique sur le cart

        //_joint = gameObject.AddComponent<FixedJoint>();
        //_joint.connectedBody = rbAttach;
        //_joint.breakForce = Mathf.Infinity;
        //_joint.breakTorque = Mathf.Infinity;

        // Ajout d'un syst�me pour suivre le joueur
        //StartCoroutine(FollowPlayer(p_parent));
    }

    // D�placement fluide du cart avec le joueur
    private IEnumerator FollowPlayer(Transform playerTransform)
    {
        while (_isCarried)
        {
            // D�placer le cart de mani�re fluide
            transform.position = playerTransform.position + playerTransform.forward * _cartOffset.z + playerTransform.right * _cartOffset.x;

            // Optionnel : appliquer rotation synchronis�e avec le joueur
            transform.rotation = Quaternion.Euler(0, playerTransform.eulerAngles.y, 0);

            yield return null;
        }
    }

    // Lors du l�cher du cart
    public override void PutDown()
    {
        base.PutDown();

        _isCarried = false;

        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }

        if (_cartRb != null)
        {
            _cartRb.isKinematic = false;  // R�active la physique du cart
            _cartRb.useGravity = true;  // Permet au cart de tomber � nouveau
        }

        if (_attachPoint != null)
        {
            Destroy(_attachPoint);
            _attachPoint = null;
        }
        _parent.parent.GetComponentInChildren<S_PlayerController>().EnableCartMode(false);
    }
private void Update()
    {
        //if (_isCarried)
        //{
        //    // D�placer le cart en m�me temps que le joueur
        //    if (_joint != null)
        //    {
        //        // Positionner le cart devant le joueur, avec un petit offset
        //        transform.position = _attachPoint.transform.position + _cartOffset;
        //        transform.rotation = _attachPoint.transform.rotation;
        //    }
        //}
    }

    public override void Interact(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        if (IsEasyToPickUp(p_playerInteract))
        {
            PickUp(p_playerInteract, p_parent);
        }
    }

    private void AttachToPlayer(S_PlayerInteract p_playerInteract, Transform p_parent)
    {
        if (!interactable) return;

        interactable = false;

        Rigidbody myRb = GetComponent<Rigidbody>();
        Rigidbody handRb = p_parent.GetComponentInParent<Rigidbody>();

        if (myRb == null || handRb == null)
        {
            Debug.LogWarning("Missing rigidbody for FixedJoint attachment.");
            return;
        }

        // D�sactiver la gravit� et rendre les objets kinematic avant l'attachement
        if (myRb != null)
        {
            myRb.useGravity = false;  // D�sactiver la gravit� pour �viter que le cart ne tombe
            myRb.isKinematic = true;  // Rendre le cart kinematic pendant l'attachement
        }

        if (handRb != null)
        {
            handRb.isKinematic = true;  // Rendre le rigidbody du joueur kinematic pendant l'attachement
        }

        // Positionner le cart devant le joueur
        Vector3 desiredPosition = p_parent.position + p_parent.forward * _cartOffset.z + p_parent.right * _cartOffset.x + p_parent.up * _cartOffset.y;
        p_parent.position = desiredPosition;

        // Cr�er le FixedJoint
        _joint = gameObject.AddComponent<FixedJoint>();
        _joint.connectedBody = handRb;
        _joint.breakForce = Mathf.Infinity;
        _joint.breakTorque = Mathf.Infinity;

        // R�activer la gravit� et le mouvement apr�s l'attachement
        if (myRb != null)
        {
            myRb.isKinematic = false;  // Restaurer le mouvement physique du cart
            myRb.useGravity = true;    // Restaurer la gravit� du cart
        }

        if (handRb != null)
        {
            handRb.isKinematic = false;  // Restaurer le mouvement physique du joueur
        }

        // Ignorer les collisions comme pr�c�demment
        foreach (Collider colliderToIgnore in p_playerInteract.pickableIgnoresColliders)
        {
            foreach (Collider collider in _colliders)
                Physics.IgnoreCollision(colliderToIgnore, collider, true);
            _ignoredColliders.Add(colliderToIgnore);
        }
    }

    private IEnumerator SynchronizeCartMovement(Transform p_parent)
    {
        while (!interactable)
        {
            // Synchronisation de la position
            Vector3 targetPosition = p_parent.position;
            transform.position = targetPosition;

            // Synchronisation de la rotation
            Quaternion targetRotation = p_parent.rotation;
            transform.rotation = targetRotation;

            yield return null;
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
}