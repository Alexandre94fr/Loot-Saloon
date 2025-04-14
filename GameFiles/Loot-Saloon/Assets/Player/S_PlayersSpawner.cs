using System.Collections.Generic;
using UnityEngine;

public enum Team // It Exist somewere else Please change with the Adao's Enum /!\
{
    Red, Blue
}

public class S_PlayersSpawner : MonoBehaviour
{
    [SerializeField] private Transform _redTeam;
    [SerializeField] private Transform _blueTeam;
    public List<GameObject> p1;
    public List<GameObject> p2;

    [SerializeField] private float _spawnDistance = 5f;
    [SerializeField] private float _SpawnRadius = 10f;

    public void SpawnPlayer(List<GameObject> p_players, Transform p_origin)
    {
        int count = p_players.Count;

        float totalWidth = (count - 1) * _spawnDistance;
        float startX = p_origin.position.x - totalWidth / 2f;
        float fixedZ = p_origin.position.z;

        for (int i = 0; i < count; i++)
        {
            GameObject player = p_players[i];

            Vector3 pos = new Vector3(startX + i * _spawnDistance, p_origin.position.y, fixedZ);
            player.transform.position = pos;

            player.transform.rotation = Quaternion.LookRotation(Vector3.right);
        }
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
