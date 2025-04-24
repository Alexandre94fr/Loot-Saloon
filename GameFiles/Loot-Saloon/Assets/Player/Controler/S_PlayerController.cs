#region
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
#endregion

public class S_PlayerController : NetworkBehaviour
{
    public Vector3 boxExtents = new(0.4f, 0.01f, 0.4f);
    public LayerMask groundLayer;
    [HideInInspector] public Transform respawnPoint;

    [Header(" Debugging :")] 
    [Tooltip("Allow the devs to test there scenes without having to pass throw the Lobby")]
    [SerializeField] private bool _isSoloTestModeEnabled = true;

    [Space]
    [SerializeField] private Animator _armsAnimator;
    [SerializeField] private GameObject _armsHandler;


    private S_PlayerAttributes _attributes;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _cartSpeedMultiplicator = 0.5f;


    [SerializeField] private bool _isCartModeEnabled = false;

    public void SetPlayerAttribute(ref S_PlayerAttributes p_playerAttributes)
    {
        _attributes = p_playerAttributes;
    }

    [ClientRpc]
    private void PutDownClientRpc(ClientRpcParams rpcParams = default)
    {
        Debug.Log("PutDownClientRpc received From Player Controller");

        // unactive cart mode immediately
        _isCartModeEnabled = false;
        DisableCartMode();

        // Unactive all component of cart mode
        var playerCamera = _playerTransform.GetComponentInChildren<S_PlayerCamera>();
        if (playerCamera != null)
        {
            playerCamera.DisableCartMode();
        }

        var playerObject = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (playerObject != null)
        {
            var playerController = playerObject.GetComponentInChildren<S_PlayerController>();
            if (playerController != null)
            {
                playerController.DisableCartMode();
            }
        }
    }

    public void EnableCartMode(Transform cart = null)
    {
        if (_isCartModeEnabled)
            return;

        _isCartModeEnabled = true;

        Debug.Log($"[CART MODE] Set to ENABLED for {gameObject.name}");

        _playerTransform.GetComponentInChildren<S_PlayerCamera>()?.EnableCartMode(cart);

        Debug.LogWarning("Cart Mode is Enabled");

        _speedMult = _cartSpeedMultiplicator;
        UpdateSpeed();
    }

    [ClientRpc]
    public void EnableCartModeClientRpc(NetworkObjectReference cartRef)
    {
        if (!IsOwner) return;

        Transform cartTransform = null;

        if (cartRef.TryGet(out NetworkObject cartObj))
        {
            cartTransform = cartObj.transform;
        }

        EnableCartMode(cartTransform);
    }

    public void DisableCartMode()
    {
        if (!_isCartModeEnabled)
            return;

        _isCartModeEnabled = false;

        Debug.Log($"[CART MODE] Set to DISABLED for {gameObject.name}");

        _playerTransform.GetComponentInChildren<S_PlayerCamera>()?.DisableCartMode();

        Debug.LogWarning("Cart Mode is Disable");
        _speedMult = 1.0f;
        UpdateSpeed();
    }

    [ClientRpc]
    public void DisableCartModeClientRpc(NetworkObjectReference p_cartReference)
    {
        if (!IsOwner) 
            return;

        DisableCartMode();

        // Unactivate localy the cart mode
        PutDownClientRpc();
        _isCartModeEnabled = false;
        _playerTransform.GetComponentInChildren<S_PlayerCamera>()?.DisableCartMode();
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

    private void SetSprintInEvent(ulong p_playerID, float p_speed)
    {
        if (NetworkManager.Singleton.LocalClientId != p_playerID)
            return;

        Sprint(_isSprinting);
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

            S_PlayerAttributes.OnPlayerDeathEvent += Respawn;
            S_Extract.OnExtract += DisableAllMeshOfPlayer;
            S_Extract.OnExtract += DropInputsEvents;
            S_PlayerAttributes.OnPlayerWalkingMovementSpeedChangeEvent += SetSprintInEvent;
            S_PlayerAttributes.OnPlayerRunningMovementSpeedChangeEvent += SetSprintInEvent;


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
        Vector3 boxCenter = _playerTransform.position + Vector3.down * _playerTransform.localScale.y;

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
        Debug.Log("The Actual Speed is ::::: " + _currentSpeed);
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

    private void UpdateSpeed()
    {
        _currentSpeed = (_isSprinting ? _attributes.RunningMovementSpeed : _attributes.WalkingMovementSpeed) * _speedMult;
    }
    private void Sprint(bool sprint)
    {
        UpdateSpeed();
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

    private void Respawn(ulong p_playerID, int p_currentPlayerHealthPoints)
    {
        if (NetworkManager.Singleton.LocalClientId != p_playerID)
            return;

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