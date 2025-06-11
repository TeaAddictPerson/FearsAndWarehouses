using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using Mono.Data.Sqlite;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    public Transform[] spawnPoints;
    public string playerTag = "Player";

    private const string UserNameKey = "LoggedInUserName";

    // --- НОВАЯ АРХИТЕКТУРА ---
    private IDbConnection dbConnection; // Единое, постоянное соединение
    private string dbPath;

    void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "FearsAndWarehouses.db");

        // Открываем соединение ОДИН РАЗ при старте
        try
        {
            string connectionString = $"URI=file:{dbPath};Version=3;";
            dbConnection = new SqliteConnection(connectionString);
            dbConnection.Open();
            Debug.Log("Соединение с БД успешно открыто и будет поддерживаться.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"КРИТИЧЕСКАЯ ОШИБКА: Не удалось открыть соединение с БД при старте! {ex.Message}");
            this.enabled = false; // Отключаем скрипт, если нет БД
            return;
        }

        SpawnPlayerAccordingToProgress();
    }

    // Закрываем соединение ОДИН РАЗ при уничтожении объекта
    void OnDestroy()
    {
        if (dbConnection != null && dbConnection.State == ConnectionState.Open)
        {
            dbConnection.Close();
            dbConnection.Dispose();
            dbConnection = null;
            Debug.Log("Соединение с БД корректно закрыто.");
        }
    }

    public void GhostDefeated(string ghostTag)
    {
        StartCoroutine(ProcessGhostDefeatRoutine(ghostTag));
    }

    private IEnumerator ProcessGhostDefeatRoutine(string ghostTag)
    {
        yield return new WaitForSeconds(10); // Геймплейная задержка

        string ghostColumnName = "";
        switch (ghostTag)
        {
            case "Phantom": ghostColumnName = "phantom"; break;
            case "LostSoul": ghostColumnName = "soul"; break;
            case "Poltergeist": ghostColumnName = "polter"; break;
            default: yield break;
        }

        // Выполняем запись, используя уже открытое соединение
        UpdateProgressInDB(ghostColumnName);

        // Выполняем чтение и перемещение, используя то же самое соединение
        SpawnPlayerAccordingToProgress();
    }

    public void SpawnPlayerAccordingToProgress()
    {
        (bool isPhantomDefeated, bool isSoulDefeated, bool isPolterDefeated) progress = LoadProgressFromDB();
        if (progress.isPhantomDefeated && progress.isSoulDefeated && progress.isPolterDefeated)
        {
            Trans();
            return;
        }

        List<string> availableSpawnTags = new List<string>();
        if (!progress.isPhantomDefeated) availableSpawnTags.Add("Phantom");
        if (!progress.isSoulDefeated) availableSpawnTags.Add("LostSoul");
        if (!progress.isPolterDefeated) availableSpawnTags.Add("Poltergeist");

        List<Transform> validSpawnPoints = new List<Transform>();
        foreach (Transform point in spawnPoints)
        {
            if (availableSpawnTags.Contains(point.tag))
            {
                validSpawnPoints.Add(point);
            }
        }

        if (validSpawnPoints.Count == 0) return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            int randomIndex = Random.Range(0, validSpawnPoints.Count);
            Transform spawnPoint = validSpawnPoints[randomIndex];
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
        }
    }

    private bool ConvertProgressValue(object dbValue)
    {
        if (dbValue == null || dbValue is System.DBNull) return false;
        return dbValue.ToString().Trim().Equals("1") || dbValue.ToString().Trim().Equals("true", System.StringComparison.OrdinalIgnoreCase);
    }

    private (bool, bool, bool) LoadProgressFromDB()
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Ошибка чтения: Соединение с БД не открыто!");
            return (false, false, false);
        }

        string currentUser = PlayerPrefs.GetString(UserNameKey);
        if (string.IsNullOrEmpty(currentUser)) return (false, false, false);

        (bool, bool, bool) progress = (false, false, false);
        try
        {
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = "SELECT phantom, soul, polter FROM users WHERE name = @name;";
                var nameParam = command.CreateParameter(); nameParam.ParameterName = "@name"; nameParam.Value = currentUser; command.Parameters.Add(nameParam);
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        progress.Item1 = ConvertProgressValue(reader["phantom"]);
                        progress.Item2 = ConvertProgressValue(reader["soul"]);
                        progress.Item3 = ConvertProgressValue(reader["polter"]);
                    }
                }
            }
        }
        catch (System.Exception ex) { Debug.LogError($"Ошибка при ЧТЕНИИ прогресса: {ex.Message}"); }
        return progress;
    }

    private void UpdateProgressInDB(string ghostColumnName)
    {
        if (dbConnection == null || dbConnection.State != ConnectionState.Open)
        {
            Debug.LogError("Ошибка записи: Соединение с БД не открыто!");
            return;
        }

        string currentUser = PlayerPrefs.GetString(UserNameKey);
        if (string.IsNullOrEmpty(currentUser)) return;

        try
        {
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = $"UPDATE users SET {ghostColumnName} = 1 WHERE name = @name;";
                var nameParam = command.CreateParameter(); nameParam.ParameterName = "@name"; nameParam.Value = currentUser; command.Parameters.Add(nameParam);
                command.ExecuteNonQuery();
                Debug.Log($"Прогресс для '{ghostColumnName}' успешно записан в БД.");
            }
        }
        catch (System.Exception ex) { Debug.LogError($"Ошибка при ЗАПИСИ прогресса: {ex.Message}"); }
    }

    public void Trans()
    {
        SceneManager.LoadScene(3);
    }
}