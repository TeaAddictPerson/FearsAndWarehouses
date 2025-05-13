using UnityEngine;

public class Cross : GhostItem
{
    [Header("Настройки распятия")]
    public float crossRange = 2f; // Дальность действия распятия

    protected override void ContinueUsing()
    {
        base.ContinueUsing();
        // TODO: Добавить логику уничтожения призраков
    }
} 