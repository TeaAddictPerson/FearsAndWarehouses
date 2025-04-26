using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using TMPro;
using System.Data;
using System.IO;

public class Tablet : MonoBehaviour
{
    public GameObject item;
    public Transform WeaponParent;
    public Button[] ghostTypeButtons;
    public Button backButton; // Кнопка возврата к выбору типа
    public GhostInfoManager ghostInfoManager;

    [Header("Ghost Information Fields")]
    public TextMeshProUGUI ghostNameText; // Имя призрака
    public TextMeshProUGUI ghostDescriptionText; // Описание призрака
    public TextMeshProUGUI ghostBehaviorText;

    private Rigidbody itemRb;
    private BoxCollider itemCollider;
    private static bool handOccupied = false;
    private bool isEquipped = false;
    private string connectionString;

    void Start()
    {
        if (item != null)
        {
            itemRb = item.GetComponent<Rigidbody>();
            itemCollider = item.GetComponent<BoxCollider>();
            itemRb.isKinematic = false;
            itemCollider.enabled = true;
            item.transform.SetParent(null);
        }

        string dbPath = Path.Combine(Application.dataPath, "FearsAndWarehouses.db");
        connectionString = $"URI=file:{dbPath}";

        // Скрываем все UI элементы при старте
        HideAllUI();

        // Настраиваем события для кнопок выбора типа
        if (ghostTypeButtons != null)
        {
            for (int i = 0; i < ghostTypeButtons.Length; i++)
            {
                if (ghostTypeButtons[i] != null)
                {
                    int index = i;
                    ghostTypeButtons[i].onClick.RemoveAllListeners(); // Очищаем старые обработчики
                    ghostTypeButtons[i].onClick.AddListener(() => OnGhostTypeSelected(index));
                }
            }
        }

        // Настраиваем событие для кнопки возврата
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners(); // Очищаем старые обработчики
            backButton.onClick.AddListener(ShowGhostTypeButtons);
        }

        // Скрываем курсор при старте
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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

    void Update()
    {
        if (isEquipped && Input.GetKeyDown(KeyCode.F))
        {
            Drop();
        }
    }

    void Drop()
    {
        if (item != null)
        {
            isEquipped = false;
            handOccupied = false;

            WeaponParent.DetachChildren();
            item.transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
            itemRb.isKinematic = false;
            itemCollider.enabled = true;

            if (ghostInfoManager != null)
                ghostInfoManager.HideUI();

            // Скрываем все UI элементы при выбрасывании
            HideAllUI();

            // Скрываем курсор и блокируем его при выбрасывании
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Equip()
    {
        if (item != null && !handOccupied)
        {
            isEquipped = true;
            handOccupied = true;

            itemRb.isKinematic = true;
            itemCollider.enabled = false;

            item.transform.position = WeaponParent.position;
            item.transform.rotation = WeaponParent.rotation;
            item.transform.SetParent(WeaponParent);

            item.transform.Rotate(180f, 0f, 180f);

            if (ghostInfoManager != null)
                ghostInfoManager.ShowUI();

            // Показываем только кнопки выбора типа при экипировке
            ShowGhostTypeButtons();

            // Показываем курсор и разблокируем его при экипировке
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isEquipped && !handOccupied && other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Equip();
            }
        }
    }

    private void ShowGhostTypeButtons()
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

    private void OnGhostTypeSelected(int ghostTypeIndex)
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