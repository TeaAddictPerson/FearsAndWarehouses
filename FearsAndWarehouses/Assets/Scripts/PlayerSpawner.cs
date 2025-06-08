using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public string playerTag = "Player";

    public bool phantomCompleted = false;
    public bool soulCompleted = false;
    public bool polterCompleted = false;

    void Start()
    {
        SpawnPlayerAccordingToProgress();
    }

    public void SpawnPlayerAccordingToProgress()
    {
        if (phantomCompleted && soulCompleted && polterCompleted)
        {
            Debug.Log("Все уровни пройдены. Переход в главное меню.");
            SceneManager.LoadScene(0);
            return;
        }

        List<string> remainingTags = new List<string>();
        if (!phantomCompleted) remainingTags.Add("Phantom");
        if (!soulCompleted) remainingTags.Add("LostSoul");
        if (!polterCompleted) remainingTags.Add("Poltergeist");

        List<Transform> validSpawnPoints = new List<Transform>();
        foreach (Transform point in spawnPoints)
        {
            if (remainingTags.Contains(point.tag))
            {
                validSpawnPoints.Add(point);
            }
        }

        if (validSpawnPoints.Count == 0)
        {
            Debug.LogError("Нет подходящих точек спавна!");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            int randomIndex = Random.Range(0, validSpawnPoints.Count);
            Transform spawnPoint = validSpawnPoints[randomIndex];

            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
            Debug.Log($"Игрок перемещен в точку: {spawnPoint.position} с тегом {spawnPoint.tag}");
        }
        else
        {
            Debug.LogError("Игрок не найден на сцене! Убедитесь, что объект игрока имеет тег 'Player'");
        }
    }
}
