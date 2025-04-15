#region
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
#endregion

public class S_PlayerController : NetworkBehaviour
{
    [Header(" Debugging :")]
    [Tooltip("Allow the devs to test there scenes without having to pass throw the Lobby")]
    [SerializeField] private bool _isSoloTestModeEnabled = true;

    [Space]
    [SerializeField] private Animator _armsAnimator;
    [SerializeField] private Transform _respawnPoint;
    [SerializeField] private GameObject _armsHandler;

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
        if (!_isSoloTestModeEnabled)
            return;

        _playerTransform = transform.parent.transform;

        HandleInputsEvents();
        S_LifeManager.OnDie += Respawn;
        S_Extract.OnExtract += DisableAllMeshOfPlayer;
        S_Extract.OnExtract += DropInputsEvents;
    }

    public override void OnNetworkSpawn()
    {
        if (_isSoloTestModeEnabled)
            return;

        _playerTransform = transform.parent.transform;
        if (_playerTransform.parent.GetComponent<NetworkObject>().IsOwner)
        {
            HandleInputsEvents();
            S_LifeManager.OnDie += Respawn;
            S_Extract.OnExtract += DisableAllMeshOfPlayer;
            S_Extract.OnExtract += DropInputsEvents;
        }
        else
        {
            //Client Side
            GameObject camerObject = _playerTransform.GetComponentInChildren<Camera>().gameObject;
            camerObject.GetComponent<Camera>().enabled = false;
            camerObject.GetComponent<S_PlayerCamera>().enabled = false;
            camerObject.GetComponent<AudioListener>().enabled = false;
            camerObject.GetComponent<UniversalAdditionalCameraData>().enabled = false;
            _playerTransform.GetComponentInChildren<PlayerInput>().gameObject.SetActive(false);
        }
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
        if (_playerTransform == null)
        {
            Debug.LogError($"ERROR ! The '{nameof(_playerTransform)}' variable is null, " +
                $"to fix this problem you can try enabling the '{nameof(_isSoloTestModeEnabled)}' variable. " +
                "This bug may occur because you tried to launch your scene without passing throw the lobby scene.\n" +

                "The update loop will not go any further."
            );

            return;
        }

        Move();
    }

    private void Jump()
    {
        if (Grounded())
            _playerTransform.GetComponent<Rigidbody>().AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    private void Move()
    {
        _playerTransform.position += (transform.right * _playerDirection.x + transform.forward * _playerDirection.z) * (Time.deltaTime * _currentSpeed);
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

    private void DropInputsEvents(E_PlayerTeam team = E_PlayerTeam.NONE)
    {
        S_PlayerInputsReciever.OnJump -= Jump;
        S_PlayerInputsReciever.OnMove -= GetDirection;
        S_PlayerInputsReciever.OnSprint -= Sprint;
        _playerDirection = Vector3.zero;
    }

    private void DisableAllMeshOfPlayer(E_PlayerTeam team = E_PlayerTeam.NONE)
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