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
    public static event Action OnShoot;

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJump?.Invoke();
        }
    }

    public void MoveInput(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(context.ReadValue<Vector2>());
    }

    public void LookInput(InputAction.CallbackContext context)
    {
        OnLook?.Invoke(context.ReadValue<Vector2>());
    }

    public void SprintInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnSprint?.Invoke(true);
        }
        else if (context.canceled)
        {
            OnSprint?.Invoke(false);
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        
        OnInteract?.Invoke();
    }

    public void Scroll(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;
        
        OnScroll?.Invoke(context.ReadValue<Vector2>());
    }

    public void Throw(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;
        
        OnThrow?.Invoke();
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;

        OnShoot?.Invoke();
    }
}
