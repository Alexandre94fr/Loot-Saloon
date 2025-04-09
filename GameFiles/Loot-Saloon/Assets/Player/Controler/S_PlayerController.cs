using UnityEngine;
using UnityEngine.UIElements;

public class S_PlayerController : MonoBehaviour
{
    private Transform _playerTransform;
    private Vector3 _playerDirection;
    public Vector3 boxExtents = new Vector3(0.4f, 0.05f, 0.4f);
    public LayerMask groundLayer;
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _sprintSpeed = 4f;
    private float _currentSpeed = 4f;

    private bool _isSprinting = false;
    private float _speedMult = 1f;

    void Start()
    {
        _playerTransform = GameObject.Find("PlayerCharacter").transform;
        S_PlayerInputsReciever.OnJump += Jump;
        S_PlayerInputsReciever.OnMove += GetDirection;
        S_PlayerInputsReciever.OnSprint += Sprint;
    }

    private bool Grounded()
    {
        Vector3 boxCenter = _playerTransform.position + Vector3.down * (_playerTransform.localScale.y / 2);

        return Physics.CheckBox(
            boxCenter,
            boxExtents,
            Quaternion.identity,
            groundLayer
        );
    }

    void Update()
    {
        Move();
        // Debug.Log(Grounded());
    }

    private void Jump()
    {
        if (Grounded())
            _playerTransform.GetComponent<Rigidbody>().AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    private void Move()
    {
        _playerTransform.position += (transform.right * _playerDirection.x + transform.forward * _playerDirection.z)
                                      * Time.deltaTime * _currentSpeed;
    }

    private void Sprint(bool sprint)
    {
        _currentSpeed = (sprint ? _sprintSpeed : _walkSpeed) * _speedMult;
        _isSprinting = sprint;
    }

    private void GetDirection(Vector3 playerDirection)
    {
        _playerDirection.x = playerDirection.x;
        _playerDirection.z = playerDirection.y;
    }
    
    public void OnObjectPickedUp(S_Pickable p_pickable)
    {
        // TODO change 20f to the actual player strength
        _speedMult = p_pickable == null ? 1f : 1f - Mathf.Clamp(p_pickable.weight / 20f, 0f, 1f);
        Sprint(_isSprinting);
    }
}
