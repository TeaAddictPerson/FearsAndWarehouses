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
    private const string UserNameKey = "LoggedInUserName"; // Ключ для PlayerPrefs

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
            return;
        }

        string connectionString = $"URI=file:{dbPath}";

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();
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
                            feedbackText.text = "Успешный вход! Подождите загрузку";
                            Debug.Log("Успешный вход!");

                            // --- ДОБАВЛЕНО ЗДЕСЬ ---
                            // Сохраняем имя пользователя в PlayerPrefs для использования в других сценах
                            PlayerPrefs.SetString(UserNameKey, userName);
                            PlayerPrefs.Save();
                            Debug.Log($"Имя пользователя '{userName}' сохранено в PlayerPrefs.");

                            // Переходим в игру
                            SceneManager.LoadScene(2);
                        }
                        else
                        {
                            feedbackText.text = "Неправильный пароль!";
                        }
                    }
                    else
                    {
                        feedbackText.text = "Пользователь не найден!";
                    }
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