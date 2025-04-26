using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;

public class EquipWeapon : MonoBehaviour
{
    public GameObject item;
    public Transform WeaponParent;
    public Button[] ghostTypeButtons; // Кнопки выбора типа призрака
    public Button backButton; // Кнопка возврата к выбору типа
    
    [Header("Ghost Information Fields")]
    public TextMeshProUGUI ghostNameText; // Имя призрака
    public TextMeshProUGUI ghostDescriptionText; // Описание призрака
    public TextMeshProUGUI ghostBehaviorText; // Поведение призрака
    
    public GameObject infoPanel; // Панель с информацией о призраке

    private Rigidbody itemRb;
    private MeshCollider itemCollider;
    private static bool handOccupied = false;
    private bool isEquipped = false;
    private string connectionString;

    void Start()
    {
        if (item != null)
        {
            itemRb = item.GetComponent<Rigidbody>();
            itemCollider = item.GetComponent<MeshCollider>();
            itemRb.isKinematic = false;
            itemCollider.enabled = true;
            item.transform.SetParent(null);
        }

        // Инициализация подключения к БД
        string dbPath = Path.Combine(Application.dataPath, "FearsAndWarehouses.db");
        connectionString = $"URI=file:{dbPath}";

        // Скрываем UI при старте
        if (infoPanel != null)
            infoPanel.SetActive(false);

        // Добавляем обработчики для кнопок
        if (ghostTypeButtons != null)
        {
            for (int i = 0; i < ghostTypeButtons.Length; i++)
            {
                int index = i; // Для замыкания
                ghostTypeButtons[i].onClick.AddListener(() => OnGhostTypeSelected(index));
            }
        }

        // Добавляем обработчик для кнопки возврата
        if (backButton != null)
        {
            backButton.onClick.AddListener(ShowGhostTypeButtons);
        }
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

            // Скрываем UI при выбрасывании
            if (infoPanel != null)
                infoPanel.SetActive(false);
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

            // Поворачиваем планшет
            item.transform.Rotate(180f, 0f, 180f);

            // Показываем панель с информацией
            if (infoPanel != null)
                infoPanel.SetActive(true);
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
        // Показываем кнопки выбора типа
        foreach (var button in ghostTypeButtons)
        {
            button.gameObject.SetActive(true);
        }

        // Скрываем информацию о призраке
        if (ghostNameText != null) ghostNameText.text = "";
        if (ghostDescriptionText != null) ghostDescriptionText.text = "";
        if (ghostBehaviorText != null) ghostBehaviorText.text = "";
    }

    private void OnGhostTypeSelected(int ghostTypeIndex)
    {
        // Скрываем кнопки выбора типа
        foreach (var button in ghostTypeButtons)
        {
            button.gameObject.SetActive(false);
        }

        // Загружаем информацию о призраке из БД
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