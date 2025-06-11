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
    private const string UserNameKey = "LoggedInUserName"; // Ключ для PlayerPrefs

    void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "FearsAndWarehouses.db");
    }

    public void AddUser()
    {
        string userName = userNameInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        // (Ваш код валидации остается без изменений)
        if (string.IsNullOrEmpty(userName))
        {
            feedbackText.text = "Имя пользователя не может быть пустым!";
            return;
        }
        string passwordFeedback = CheckPasswordErrors(password);
        if (!string.IsNullOrEmpty(passwordFeedback))
        {
            feedbackText.text = passwordFeedback;
            return;
        }

        string connectionString = $"URI=file:{dbPath}";

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // Проверяем, существует ли пользователь
                string checkQuery = "SELECT COUNT(*) FROM users WHERE name = @name;";
                using (IDbCommand checkCommand = dbConnection.CreateCommand())
                {
                    checkCommand.CommandText = checkQuery;
                    IDbDataParameter nameParam = checkCommand.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.Value = userName;
                    checkCommand.Parameters.Add(nameParam);
                    if (Convert.ToInt32(checkCommand.ExecuteScalar()) > 0)
                    {
                        feedbackText.text = "Пользователь с таким именем уже существует!";
                        return;
                    }
                }

                // --- ИЗМЕНЕНИЕ ЗДЕСЬ ---
                // Добавляем нового пользователя с нулевым прогрессом
                string query = "INSERT INTO users (name, password, phantom, soul, polter) VALUES (@name, @password, 0, 0, 0);";

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
                        feedbackText.text = "Регистрация успешна! Вход...";
                        Debug.Log($"Пользователь '{userName}' успешно добавлен в базу данных с нулевым прогрессом.");

                        // --- ДОБАВЛЕНО ЗДЕСЬ ---
                        // Сохраняем имя пользователя в PlayerPrefs для использования в других сценах
                        PlayerPrefs.SetString(UserNameKey, userName);
                        PlayerPrefs.Save();
                        Debug.Log($"Имя пользователя '{userName}' сохранено в PlayerPrefs.");

                        // Сразу переходим в игру
                        SceneManager.LoadScene(2);
                    }
                    else
                    {
                        feedbackText.text = "Не удалось добавить пользователя.";
                    }
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
        // (Ваш код валидации пароля остается без изменений)
        if (password.Length < 8) return "Пароль должен содержать не менее 8 символов";
        if (password.Length > 12) return "Пароль должен содержать не более 12 символов";
        bool hasLetter = false, hasDigit = false, hasSpecialChar = false;
        List<string> missingComponents = new List<string>();
        foreach (char c in password)
        {
            if (char.IsLower(c)) hasLetter = true;
            if (char.IsDigit(c)) hasDigit = true;
            if ("_+-/()".Contains(c)) hasSpecialChar = true;
        }
        if (!hasLetter) missingComponents.Add("букву");
        if (!hasDigit) missingComponents.Add("цифру");
        if (!hasSpecialChar) missingComponents.Add("специальный символ (_+-/())");
        if (missingComponents.Count > 0) return "Пароль должен содержать: " + string.Join(", ", missingComponents) + ".";
        return string.Empty;
    }
}