#region
using Unity.Netcode;
using UnityEngine;
#endregion

public enum Team // It Exist somewere else Please change with the Adao's Enum /!\
{
    Red,
    Blue
}

public class S_PlayersSpawner : MonoBehaviour
{
    [SerializeField] private Transform _redTeam;
    [SerializeField] private Transform _blueTeam;

    [SerializeField] private float _spawnDistance = 5f;
    [SerializeField] private float _SpawnRadius = 10f;

    public static S_PlayersSpawner Instance { get; private set; }

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

    public void SpawnPlayer(GameObject p_player, Transform p_origin)
    {
        Debug.Log($"[{nameof(S_PlayersSpawner)}] Spawn player {p_player.name} at {p_origin.position}");
        int count = NetworkManager.Singleton.ConnectedClients.Count;

        float totalWidth = (count - 1) * _spawnDistance;
        float startX = p_origin.position.x - totalWidth / 2f;
        float fixedZ = p_origin.position.z;
        NetworkObject networkObject = p_player.TryGetComponent(out NetworkObject netObj) ? netObj : null;
        if (networkObject == null)
        {
            Debug.LogError($"[{nameof(S_PlayersSpawner)}] NetworkObject not found on {p_player.name}");
            return;
        }

        Vector3 pos = new Vector3(startX + p_player.GetComponent<NetworkObject>().NetworkObjectId * _spawnDistance, p_origin.position.y, fixedZ);
        p_player.transform.position = pos;

        p_player.transform.rotation = Quaternion.LookRotation(Vector3.right);
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