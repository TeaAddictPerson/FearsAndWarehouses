using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;

public class GhostInfoManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] ghostTypeButtons;
    public Button backButton;
    public TextMeshProUGUI ghostNameText;
    public TextMeshProUGUI ghostDescriptionText;
    public TextMeshProUGUI ghostBehaviorText;

    private string connectionString;

    void Start()
    {
        // Инициализация подключения к БД
        string dbPath = Path.Combine(Application.dataPath, "FearsAndWarehouses.db");
        connectionString = $"URI=file:{dbPath}";

        // Скрываем все UI элементы при старте
        HideAllUI();

        // Настраиваем события для кнопок
        SetupButtons();
    }

    private void SetupButtons()
    {
        // Настраиваем кнопки выбора типа
        if (ghostTypeButtons != null)
        {
            for (int i = 0; i < ghostTypeButtons.Length; i++)
            {
                if (ghostTypeButtons[i] != null)
                {
                    int index = i;
                    ghostTypeButtons[i].onClick.RemoveAllListeners();
                    ghostTypeButtons[i].onClick.AddListener(() => OnGhostTypeSelected(index));
                }
            }
        }

        // Настраиваем кнопку возврата
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ShowGhostTypeButtons);
        }
    }

    public void ShowUI()
    {
        ShowGhostTypeButtons();
    }

    public void HideUI()
    {
        HideAllUI();
    }

    private void HideAllUI()
    {
        // Скрываем все кнопки выбора типа
        if (ghostTypeButtons != null)
        {
            foreach (var button in ghostTypeButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(false);
            }
        }

        // Скрываем кнопку возврата
        if (backButton != null)
            backButton.gameObject.SetActive(false);

        // Скрываем текстовые поля
        if (ghostNameText != null)
            ghostNameText.gameObject.SetActive(false);
        if (ghostDescriptionText != null)
            ghostDescriptionText.gameObject.SetActive(false);
        if (ghostBehaviorText != null)
            ghostBehaviorText.gameObject.SetActive(false);
    }

    public void ShowGhostTypeButtons()
    {
        // Скрываем все UI элементы сначала
        HideAllUI();

        // Показываем только кнопки выбора типа
        if (ghostTypeButtons != null)
        {
            foreach (var button in ghostTypeButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(true);
            }
        }

        // Очищаем текстовые поля
        if (ghostNameText != null) ghostNameText.text = "";
        if (ghostDescriptionText != null) ghostDescriptionText.text = "";
        if (ghostBehaviorText != null) ghostBehaviorText.text = "";
    }

    public void OnGhostTypeSelected(int ghostTypeIndex)
    {
        // Скрываем кнопки выбора типа
        if (ghostTypeButtons != null)
        {
            foreach (var button in ghostTypeButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(false);
            }
        }

        // Показываем кнопку возврата и текстовые поля
        if (backButton != null)
            backButton.gameObject.SetActive(true);
        if (ghostNameText != null)
            ghostNameText.gameObject.SetActive(true);
        if (ghostDescriptionText != null)
            ghostDescriptionText.gameObject.SetActive(true);
        if (ghostBehaviorText != null)
            ghostBehaviorText.gameObject.SetActive(true);

        LoadGhostInfo(ghostTypeIndex + 1); // +1 потому что ghost_id начинается с 1
    }

    private void LoadGhostInfo(int ghostId)
    {
        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();
                using (IDbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = $"SELECT ghost_type, ghost_sign, ghost_exile FROM Ghosts WHERE ghost_id = {ghostId}";

                    using (IDataReader reader = dbCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (ghostNameText != null)
                                ghostNameText.text = reader.GetString(0); // ghost_type
                            if (ghostDescriptionText != null)
                                ghostDescriptionText.text = reader.GetString(1); // ghost_sign
                            if (ghostBehaviorText != null)
                                ghostBehaviorText.text = reader.GetString(2); // ghost_exile
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка при загрузке информации о призраке: {ex.Message}");
        }
    }
} 