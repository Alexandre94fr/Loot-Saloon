using System;
using System.Collections;
using System.Xml.Linq;
using Unity.Netcode;
using UnityEngine;

public enum E_PlayerTeam
{
    NONE,
    BLUE,
    RED
}

public class S_PlayerAttributes : NetworkBehaviour
{
    #region -= Events =-

    public static Action<ulong, float> OnPlayerWalkingMovementSpeedChangeEvent;
    public static Action<ulong, float> OnPlayerRunningMovementSpeedChangeEvent;

    public static Action<ulong, float> OnPlayerJumpPowerChangeEvent;

    public static Action<ulong, int > OnPlayerDeathEvent;

    public static Action<ulong, int> OnPlayerMaxHealthPointChangeEvent;
    public static Action<ulong, int> OnPlayerHealthPointChangeEvent;

    public static Action<ulong, int> OnPlayerLiftingStrenghChangeEvent;

    public static Action<ulong, E_PlayerTeam> OnPlayerTeamChangeEvent;
    #endregion

    #region -= Getters =-

    public float WalkingMovementSpeed => _walkingMovementSpeedNetworkVariable.Value;
    public float RunningMovementSpeed => _runningMovementSpeedNetworkVariable.Value;

    public float JumpPower => _jumpPowerNetworkVariable.Value;

    public int MaxHealthPoint => _maxHealthPointsNetworkVariable.Value;
    public int CurrentHealthPoint => _currentHealthPointsNetworkVariable.Value;

    public int LiftingStrengh => _liftingStrenghNetworkVariable.Value;

    public E_PlayerTeam Team => _teamNetworkVariable.Value;
    #endregion

    [Header(" Debugging")]
    [SerializeField] private bool _isDebugModeOn = true;

    [Header(" Properties :")]
    [SerializeField] private S_PlayerProperties _playerProperties;

    #region -= Network variables =-

    [Header(" Network variables (please don't modify them from inspector) :")]
    [SerializeField] private NetworkVariable<float> _walkingMovementSpeedNetworkVariable = new(
        readPerm: NetworkVariableReadPermission.Everyone, 
        writePerm: NetworkVariableWritePermission.Server
    );
    [SerializeField] private NetworkVariable<float> _runningMovementSpeedNetworkVariable = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    [SerializeField] private NetworkVariable<float> _jumpPowerNetworkVariable = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    [SerializeField] private NetworkVariable<int> _maxHealthPointsNetworkVariable = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );
    [SerializeField] private NetworkVariable<int> _currentHealthPointsNetworkVariable = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    [SerializeField] private NetworkVariable<int> _liftingStrenghNetworkVariable = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    // Set by the S_PlayerSpawner script
    [SerializeField] private NetworkVariable<E_PlayerTeam> _teamNetworkVariable = new(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );
    #endregion

    #region -= Local attributes variables =-

    // NOTE : The ReadOnlyInInspector is there in case the variable are serialized,
    //        if there are [SerializeField] it's to be able to debug


    [Header(" Attributes (check network variable to see the updated version) :")]

    [SerializeField] [ReadOnlyInInspector] ulong _playerID;

    [Space]
    [ReadOnlyInInspector] float _walkingMovementSpeed = 4;
    [ReadOnlyInInspector] float _runningMovementSpeed = 8;

    [ReadOnlyInInspector] float _jumpPower = 5;

    [ReadOnlyInInspector] int _maxHP = 100;
    [ReadOnlyInInspector] int _currentHP = 0;

    [ReadOnlyInInspector] int _liftingStrengh = 20;

    [ReadOnlyInInspector] E_PlayerTeam _team = E_PlayerTeam.NONE;
    #endregion


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _playerID = NetworkManager.Singleton.LocalClientId;
    }

    private void Start()
    {
        if (!S_VariablesChecker.AreVariablesCorrectlySetted(name, null,
            (_playerProperties, nameof(_playerProperties))
        )) return;

        OnPlayerDeathEvent += OnDeathInitalization;

        Initialize();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        OnPlayerDeathEvent -= OnDeathInitalization;
    }

    void Initialize()
    {
        Initialize_ServerRPC();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    void Initialize_ServerRPC()
    {
        if (!IsServer)
            return;

        SetWalkingMovementSpeed_RPC(_playerProperties.walkingMovementSpeed);
        SetRunningMovementSpeed_RPC(_playerProperties.runningMovementSpeed);

        SetJumpPower_RPC(_playerProperties.jumpPower);

        SetMaxHealthPoint_RPC(_playerProperties.maxHealthPoints);
        SetCurrentHealthPoint_RPC(MaxHealthPoint);

        SetLiftingStrengh_RPC(_playerProperties.liftingStrengh);
    }

    void OnDeathInitalization(ulong p_playerID, int p_currentPlayerHealthPoints)
    {
        Initialize_ServerRPC();

        // NOTE : For now it's a perfect copy of Initialize but, that may change in the futur
    }

    #region -= Setter methods =-

    #region - Movement speed -

    #region Walking movement speed

    [Rpc(SendTo.Server)]
    public void SetWalkingMovementSpeed_RPC(float p_newWalkingMovementSpeed)
    {
        if (!IsServer)
            return;

        _walkingMovementSpeed = p_newWalkingMovementSpeed;
        _walkingMovementSpeedNetworkVariable.Value = p_newWalkingMovementSpeed;

        UpdateWalkingMovementSpeed_RPC(_walkingMovementSpeed);
    }

    [Rpc(SendTo.Server)]
    public void AddWalkingMovementSpeed_RPC(float p_newWalkingMovementSpeed)
    {
        if (!IsServer)
            return;

        SetWalkingMovementSpeed_RPC(WalkingMovementSpeed + p_newWalkingMovementSpeed);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateWalkingMovementSpeed_RPC(float p_newWalkingMovementSpeed)
    {
        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateWalkingMovementSpeed_RPC)} UPDATING | '{nameof(p_newWalkingMovementSpeed)}' : {p_newWalkingMovementSpeed}");

        _walkingMovementSpeed = p_newWalkingMovementSpeed;

        OnPlayerWalkingMovementSpeedChangeEvent?.Invoke(_playerID, p_newWalkingMovementSpeed);
        Debug.LogWarning("Change Event was Called for :: " + p_newWalkingMovementSpeed);
        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateWalkingMovementSpeed_RPC)} UPDATED | '{nameof(p_newWalkingMovementSpeed)}' : {p_newWalkingMovementSpeed}");
    }
    #endregion

    #region Running movement speed

    [Rpc(SendTo.Server)]
    public void SetRunningMovementSpeed_RPC(float p_newRunningMovementSpeed)
    {
        if (!IsServer)
            return;

        _runningMovementSpeed = p_newRunningMovementSpeed;
        _runningMovementSpeedNetworkVariable.Value = p_newRunningMovementSpeed;

        UpdateRunningMovementSpeed_RPC(_runningMovementSpeed);
    }

    [Rpc(SendTo.Server)]
    public void AddRunningMovementSpeed_RPC(float p_newRunningMovementSpeed)
    {
        if (!IsServer)
            return;

        SetRunningMovementSpeed_RPC(RunningMovementSpeed + p_newRunningMovementSpeed);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateRunningMovementSpeed_RPC(float p_newRunningMovementSpeed)
    {
        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateRunningMovementSpeed_RPC)} UPDATING | '{nameof(p_newRunningMovementSpeed)}' : {p_newRunningMovementSpeed}");

        _runningMovementSpeed = p_newRunningMovementSpeed;

        Debug.LogWarning($"[Server] Call Coroutine Event {OwnerClientId}");
        StartCoroutine(DelayedEvent());

        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateRunningMovementSpeed_RPC)} UPDATED | '{nameof(p_newRunningMovementSpeed)}' : {p_newRunningMovementSpeed}");
    }

    private IEnumerator DelayedEvent()
    {
        yield return new WaitUntil(() => _runningMovementSpeedNetworkVariable.Value != _runningMovementSpeed);

        OnPlayerRunningMovementSpeedChangeEvent?.Invoke(_playerID, RunningMovementSpeed);
    }

    #endregion

    #endregion

    #region - Jump power -

    [Rpc(SendTo.Server)]
    public void SetJumpPower_RPC(float p_newJumpPower)
    {
        if (!IsServer)
            return;
        _jumpPower = p_newJumpPower;
        _jumpPowerNetworkVariable.Value = p_newJumpPower;
        UpdateJumpPower_RPC(_jumpPower);
    }
    [Rpc(SendTo.Server)]
    public void AddJumpPower_RPC(float p_newJumpPower)
    {
        if (!IsServer)
            return;
        SetJumpPower_RPC(JumpPower + p_newJumpPower);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateJumpPower_RPC(float p_newJumpPower)
    {
        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateJumpPower_RPC)} UPDATING | '{nameof(p_newJumpPower)}' : {p_newJumpPower}");

        _jumpPower = p_newJumpPower;

        OnPlayerJumpPowerChangeEvent?.Invoke(_playerID, p_newJumpPower);

        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateJumpPower_RPC)} UPDATED | '{nameof(p_newJumpPower)}' : {p_newJumpPower}");
    }
    #endregion

    #region - Health points -

    #region Max Health points

    [Rpc(SendTo.Server)]
    public void SetMaxHealthPoint_RPC(int p_newMaxHP)
    {
        if (!IsServer)
            return;

        _maxHP = p_newMaxHP;
        _maxHealthPointsNetworkVariable.Value = p_newMaxHP;

        UpdateMaxHealthPoint_RPC(_maxHP);
    }

    [Rpc(SendTo.Server)]
    public void AddMapHealthPoint_RPC(int p_newMaxHP)
    {
        if (!IsServer)
            return;

        SetMaxHealthPoint_RPC(MaxHealthPoint + p_newMaxHP);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateMaxHealthPoint_RPC(int p_newMaxHP)
    {
        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateMaxHealthPoint_RPC)} UPDATING | '{nameof(p_newMaxHP)}' : {p_newMaxHP}");

        _maxHP = p_newMaxHP;

        OnPlayerMaxHealthPointChangeEvent?.Invoke(_playerID, p_newMaxHP);

        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateMaxHealthPoint_RPC)} UPDATED | '{nameof(p_newMaxHP)}' : {p_newMaxHP}");
    }
    #endregion

    #region Current Health points

    /// <summary>
    /// Ask for the server to set the current HealthPoint </summary>
    [Rpc(SendTo.Server)]
    public void SetCurrentHealthPoint_RPC(int p_newHP)
    {
        if (!IsServer)
            return;

        if (_isDebugModeOn)
            Debug.Log($"{nameof(SetCurrentHealthPoint_RPC)} UPDATING | '{nameof(p_newHP)}' : {p_newHP}");

        _currentHP = p_newHP;

        // Death handling
        if (_currentHP <= 0)
        {
            _currentHP = 0;

            // Launching death event
            NotifyPlayerDeath_RPC();

            // If necessary you can un-comment the line below

            // Stop replicating the player (you can put 'false' in Despawn, it means, Despawn function will not destroy the object)
            //_playerCharacter.GetComponent<NetworkObject>().Despawn();
        }

        // Full life handling
        if (_currentHP > _maxHP)
        {
            _currentHP = _maxHP;
        }

        _currentHealthPointsNetworkVariable.Value = _currentHP;

        UpdateCurrentHealthPoint_RPC(_currentHP);

        if (_isDebugModeOn)
            Debug.Log($"{nameof(SetCurrentHealthPoint_RPC)} UPDATED | '{nameof(p_newHP)}' : {p_newHP}");
    }

    /// <summary>
    /// Ask the server to add the Health points given. You can subtract by doing -YOUR_NUMBER </summary>
    [Rpc(SendTo.Server)]
    public void AddCurrentHealthPoint_RPC(int p_newHP)
    {
        if (_isDebugModeOn)
            Debug.Log($"{nameof(AddCurrentHealthPoint_RPC)} UPDATING | '{nameof(p_newHP)}' : {p_newHP}");

        // NOTE : The values are updated automaticly for all clients in SetCurrentHealthPoint_RPC, so no need to re-update.
        SetCurrentHealthPoint_RPC(CurrentHealthPoint + p_newHP);

        if (_isDebugModeOn)
            Debug.Log($"{nameof(AddCurrentHealthPoint_RPC)} UPDATED | '{nameof(p_newHP)}' : {p_newHP}");
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateCurrentHealthPoint_RPC(int p_newHP)
    {
        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateCurrentHealthPoint_RPC)} UPDATING | '{nameof(p_newHP)}' : {p_newHP}");

        _currentHP = p_newHP;

        OnPlayerHealthPointChangeEvent?.Invoke(_playerID, p_newHP);

        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateCurrentHealthPoint_RPC)} UPDATED | '{nameof(p_newHP)}' : {p_newHP}");
    }
    #endregion

    [Rpc(SendTo.ClientsAndHost)]
    void NotifyPlayerDeath_RPC()
    {
        if (!IsOwner)
            return;

        OnPlayerDeathEvent?.Invoke(_playerID, CurrentHealthPoint);
    }
    #endregion

    #region - Lifting strengh -

    [Rpc(SendTo.Server)]
    public void SetLiftingStrengh_RPC(int p_newLiftingStrengh)
    {
        if (!IsServer)
            return;

        _liftingStrengh = p_newLiftingStrengh;
        _liftingStrenghNetworkVariable.Value = p_newLiftingStrengh;

        UpdateLiftingStrengh_RPC(_liftingStrengh);
    }

    [Rpc(SendTo.Server)]
    public void AddLiftingStrengh_RPC(int p_newLiftingStrengh)
    {
        if (!IsServer)
            return;

        SetLiftingStrengh_RPC(LiftingStrengh + p_newLiftingStrengh);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateLiftingStrengh_RPC(int p_newLiftingStrengh)
    {
        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateLiftingStrengh_RPC)} UPDATING | '{nameof(p_newLiftingStrengh)}' : {p_newLiftingStrengh}");

        _liftingStrengh = p_newLiftingStrengh;

        OnPlayerLiftingStrenghChangeEvent?.Invoke(_playerID, p_newLiftingStrengh);

        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateLiftingStrengh_RPC)} UPDATED | '{nameof(p_newLiftingStrengh)}' : {p_newLiftingStrengh}");
    }
    #endregion

    #region - Team -

    [Rpc(SendTo.Server)]
    public void SetTeam_RPC(E_PlayerTeam p_newTeam)
    {
        if (!IsServer)
            return;

        _team = p_newTeam;
        _teamNetworkVariable.Value = p_newTeam;

        UpdateTeam_RPC(_team);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateTeam_RPC(E_PlayerTeam p_newTeam)
    {
        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateTeam_RPC)} UPDATING | '{nameof(p_newTeam)}' : {p_newTeam}");

        _team = p_newTeam;

        OnPlayerTeamChangeEvent?.Invoke(_playerID, p_newTeam);

        if (_isDebugModeOn)
            Debug.Log($"{nameof(UpdateTeam_RPC)} UPDATED | '{nameof(p_newTeam)}' : {p_newTeam}");
    }
    #endregion

    #endregion
}