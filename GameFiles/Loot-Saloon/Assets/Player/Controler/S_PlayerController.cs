using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class S_PlayerController : MonoBehaviour
{
    private Transform _playerTransform;
    [SerializeField] private Animator _armsAnimator;
    [SerializeField] private Transform _respawnPoint;
    [SerializeField] private GameObject _armsHandler;
    
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
        HandleInputsEvents();
        S_LifeManager.OnDie += Respawn;
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
        _armsAnimator.speed = sprint ? 2 : 1;
    }

    private void GetDirection(Vector3 playerDirection)
    {
        _playerDirection.x = playerDirection.x;
        _playerDirection.z = playerDirection.y;
        if (_playerDirection.x != 0 || _playerDirection.z != 0)
        {
            _armsAnimator.SetBool("Walking", true);
        }
        else
        {
            _armsAnimator.SetBool("Walking", false);
        }
    }
    
    public void OnObjectPickedUp(S_Pickable p_pickable)
    {
        // TODO change 20f to the actual player strength
        _speedMult = p_pickable == null ? 1f : 1f - Mathf.Clamp(p_pickable.weight / 20f, 0f, 1f);
        Sprint(_isSprinting);
    }

    private void Respawn()
    {
        DropInputsEvents();
        DisableAllMeshOfPlayer();
        StartCoroutine(RespawnCoroutine());
    }
    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(5);
        _playerTransform.position = _respawnPoint.position;
        EnableAllMeshOfPlayer();
        HandleInputsEvents();
    }

    private void HandleInputsEvents()
    {
        S_PlayerInputsReciever.OnJump += Jump;
        S_PlayerInputsReciever.OnMove += GetDirection;
        S_PlayerInputsReciever.OnSprint += Sprint;
    }

    private void DropInputsEvents()
    {
        S_PlayerInputsReciever.OnJump -= Jump;
        S_PlayerInputsReciever.OnMove -= GetDirection;
        S_PlayerInputsReciever.OnSprint -= Sprint;
        _playerDirection = Vector3.zero;
    }

    private void DisableAllMeshOfPlayer()
    {
        _playerTransform.GetComponent<MeshRenderer>().enabled = false;
        _armsHandler.SetActive(false);
        _armsAnimator.enabled = false;

    }
    
    private void EnableAllMeshOfPlayer()
    {
        _playerTransform.GetComponent<MeshRenderer>().enabled = true;
        _armsHandler.SetActive(true);
        _armsAnimator.enabled = true;
    }
}
