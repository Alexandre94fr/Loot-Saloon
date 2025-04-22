using UnityEngine;

public class S_PlayerCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private float _sensitivity = 50f;

    [Header("Cart Mode")]
    [SerializeField] private float _maxYawOffset = 45f;

    private Vector2 _lookInput;
    private float _xRotation = 0f;

    private Transform _playerTransform;

    private bool _hasCart = false;
    private Transform _cartReference = null;
    private float _cartYawOffset = 0f;

    public void SetPlayerTransform(Transform player)
    {
        _playerTransform = player;
    }

    private void Start()
    {
        _playerTransform = transform.parent.transform;
        Cursor.lockState = CursorLockMode.Locked;
        S_PlayerInputsReciever.OnLook += GetLookInput;
    }

    private void Update()
    {
        float mouseX = _lookInput.x * _sensitivity * Time.deltaTime;
        float mouseY = _lookInput.y * _sensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        if (_hasCart && _cartReference != null)
        {
            _cartYawOffset = Mathf.Clamp(_cartYawOffset + mouseX, -_maxYawOffset, _maxYawOffset);

            // Applique le pitch (haut/bas) et yaw offset limité
            transform.localRotation = Quaternion.Euler(_xRotation, _cartYawOffset, 0f);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            _playerTransform.Rotate(Vector3.up * mouseX);
        }
    }

    public void EnableCartMode(bool enable, Transform cart = null)
    {
        _hasCart = enable;
        _cartReference = enable ? cart : null;
        _cartYawOffset = 0f;
    }

    /// <summary>
    /// À appeler par le PlayerController pour déplacer le joueur dans la bonne direction.
    /// </summary>
    public Vector3 GetMovementDirection(Vector2 moveInput)
    {
        Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y);

        if (_hasCart && _cartReference != null)
        {
            return _cartReference.TransformDirection(inputDir.normalized);
        }
        else
        {
            if (_playerTransform == null)
            {
                Debug.LogError("S_PlayerCamera :: _playerTransform is null in GetMovementDirection()");
                return Vector3.zero;
            }
            return _playerTransform.TransformDirection(inputDir.normalized);
        }
    }

    private void GetLookInput(Vector2 lookInput)
    {
        _lookInput = lookInput;
    }
}