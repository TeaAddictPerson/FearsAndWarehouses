using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Здоровье")]
    public int maxHealth = 3;
    private int currentHealth;
    public bool isDead = false;
    public int CurrentHealth => currentHealth;

    // ... existing code ...
} 