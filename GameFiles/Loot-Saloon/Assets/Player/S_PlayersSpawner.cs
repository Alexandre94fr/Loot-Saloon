#region
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
#endregion

public class S_PlayersSpawner : MonoBehaviour
{
    [SerializeField] private Transform _redTeam;
    [SerializeField] private Transform _blueTeam;

    [SerializeField] private float _spawnDistance = 5f;
    [SerializeField] private float _SpawnRadius = 10f;
    [SerializeField] private E_PlayerTeam _playerTeam;
    public static S_PlayersSpawner Instance { get; private set; }

    private int _bluePlayer = 0;
    private int _redPlayer = 0;

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
        _playerTeam = await gameLobbyManager.GetPlayerTeam();
        int count = NetworkManager.Singleton.ConnectedClients.Count;
        float totalWidth = (count - 1) * _spawnDistance;
        float startX = p_origin.position.x - totalWidth / 2f;
        int nbPlayer;
        float fixedZ = 0;
        if (_playerTeam == E_PlayerTeam.BLUE)
        {
            _bluePlayer++;
            nbPlayer = _bluePlayer;
            fixedZ = _blueTeam.position.z;
        }
        else
        {
            _redPlayer++;
            nbPlayer = _redPlayer;
            fixedZ = _redTeam.position.z;
        }

        NetworkObject networkObject = p_player.TryGetComponent(out NetworkObject netObj) ? netObj : null;

        Vector3 pos = new Vector3(startX + nbPlayer * _spawnDistance, p_origin.position.y, fixedZ);
        NetworkTransform playerNetworkTransform = p_player.GetComponentInChildren<NetworkTransform>();
        playerNetworkTransform.Teleport(pos, Quaternion.Euler(Vector3.right), transform.localScale);

        Debug.Log($"Spawn player {p_player.name} at {playerNetworkTransform.transform.position}");
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