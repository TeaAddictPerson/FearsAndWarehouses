using UnityEngine;

public class RespawnSpawner : MonoBehaviour
{
    [Tooltip("Точки респавна для игрока")]
    public Transform[] respawnPoints;

    private int lastRespawnIndex = -1;


    public Transform GetNextRespawnPoint()
    {
        if (respawnPoints == null || respawnPoints.Length == 0)
        {
            Debug.LogError("RespawnSpawner: нет точек респавна!");
            return null;
        }

        lastRespawnIndex++;
        if (lastRespawnIndex >= respawnPoints.Length)
            lastRespawnIndex = 0;

        return respawnPoints[lastRespawnIndex];
    }

    // Можно оставить метод, если нужно
    public void RespawnPlayer(GameObject player)
    {
        Transform spawnPoint = GetNextRespawnPoint();
        if (spawnPoint == null) return;

        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"RespawnSpawner: игрок возрожден на точке {lastRespawnIndex}");
    }
}
