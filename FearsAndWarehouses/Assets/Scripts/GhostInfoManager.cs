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
        string dbPath = Path.Combine(Application.dataPath, "FearsAndWarehouses.db");
        connectionString = $"URI=file:{dbPath}";

        HideAllUI();

        SetupButtons();
    }

    private void SetupButtons()
    {

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
        if (ghostTypeButtons != null)
        {
            foreach (var button in ghostTypeButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(false);
            }
        }

        if (backButton != null)
            backButton.gameObject.SetActive(false);

        if (ghostNameText != null)
            ghostNameText.gameObject.SetActive(false);
        if (ghostDescriptionText != null)
            ghostDescriptionText.gameObject.SetActive(false);
        if (ghostBehaviorText != null)
            ghostBehaviorText.gameObject.SetActive(false);
    }

    private void ShowGhostTypeButtons()
    {
        HideAllUI();

        if (ghostTypeButtons != null)
        {
            foreach (var button in ghostTypeButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(true);
            }
        }

        if (ghostNameText != null) ghostNameText.text = "";
        if (ghostDescriptionText != null) ghostDescriptionText.text = "";
        if (ghostBehaviorText != null) ghostBehaviorText.text = "";
    }

    public void OnGhostTypeSelected(int ghostTypeIndex)
    {
        if (ghostTypeButtons != null)
        {
            foreach (var button in ghostTypeButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(false);
            }
        }

        if (backButton != null)
            backButton.gameObject.SetActive(true);
        if (ghostNameText != null)
            ghostNameText.gameObject.SetActive(true);
        if (ghostDescriptionText != null)
            ghostDescriptionText.gameObject.SetActive(true);
        if (ghostBehaviorText != null)
            ghostBehaviorText.gameObject.SetActive(true);

        LoadGhostInfo(ghostTypeIndex + 1); 
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
                                ghostNameText.text = reader.GetString(0); 
                            if (ghostDescriptionText != null)
                                ghostDescriptionText.text = reader.GetString(1); 
                            if (ghostBehaviorText != null)
                                ghostBehaviorText.text = reader.GetString(2); 
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