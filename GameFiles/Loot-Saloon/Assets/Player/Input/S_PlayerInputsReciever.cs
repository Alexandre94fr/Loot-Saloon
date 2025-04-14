using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class S_PlayerInputsReciever : MonoBehaviour
{
    public static event Action OnJump;
    public static event Action<Vector3> OnMove;
    public static event Action<Vector2> OnLook;
    public static event Action<bool> OnSprint;
    public static event Action OnInteract;
    public static event Action<Vector2> OnScroll;
    public static event Action OnThrow;

    public void JumpInput(InputAction.CallbackContext p_context)
    {
        if (p_context.performed)
        {
            OnJump?.Invoke();
        }
    }

    public void MoveInput(InputAction.CallbackContext p_context)
    {
        OnMove?.Invoke(p_context.ReadValue<Vector2>());
    }

    public void LookInput(InputAction.CallbackContext p_context)
    {
        OnLook?.Invoke(p_context.ReadValue<Vector2>());
    }

    public void SprintInput(InputAction.CallbackContext p_context)
    {
        if (p_context.started)
        {
            OnSprint?.Invoke(true);
        }
        else if (p_context.canceled)
        {
            OnSprint?.Invoke(false);
        }
    }

    public void Interact(InputAction.CallbackContext p_context)
    {
        if (!p_context.performed)
            return;
        
        OnInteract?.Invoke();
    }

    public void Scroll(InputAction.CallbackContext p_context)
    {
        if (!p_context.started)
            return;
        
        OnScroll?.Invoke(p_context.ReadValue<Vector2>());
    }

    public void Throw(InputAction.CallbackContext p_context)
    {
        if (!p_context.started)
            return;
        
        OnThrow?.Invoke();
    }
}
