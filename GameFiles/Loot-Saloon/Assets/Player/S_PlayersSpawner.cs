#region
using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
#endregion

public class S_PlayersSpawner : NetworkBehaviour
{
    [SerializeField] private Transform _redTeam;
    [SerializeField] private Transform _blueTeam;

    [SerializeField] private float _spawnDistance = 5f;
    [SerializeField] private float _SpawnRadius = 10f;
    [SerializeField] private E_PlayerTeam _playerTeam;
    
    [SerializeField] private Material _redTeamMaterial;
    [SerializeField] private Material _blueTeamMaterial;
    public static S_PlayersSpawner Instance { get; private set; }

    private int _bluePlayer = 0;
    private int _redPlayer = 0;

    private Quaternion _redRotation = Quaternion.Euler(0, 0, 0);
    private Quaternion _blueRotation = Quaternion.Euler(0, 180, 0);
    private Quaternion _rotation;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void SpawnPlayer(GameObject p_player, Transform p_origin)
    {
        S_GameLobbyManager gameLobbyManager = S_GameLobbyManager.instance;

        playerTeam = await gameLobbyManager.GetPlayerTeamAsync();

        int count = NetworkManager.Singleton.ConnectedClients.Count;
        float totalWidth = (count - 1) * _spawnDistance;
        float startX = p_origin.position.x - totalWidth / 2f;
        int nbPlayer;
        float fixedZ = 0;

        S_PlayerCharacter playerCharacter = p_player.GetComponentInChildren<S_PlayerCharacter>();
        S_PlayerAttributes playerAttributes = playerCharacter.playerAttributes;
        S_PlayerController playerController = playerCharacter.playerController;
        playerAttributes.SetTeam_RPC(playerTeam);

        if (playerTeam == E_PlayerTeam.BLUE)
        {
            _bluePlayer++;
            nbPlayer = _bluePlayer;
            fixedZ = _blueTeam.position.z;
            playerController.respawnPoint = _blueTeam;
            _rotation = _blueRotation;
        }
        else
        {
            _redPlayer++;
            nbPlayer = _redPlayer;
            fixedZ = _redTeam.position.z;
            playerController.respawnPoint = _redTeam;
            _rotation = _redRotation;
        }

        Vector3 newPosition = new(startX + nbPlayer * _spawnDistance, p_origin.position.y, fixedZ);

        StartCoroutine(TPPlayer(p_player, newPosition));
    }

    IEnumerator TPPlayer(GameObject p_player, Vector3 p_newPosition)
    {
        if (p_player.GetComponentInChildren<NetworkTransform>() != null)
        {
            p_player.GetComponentInChildren<NetworkTransform>().Teleport(p_newPosition, _rotation, Vector3.one);
        }
        else
            Debug.LogWarning("WARNING ! NetworkTransform not founded on the player.");

        yield break;
    }

    public void RandomSpawnInRadius(GameObject p_player, Transform p_origin)
    {
        const int maxAttempts = 30;
        float radius = _SpawnRadius;
        float checkRadius = 0.5f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 candidate = new Vector3(
                p_origin.position.x + randomCircle.x,
                p_origin.position.y,
                p_origin.position.z + randomCircle.y
            );

            if (!Physics.CheckSphere(candidate, checkRadius))
            {
                p_player.transform.position = candidate;
                p_player.transform.rotation = Quaternion.LookRotation(Vector3.right);
                return;
            }
        }

        Debug.LogWarning($"[{nameof(S_PlayersSpawner)}] Can't find a safe zone after {maxAttempts} tries.");
    }
}