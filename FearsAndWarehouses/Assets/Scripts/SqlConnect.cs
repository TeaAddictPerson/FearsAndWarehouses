using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;

public class SqlConnect : MonoBehaviour
{
    void Start()
    {
        CheckDatabaseConnection();
    }

    void CheckDatabaseConnection()
    {
      
        string dbPath = Path.Combine(Application.dataPath, "OwlsCurse.db");

  
        string connectionString = $"URI=file:{dbPath}";

        try
        {
            
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open(); 

               
                if (dbConnection.State == ConnectionState.Open)
                {
                    Debug.Log("Подключение к базе данных успешно!");
                }
                else
                {
                    Debug.LogWarning("Не удалось подключиться к базе данных.");
                }
            }
        }
        catch (System.Exception ex)
        {
            
            Debug.LogError($"Ошибка при подключении к базе данных: {ex.Message}");
        }
    }
}