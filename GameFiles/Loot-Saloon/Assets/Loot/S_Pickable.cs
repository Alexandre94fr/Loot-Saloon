using UnityEngine;

public abstract class S_Pickable : S_Interactable
{
    [SerializeField] private Vector3 _onPickUpOffset = Vector3.forward;

    [Range(0f, 20f)] public float weight = 0f;

    public override void Interact(Transform p_caller)
    {
        if (!interactable)
            return;
        interactable = false;
        
        _trigger.enabled = false;
        _body.useGravity = false;
        _body.freezeRotation = true;
        _body.linearVelocity = Vector3.zero;

        _transform.SetParent(p_caller, false);
        _transform.localPosition = _onPickUpOffset;
    }

    public virtual void PutDown()
    {
        _trigger.enabled = true;
        _body.useGravity = true;
        _body.freezeRotation = false;
        _transform.SetParent(null, true);

        interactable = true;
    }
}
