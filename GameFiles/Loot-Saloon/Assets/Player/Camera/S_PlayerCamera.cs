using UnityEngine;

public class S_PlayerCamera : MonoBehaviour
{
    private Vector2 _lookInput;
    private float sensitivity = 100f;
    private float xRotation = 0f;
    private Transform _playerTransform;
    void Start()
    {
        _playerTransform = GameObject.Find("PlayerCharacter").transform;
        Cursor.lockState = CursorLockMode.Locked;
        S_PlayerInputsReciever.OnLook += GetLookInput;
    }

    void Update()
    {
        float mouseX = _lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = _lookInput.y * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        _playerTransform.Rotate(Vector3.up * mouseX);
    }

    private void GetLookInput(Vector2 lookInput)
    {
        _lookInput = lookInput;
    }
}
