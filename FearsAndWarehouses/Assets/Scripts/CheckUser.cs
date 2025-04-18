using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckUser : MonoBehaviour
{
    public TMP_InputField userNameInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText;
    private string dbPath;

    void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "FearsAndWarehouses.db");
    }

    public void Check()
    {
        string userName = userNameInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            feedbackText.text = "Имя пользователя и пароль не могут быть пустыми!";
            Debug.LogWarning("Имя пользователя и пароль не могут быть пустыми!");
            return;
        }

        string connectionString = $"URI=file:{dbPath}";

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                if (dbConnection.State == ConnectionState.Open)
                {
                    Debug.Log("Подключение к базе данных успешно!");

                    string query = "SELECT password FROM users WHERE name = @name;";

                    using (IDbCommand command = dbConnection.CreateCommand())
                    {
                        command.CommandText = query;

                        IDbDataParameter nameParam = command.CreateParameter();
                        nameParam.ParameterName = "@name";
                        nameParam.Value = userName;
                        command.Parameters.Add(nameParam);

                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            string storedPassword = result.ToString();

                            if (storedPassword == password)
                            {
                                feedbackText.text = "Успешный вход!";
                                Debug.Log("Успешный вход!");
                                //SceneManager.LoadScene(3);

                                UserSession.UserName = userName;
                                Debug.Log($"Имя пользователя сохранено: {UserSession.UserName}");
                            }
                            else
                            {
                                feedbackText.text = "Неправильный пароль!";
                                Debug.LogWarning("Неправильный пароль!");
                            }
                        }
                        else
                        {
                            feedbackText.text = "Пользователь не найден!";
                            Debug.LogWarning("Пользователь не найден!");
                        }
                    }
                }
                else
                {
                    feedbackText.text = "Не удалось подключиться к базе данных.";
                    Debug.LogWarning("Не удалось подключиться к базе данных.");
                }
            }
        }
        catch (Exception ex)
        {
            feedbackText.text = "Ошибка при проверке пользователя.";
            Debug.LogError($"Ошибка при проверке пользователя: {ex.Message}");
        }
    }
}