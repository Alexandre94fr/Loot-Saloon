using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
public abstract class S_Interactable : MonoBehaviour
{
    public string interactName = "???";
    public bool interactInstantly = false;

    public bool interactable {get; protected set;} = true;

    [SerializeField] protected Collider _collider;

    protected Rigidbody _body;
    protected Transform _transform;

    public abstract void Interact(S_PlayerInteract p_playerInteract);
    public abstract void StopInteract(S_PlayerInteract p_playerInteract);

    protected virtual void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _transform = transform;
    }
}
