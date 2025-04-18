#region
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
#endregion

public class S_PlayerController : NetworkBehaviour
{
    public Vector3 boxExtents = new(0.4f, 0.05f, 0.4f);
    public LayerMask groundLayer;
    [HideInInspector] public Transform respawnPoint;

    [Header(" Debugging :")] 
    [Tooltip("Allow the devs to test there scenes without having to pass throw the Lobby")]
    [SerializeField] private bool _isSoloTestModeEnabled = true;

    [Space]
    [SerializeField] private Animator _armsAnimator;
    [SerializeField] private GameObject _armsHandler;

    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _sprintSpeed = 4f;

    [SerializeField] private bool _isCartModeEnabled = false;



    [ClientRpc]
    private void PutDownClientRpc(ClientRpcParams rpcParams = default)
    {
        Debug.Log("PutDownClientRpc received From Player Controller");

        // unactive cart mode immediately
        _isCartModeEnabled = false;
        EnableCartMode(false, null);

        // Unactive all component of cart mode
        var playerCamera = _playerTransform.GetComponentInChildren<S_PlayerCamera>();
        if (playerCamera != null)
        {
            playerCamera.EnableCartMode(false, null);
        }

        var playerObject = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (playerObject != null)
        {
            var playerController = playerObject.GetComponentInChildren<S_PlayerController>();
            if (playerController != null)
            {
                playerController.EnableCartMode(false, null);
            }
        }
    }

    [ClientRpc]
    public void EnableCartModeClientRpc(bool isEnabled, NetworkObjectReference cartRef)
    {
        if (!IsOwner) return;

        Transform cartTransform = null;

        if (isEnabled && cartRef.TryGet(out NetworkObject cartObj))
        {
            cartTransform = cartObj.transform;
        }

        EnableCartMode(isEnabled, cartTransform); // Local activation

        if (!isEnabled)
        {
            // Unactivate localy the cart mode
            PutDownClientRpc();
            _isCartModeEnabled = false;
            _playerTransform.GetComponentInChildren<S_PlayerCamera>()?.EnableCartMode(false, null);
        }
    }

    public void EnableCartMode(bool enabled, Transform cart = null)
    {
        _isCartModeEnabled = enabled;

        Debug.Log($"[CART MODE] Set to {(enabled ? "ENABLED" : "DISABLED")} for {gameObject.name}");

        _playerTransform.GetComponentInChildren<S_PlayerCamera>()?.EnableCartMode(enabled, cart);
    }


    private Transform _playerTransform;
    private S_PlayerCamera _playerCamera;
    private Vector3 _playerDirection;

    private float _currentSpeed = 4f;

    private bool _isSprinting = false;
    private float _speedMult = 1f;

    public bool activeInputs = true;

    void Start()
    {
        if (!_isSoloTestModeEnabled)
            return;

        _playerTransform = transform.parent.transform;
    }

    public override void OnNetworkSpawn()
    {
        if (_isSoloTestModeEnabled)
            return;

        _playerTransform = transform.parent.transform;
        _playerCamera = _playerTransform.GetComponentInChildren<S_PlayerCamera>();
        _playerCamera.SetPlayerTransform(_playerTransform);

        if (_playerCamera == null)
        {
            Debug.Log("Player camera wasn't set Please Check :: " + _playerTransform.name);
        }

        if (_playerTransform.parent.GetComponent<NetworkObject>().IsOwner)
        {
            HandleInputsEvents();

            S_LifeManager.OnDie += Respawn;
            S_Extract.OnExtract += DisableAllMeshOfPlayer;
            S_Extract.OnExtract += DropInputsEvents;

            S_PlayersSpawner.Instance.SpawnPlayer(_playerTransform.transform.parent.gameObject, _playerTransform);
        }
        else
        {
            // Client Side
            GameObject camerObject = _playerTransform.GetComponentInChildren<Camera>().gameObject;
            camerObject.GetComponent<Camera>().enabled = false;
            camerObject.GetComponent<S_PlayerCamera>().enabled = false;
            camerObject.GetComponent<AudioListener>().enabled = false;
            camerObject.GetComponent<UniversalAdditionalCameraData>().enabled = false;
            _playerTransform.GetComponentInChildren<PlayerInput>().gameObject.SetActive(false);
            _playerTransform.GetComponentInChildren<S_PlayerInteract>().gameObject.SetActive(false);
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
            Debug.LogError(
                $"ERROR ! The '{nameof(_playerTransform)}' variable is null, " +
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
        if (_isCartModeEnabled)
        {
            // Simulate cart movement for always go forward
            Vector3 forward = _playerTransform.forward * _playerDirection.z; // z = forward/rear
            _playerTransform.position += forward * (Time.deltaTime * _currentSpeed);

            // Rotation for horizontal inputs
            if (Mathf.Abs(_playerDirection.x) > 0.1f)
            {
                float rotationAmount = _playerDirection.x * 100f * Time.deltaTime; 
                _playerTransform.Rotate(0, rotationAmount, 0);
            }
        }
        else
        {
            if (_playerCamera == null)
            {
                Debug.LogWarning("Trying to move but _playerCamera is null. Skipping movement.");
                return;
            }

            Vector3 moveDir = _playerCamera.GetMovementDirection(new Vector2(_playerDirection.x, _playerDirection.z));
            _playerTransform.position += moveDir * (Time.deltaTime * _currentSpeed);
        }
    }

    private void Sprint(bool sprint)
    {
        _currentSpeed = (sprint ? _sprintSpeed : _walkSpeed) * _speedMult;
        _isSprinting = sprint;
        _armsAnimator.speed = sprint ? 2 : 1;
    }

    private void GetDirection(Vector3 p_playerDirection)
    {
        _playerDirection.x = p_playerDirection.x;
        _playerDirection.z = p_playerDirection.y;

        _armsAnimator.SetBool("Walking", _playerDirection.sqrMagnitude > 0.01f);
    }

    public void OnObjectPickedUp(S_Pickable p_pickable)
    {
        // TODO change 20f to the actual player strength
        _speedMult = p_pickable == null ? 1f : 1f - Mathf.Clamp(p_pickable.weight / 20f, 0f, 1f);
        Sprint(_isSprinting);
    }

    private void Respawn(S_PlayerAttributes attributes)
    {
        DropInputsEvents();
        DisableAllMeshOfPlayer();
        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(5);
        _playerTransform.position = respawnPoint.position;
        EnableAllMeshOfPlayer();
        HandleInputsEvents();
    }

    private void HandleInputsEvents()
    {
        S_PlayerInputsReciever.OnJump += Jump;
        S_PlayerInputsReciever.OnMove += GetDirection;
        S_PlayerInputsReciever.OnSprint += Sprint;

        activeInputs = true;
    }

    private void DropInputsEvents(E_PlayerTeam team = E_PlayerTeam.NONE)
    {
        S_PlayerInputsReciever.OnJump -= Jump;
        S_PlayerInputsReciever.OnMove -= GetDirection;
        S_PlayerInputsReciever.OnSprint -= Sprint;

        activeInputs = false;

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