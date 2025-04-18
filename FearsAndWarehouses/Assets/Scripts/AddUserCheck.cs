using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Mono.Data.Sqlite;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddUserCheck : MonoBehaviour
{
    public TMP_InputField userNameInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText;
    private string dbPath;

    void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "FearsAndWarehouses.db");
    }

    public void AddUser()
    {
        string userName = userNameInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        if (string.IsNullOrEmpty(userName))
        {
            feedbackText.text = "Имя пользователя не может быть пустым!";
            Debug.LogWarning("Имя пользователя не может быть пустым!");
            return;
        }

        string passwordFeedback = CheckPasswordErrors(password);
        if (!string.IsNullOrEmpty(passwordFeedback))
        {
            feedbackText.text = passwordFeedback;
            Debug.LogWarning(passwordFeedback);
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
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE name = @name;";

                    using (IDbCommand checkCommand = dbConnection.CreateCommand())
                    {
                        checkCommand.CommandText = checkQuery;

                        IDbDataParameter nameParam = checkCommand.CreateParameter();
                        nameParam.ParameterName = "@name";
                        nameParam.Value = userName;
                        checkCommand.Parameters.Add(nameParam);

                        int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (userCount > 0)
                        {
                            feedbackText.text = "Пользователь с таким именем уже существует!";
                            Debug.LogWarning("Пользователь с таким именем уже существует!");
                            return;
                        }
                    }

                    string query = "INSERT INTO users (name, password) VALUES (@name, @password);";

                    using (IDbCommand command = dbConnection.CreateCommand())
                    {
                        command.CommandText = query;

                        var nameParameter = command.CreateParameter();
                        nameParameter.ParameterName = "@name";
                        nameParameter.Value = userName;
                        command.Parameters.Add(nameParameter);

                        var passwordParameter = command.CreateParameter();
                        passwordParameter.ParameterName = "@password";
                        passwordParameter.Value = password;
                        command.Parameters.Add(passwordParameter);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            feedbackText.text = $"Пользователь '{userName}' успешно добавлен!";
                            Debug.Log($"Пользователь '{userName}' успешно добавлен в базу данных.");
                            SceneManager.LoadScene(3);
                           
                            UserSession.UserName = userName;
                            Debug.Log($"Имя пользователя сохранено: {UserSession.UserName}");
                        }
                        else
                        {
                            feedbackText.text = "Не удалось добавить пользователя.";
                            Debug.LogWarning("Не удалось добавить пользователя в базу данных.");
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
            feedbackText.text = "Ошибка при добавлении пользователя.";
            Debug.LogError($"Ошибка при добавлении пользователя: {ex.Message}");
        }
    }

    private string CheckPasswordErrors(string password)
    {
        if (password.Length < 8)
            return "Пароль должен содержать не менее 8 символов";

        if (password.Length > 12)
            return "Пароль должен содержать не более 12 символов";

        bool hasLetter = false;
        bool hasDigit = false;
        bool hasSpecialChar = false;
        List<string> missingComponents = new List<string>();

        foreach (char c in password)
        {
            if (char.IsLower(c)) hasLetter = true;
            if (char.IsDigit(c)) hasDigit = true;
            if ("_+-/()".Contains(c)) hasSpecialChar = true;
        }

        if (!hasLetter)
            missingComponents.Add("букву");
        if (!hasDigit)
            missingComponents.Add("цифру");
        if (!hasSpecialChar)
            missingComponents.Add("специальный символ (_+-/())");

        if (missingComponents.Count > 0)
        {
            return "Пароль должен содержать: " + string.Join(", ", missingComponents) + ".";
        }

        return string.Empty;
    }
}