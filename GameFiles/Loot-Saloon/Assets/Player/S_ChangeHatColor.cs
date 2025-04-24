using System;
using Unity.Netcode;
using UnityEngine;

public class S_ChangeHatColor : NetworkBehaviour
{
    [SerializeField] private Material _blueMaterial;
    [SerializeField] private Material _redMaterial;
    [SerializeField] private SkinnedMeshRenderer _hat;

    private void OnEnable()
    {
        S_PlayersSpawner.OnPlayerSpawned += NotifyHatColorChange;
    }

    private void OnDisable()
    {
        S_PlayersSpawner.OnPlayerSpawned -= NotifyHatColorChange;
    }

    private void NotifyHatColorChange()
    {
        if (IsOwner)
        {
            var playerTeam = GetComponentInChildren<S_PlayerAttributes>().Team;
            UpdateHatMaterialServerRpc(playerTeam);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateHatMaterialServerRpc(E_PlayerTeam p_team)
    {
        // Broadcast the change to all clients
        UpdateHatMaterialClientRpc(NetworkObjectId, p_team);
    }

    [ClientRpc]
    private void UpdateHatMaterialClientRpc(ulong playerId, E_PlayerTeam p_team)
    {
        // Ensure the correct player's hat is updated
        if (NetworkObjectId == playerId)
        {
            _hat.material = p_team == E_PlayerTeam.BLUE ? _blueMaterial : _redMaterial;
        }
    }
}