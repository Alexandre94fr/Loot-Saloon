using UnityEngine;
using UnityEngine.UIElements;

public class S_PlayerController : MonoBehaviour
{
    private Transform _playerTransform;
    private Vector3 _playerDirection;
    public Vector3 boxExtents = new Vector3(0.4f, 0.05f, 0.4f);
    public LayerMask groundLayer;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float sprintSpeed = 4f;
    private float currentSpeed = 4f;
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
        Debug.Log(Grounded());
    }

    private void Jump()
    {
        if (Grounded())
            _playerTransform.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void Move()
    {
        _playerTransform.position += (transform.right * _playerDirection.x + transform.forward * _playerDirection.z)
                                      * Time.deltaTime * currentSpeed;
    }

    private void Sprint(bool sprint)
    {
        currentSpeed = sprint ? sprintSpeed : walkSpeed;
    }

    private void GetDirection(Vector3 playerDirection)
    {
        _playerDirection.x = playerDirection.x;
        _playerDirection.z = playerDirection.y;
    }
    
}
