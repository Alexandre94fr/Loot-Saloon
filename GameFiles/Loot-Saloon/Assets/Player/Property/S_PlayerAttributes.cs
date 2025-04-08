// using System;
// using Unity.Netcode;
// using UnityEngine;
//
// public class S_PlayerAttributes : NetworkBehaviour
// {
//     #region -= Events =-
//     
//     public static Action<S_Player, int> _OnPlayerMaxHPChangeEvent;
//     public static Action<S_Player, int> _OnPlayerHPChangeEvent;
//
//     public static Action<S_Player, int> _OnPlayerMaxXPChangeEvent;
//     public static Action<S_Player, int> _OnPlayerXPChangeEvent;
//
//     public static Action<S_Player, int> _OnPlayerMaxLevelChangeEvent;
//     public static Action<S_Player, int> _OnPlayerLevelChangeEvent;
//
//     public static Action<S_Player, int> _OnPlayerScoreChangeEvent;
//     #endregion
//
//     #region -= Getters =-
//
//     public int MaxHP => _maxHPNetworkVariable.Value;
//     public int CurrentHP => _currentHPNetworkVariable.Value;
//
//     public int MaxXP => _maxXPNetworkVariable.Value;
//     public int CurrentXP => _currentXPNetworkVariable.Value;
//
//     public int MaxLevel => _maxLevelNetworkVariable.Value;
//     public int CurrentLevel => _currentLevelNetworkVariable.Value;
//
//     public int CurrentScore => _currentScoreNetworkVariable.Value;
//     #endregion
//
//     [Header(" Debugging")]
//     [SerializeField] bool _isDebugModeOn = true;
//
//     [Header(" Reference to the player's stats :")]
//     [SerializeField] S_Player _player;
//
//     // Attributes
//
//     [Header(" Health points :")]
//     [SerializeField] NetworkVariable<int> _maxHPNetworkVariable = new(100, writePerm: NetworkVariableWritePermission.Server);
//     [SerializeField] NetworkVariable<int> _currentHPNetworkVariable = new(100, writePerm: NetworkVariableWritePermission.Server);
//
//     [Header(" Experience points :")]
//     [SerializeField] NetworkVariable<int> _maxXPNetworkVariable = new(100, writePerm: NetworkVariableWritePermission.Server);
//     [SerializeField] NetworkVariable<int> _currentXPNetworkVariable = new(0, writePerm: NetworkVariableWritePermission.Server);
//
//     [Header(" Levels :")]
//     [SerializeField] NetworkVariable<int> _maxLevelNetworkVariable = new(10, writePerm: NetworkVariableWritePermission.Server);
//     [SerializeField] NetworkVariable<int> _currentLevelNetworkVariable = new(1, writePerm: NetworkVariableWritePermission.Server);
//
//     [Header(" Score :")]
//     [SerializeField] NetworkVariable<int> _currentScoreNetworkVariable = new(0, writePerm: NetworkVariableWritePermission.Server);
//
//     // NOTE : The ReadOnlyInInspector is there in case the variable are serialized
//
//     [ReadOnlyInInspector] int _maxHP = 100;
//     [ReadOnlyInInspector] int _currentHP = 100;
//
//     [ReadOnlyInInspector] int _maxXP = 100;
//     [ReadOnlyInInspector] int _currentXP = 0;
//
//     [ReadOnlyInInspector] int _maxLevel = 10;
//     [ReadOnlyInInspector] int _currentLevel = 1;
//
//     [ReadOnlyInInspector] int _currentScore = 0;
//
//     void Start()
//     {
//         if (!IsVariablesCorrectlySetted())
//             return;
//     }
//
//     bool IsVariablesCorrectlySetted()
//     {
//         bool areVariablesValid = true;
//
//         if (_player == null)
//         {
//             Debug.LogError($"ERROR ! The variable '{nameof(_player)}' in '{name}' GameObject is null, please set it through the Unity inspector.");
//             areVariablesValid = false;
//         }
//
//         // You can add more checks if necessary
//
//         return areVariablesValid;
//     }
//
//     #region -= Setter methods =-
//
//     #region - Health points -
//
//     // Max Health points
//
//     [Rpc(SendTo.Server)]
//     public void SetMaxHP_RPC(int p_newMaxHP)
//     {
//         if (!IsServer)
//             return;
//
//         _maxHP = p_newMaxHP;
//         _maxHPNetworkVariable.Value = p_newMaxHP;
//
//         UpdateMaxHP_RPC(_maxHP);
//     }
//
//     [Rpc(SendTo.Server)]
//     public void AddMapHP_RPC(int p_newMaxHP)
//     {
//         if (!IsServer)
//             return;
//
//         SetMaxHP_RPC(MaxHP + p_newMaxHP);
//     }
//
//     [Rpc(SendTo.ClientsAndHost)]
//     void UpdateMaxHP_RPC(int p_newMaxHP)
//     {
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateMaxHP_RPC UPDATING | '{nameof(p_newMaxHP)}' : {p_newMaxHP}");
//
//         _OnPlayerMaxHPChangeEvent?.Invoke(_player, p_newMaxHP);
//
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateMaxHP_RPC UPDATING | '{nameof(p_newMaxHP)}' : {p_newMaxHP}");
//     }
//
//     // Current Health points
//
//     /// <summary>
//     /// Ask for the server to set the current HP </summary>
//     [Rpc(SendTo.Server)]
//     public void SetCurrentHP_RPC(int p_newHP)
//     {
//         if (!IsServer)
//             return;
//
//         if (_isDebugModeOn)
//             Debug.Log($"SetCurrentHP_RPC UPDATING | '{nameof(p_newHP)}' : {p_newHP}");
//
//         _currentHP = p_newHP;
//
//         // Death handling
//         if (_currentHP <= 0)
//         {
//             _currentHP = 0;
//             
//             // Launching death event
//             NotifyPlayerDeath_RPC();
//
//             // Stop replicating the player (you can put 'false' in Despawn, it means, Despawn function will not destroy the object)
//             _player.GetComponent<NetworkObject>().Despawn();
//         }
//
//         // Full life handling
//         if (_currentHP > _maxHP)
//         {
//             _currentHP = _maxHP;
//         }
//
//         _currentHPNetworkVariable.Value = _currentHP;
//
//         UpdateCurrentHP_RPC(_currentHP);
//
//         if (_isDebugModeOn)
//             Debug.Log($"SetCurrentHP_RPC UPDATED | '{nameof(p_newHP)}' : {p_newHP}");
//     }
//
//     /// <summary>
//     /// Ask the server to add the Health points given. You can subtract by doing -YOUR_NUMBER </summary>
//     [Rpc(SendTo.Server)]
//     public void AddCurrentHP_RPC(int p_newHP)
//     {
//         if (_isDebugModeOn)
//             Debug.Log($"AddCurrentHP_RPC UPDATING | '{nameof(p_newHP)}' : {p_newHP}");
//
//         // NOTE : The values are updated automaticly for all clients in SetCurrentHP_RPC, so no need to re-update.
//         SetCurrentHP_RPC(CurrentHP + p_newHP);
//
//         if (_isDebugModeOn)
//             Debug.Log($"AddCurrentHP_RPC UPDATED | '{nameof(p_newHP)}' : {p_newHP}");
//     }
//
//     [Rpc(SendTo.ClientsAndHost)]
//     void UpdateCurrentHP_RPC(int p_newHP)
//     {
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateCurrentHP_RPC UPDATING | '{nameof(p_newHP)}' : {p_newHP}");
//
//         _OnPlayerHPChangeEvent?.Invoke(_player, p_newHP);
//
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateCurrentHP_RPC UPDATED | '{nameof(p_newHP)}' : {p_newHP}");
//     }
//
//     [Rpc(SendTo.ClientsAndHost)]
//     void NotifyPlayerDeath_RPC()
//     {
//         S_Player._OnPlayerDeathEvent?.Invoke(_player, CurrentHP);
//     }
//     #endregion
//
//     #region - Experience points -
//
//     // Max Experience points
//
//     [Rpc(SendTo.Server)]
//     public void SetMaxXP_RPC(int p_newMaxXP)
//     {
//         if (!IsServer)
//             return;
//
//         _maxXP = p_newMaxXP;
//         _maxXPNetworkVariable.Value = p_newMaxXP;
//
//         UpdateMaxXP_Rpc(_maxXP);
//     }
//
//     [Rpc(SendTo.Server)]
//     public void AddMaxXP_RPC(int p_newMaxXP)
//     {
//         if (!IsServer)
//             return;
//
//         SetMaxXP_RPC(MaxXP + p_newMaxXP);
//     }
//
//     [Rpc(SendTo.ClientsAndHost)]
//     void UpdateMaxXP_Rpc(int p_newMaxXP)
//     {
//         _OnPlayerMaxHPChangeEvent?.Invoke(_player, p_newMaxXP);
//     }
//
//     // Current Experience points
//
//     /// <summary>
//     /// Ask for the server to set the current XP </summary>
//     [Rpc(SendTo.Server)]
//     public void SetCurrentXP_RPC(int p_newXP) 
//     {
//         if (!IsServer)
//             return;
//
//         if (_isDebugModeOn)
//             Debug.Log($"SetCurrentXP_RPC UPDATING | '{nameof(p_newXP)}' : {p_newXP}");
//
//         AddCurrentScore_RPC(p_newXP - _currentXP);
//
//         _currentXP = p_newXP;
//
//         if (_currentXP < 0)
//         {
//             Debug.LogError($"ERROR ! Someone tryied to set the variable '{nameof(_currentXP)}' of '{name}' GameObject to a value under 0. \nThe value has been set to 0.");
//             _currentXP = 0;
//         }
//
//         // Check if the player can gain one or more levels
//         if (_currentXP >= _maxXP && _currentLevel < _maxLevel)
//         {
//             // Compute the remaining XP that will remain after the level up
//             int remainingXPAfterLevelUp = _currentXP % _maxXP;
//
//             // Level up the player
//             AddCurrentLevel_RPC(_currentXP / _maxXP);
//
//             // Set the remaining XP after the level up
//             _currentXP = remainingXPAfterLevelUp;
//         }
//
//         // If the max level is already achieved, and if the currentXP is superiour to the max
//         if (_currentXP >= _maxXP)
//         {
//             _currentXP = _maxXP;
//         }
//
//         _currentXPNetworkVariable.Value = _currentXP;
//
//         UpdateCurrentXP_RPC(_currentXP);
//
//         if (_isDebugModeOn)
//             Debug.Log($"SetCurrentXP_RPC UPDATED | '{nameof(p_newXP)}' : {p_newXP}");
//     }
//
//     /// <summary>
//     /// Ask the server to add the Experience points given. You can subtract by doing -YOUR_NUMBER </summary>
//     [Rpc(SendTo.Server)]
//     public void AddCurrentXP_RPC(int p_newXP)
//     {
//         if (!IsServer)
//             return;
//
//         if (_isDebugModeOn)
//             Debug.Log($"AddCurrentXP_RPC UPDATING | '{nameof(p_newXP)}' : {p_newXP}");
//
//         // NOTE : The values are updated automaticly for all clients in SetCurrentXP_RPC, so no need to re-update.
//         SetCurrentXP_RPC(CurrentXP + p_newXP);
//
//         if (_isDebugModeOn)
//             Debug.Log($"AddCurrentXP_RPC UPDATED | '{nameof(p_newXP)}' : {p_newXP}");
//     }
//
//     [Rpc(SendTo.ClientsAndHost)]
//     void UpdateCurrentXP_RPC(int p_newXP)
//     {
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateCurrentXP_RPC UPDATING | '{nameof(p_newXP)}' : {p_newXP}");
//
//         _OnPlayerXPChangeEvent?.Invoke(_player, p_newXP);
//
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateCurrentXP_RPC UPDATED | '{nameof(p_newXP)}' : {p_newXP}");
//     }
//     #endregion
//
//     #region - Levels -
//
//     // Max Level
//
//     [Rpc(SendTo.Server)]
//     public void SetMaxLevel_RPC(int p_newMaxLevel)
//     {
//         if (!IsServer)
//             return;
//
//         _maxLevel = p_newMaxLevel;
//         _maxLevelNetworkVariable.Value = p_newMaxLevel;
//
//         UpdateMaxLevel_RPC(_maxLevel);
//     }
//
//     [Rpc(SendTo.ClientsAndHost)]
//     void UpdateMaxLevel_RPC(int p_newMaxLevel)
//     {
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateMaxLevel_RPC UPDATING | '{nameof(p_newMaxLevel)}' : {p_newMaxLevel}");
//
//         _OnPlayerMaxLevelChangeEvent?.Invoke(_player, p_newMaxLevel);
//
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateMaxLevel_RPC UPDATING | '{nameof(p_newMaxLevel)}' : {p_newMaxLevel}");
//     }
//
//     // Current Level
//
//     /// <summary>
//     /// Ask for the server to set the current Level </summary>
//     [Rpc(SendTo.Server)]
//     public void SetCurrentLevel_RPC(int p_newLevel)
//     {
//         if (!IsServer)
//             return;
//
//         if (_isDebugModeOn)
//             Debug.Log($"SetCurrentLevel_RPC UPDATING | '{nameof(p_newLevel)}' : {p_newLevel}");
//
//         _currentLevel = p_newLevel;
//
//         // Handling under 0 value
//         if (_currentLevel < 0)
//         {
//             Debug.LogError($"ERROR ! Someone tryied to set the variable '{nameof(_currentLevel)}' of '{name}' GameObject to a value under 0. \nThe value has been set to 0.");
//             _currentLevel = 0;
//         }
//
//         // Max level handling
//         if (_currentLevel > _maxLevel)
//         {
//             _currentLevel = _maxLevel;
//         }
//
//         _currentLevelNetworkVariable.Value = _currentLevel;
//
//         UpdateCurrentLevel_RPC(_currentLevel);
//
//         // Change max XP value
//         if (_currentLevel != 1)
//             SetMaxXP_RPC((int)(_player._Level1MaxXP * Math.Pow(_player._LevelUpMaxXPMultiplicationFactor, _currentLevel - 1)));
//
//         if (_isDebugModeOn)
//             Debug.Log($"SetCurrentLevel_RPC UPDATED | '{nameof(p_newLevel)}' : {p_newLevel}");
//     }
//
//     /// <summary>
//     /// Ask the server to add the Health points given. You can subtract by doing -YOUR_NUMBER </summary>
//     [Rpc(SendTo.Server)]
//     public void AddCurrentLevel_RPC(int p_newLevel)
//     {
//         if (!IsServer)
//             return;
//
//         if (_isDebugModeOn)
//             Debug.Log($"AddCurrentLevel_RPC UPDATING | '{nameof(p_newLevel)}' : {p_newLevel}");
//
//         // NOTE : The values are updated automaticly for all clients in SetCurrentLevel_RPC, so no need to re-update.
//         SetCurrentLevel_RPC(CurrentLevel + p_newLevel);
//
//         if (_isDebugModeOn)
//             Debug.Log($"AddCurrentLevel_RPC UPDATED | '{nameof(p_newLevel)}' : {p_newLevel}");
//     }
//
//     [Rpc(SendTo.ClientsAndHost)]
//     void UpdateCurrentLevel_RPC(int p_newLevel)
//     {
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateCurrentLevel_RPC UPDATING | '{nameof(p_newLevel)}' : {p_newLevel}");
//
//         _OnPlayerLevelChangeEvent?.Invoke(_player, p_newLevel);
//
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateCurrentLevel_RPC UPDATED | '{nameof(p_newLevel)}' : {p_newLevel}");
//     }
//     #endregion
//
//     #region - Score -
//
//     // Current Score
//
//     /// <summary>
//     /// Ask for the server to set the current Score </summary>
//     [Rpc(SendTo.Server)]
//     public void SetCurrentScore_RPC(int p_newScore)
//     {
//         if (!IsServer)
//             return;
//
//         if (_isDebugModeOn)
//             Debug.Log($"SetCurrentScore_RPC UPDATING | '{nameof(p_newScore)}' : {p_newScore}");
//
//         _currentScore = p_newScore;
//
//         // Handling under 0 value
//         if (_currentScore < 0)
//         {
//             Debug.LogError($"ERROR ! Someone tryied to set the variable '{nameof(_currentScore)}' of '{name}' GameObject to a value under 0. \nThe value has been set to 0.");
//             _currentScore = 0;
//         }
//         
//         _currentScoreNetworkVariable.Value = _currentScore;
//
//         UpdateCurrentScore_RPC(_currentScore);
//
//         if (_isDebugModeOn)
//             Debug.Log($"SetCurrentScore_RPC UPDATED | '{nameof(p_newScore)}' : {p_newScore}");
//     }
//
//     /// <summary>
//     /// Ask the server to add the Health points given. You can subtract by doing -YOUR_NUMBER </summary>
//     [Rpc(SendTo.Server)]
//     public void AddCurrentScore_RPC(int p_newScore)
//     {
//         if (!IsServer)
//             return;
//
//         if (_isDebugModeOn)
//             Debug.Log($"AddCurrentScore_RPC UPDATING | '{nameof(p_newScore)}' : {p_newScore}");
//
//         // NOTE : The values are updated automaticly for all clients in SetCurrentScore_RPC, so no need to re-update.
//         SetCurrentScore_RPC(CurrentScore + p_newScore);
//
//         if (_isDebugModeOn)
//             Debug.Log($"AddCurrentScore_RPC UPDATED | '{nameof(p_newScore)}' : {p_newScore}");
//     }
//
//     [Rpc(SendTo.ClientsAndHost)]
//     void UpdateCurrentScore_RPC(int p_newScore)
//     {
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateCurrentScore_RPC UPDATING | '{nameof(p_newScore)}' : {p_newScore}");
//
//         _OnPlayerScoreChangeEvent?.Invoke(_player, p_newScore);
//
//         if (_isDebugModeOn)
//             Debug.Log($"UpdateCurrentScore_RPC UPDATED | '{nameof(p_newScore)}' : {p_newScore}");
//     }
//     #endregion
//
//     #endregion
// }
