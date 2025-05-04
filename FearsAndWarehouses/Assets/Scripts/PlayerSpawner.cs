using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Transform[] spawnPoints; 
    public string playerTag = "Player"; 

    void Start()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
     
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            
            if (player != null)
            {
              
                int randomIndex = Random.Range(0, spawnPoints.Length);
                Transform spawnPoint = spawnPoints[randomIndex];

               
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;
                Debug.Log($"Игрок перемещен в точку: {spawnPoint.position} с поворотом: {spawnPoint.rotation}");
            }
            else
            {
                Debug.LogError("Игрок не найден на сцене! Убедитесь, что объект игрока имеет тег 'Player'");
            }
        }
        else
        {
            Debug.LogError("Точки спавна не назначены!");
        }
    }
} 