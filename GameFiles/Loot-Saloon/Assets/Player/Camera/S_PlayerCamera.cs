#region
using UnityEngine;
#endregion

public class S_PlayerCamera : MonoBehaviour
{
    private Vector2 _lookInput;
    private float _sensitivity = 100f;
    private float _xRotation = 0f;
    private Transform _playerTransform;

    void Start()
    {
        _playerTransform = transform.parent.transform;
        Cursor.lockState = CursorLockMode.Locked;
        S_PlayerInputsReciever.OnLook += GetLookInput;
    }

    void Update()
    {
        float mouseX = _lookInput.x * _sensitivity * Time.deltaTime;
        float mouseY = _lookInput.y * _sensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _playerTransform.Rotate(Vector3.up * mouseX);
    }

    private void GetLookInput(Vector2 p_lookInput)
    {
        _lookInput = p_lookInput;
    }
}