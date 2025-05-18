using UnityEngine;

public class Incense : StaticGhostItem
{
    [Header("Настройки благовоний")]
    public ParticleSystem incenseParticle; // Частицы дыма благовоний
    private EquipWeapon equipWeapon;
    private bool hasBeenUsed = false;

    public bool IsActive => hasBeenUsed;

    private void Awake()
    {
        // Ищем EquipWeapon на родительском объекте
        equipWeapon = GetComponentInParent<EquipWeapon>();
        Debug.Log($"Incense: Инициализация благовоний. EquipWeapon найден: {equipWeapon != null}");
    }

    private void Start()
    {
        if (incenseParticle == null)
        {
            Debug.LogError("Incense: incenseParticle не назначен!");
        }
        else
        {
            incenseParticle.Stop();
            Debug.Log("Incense: Частицы остановлены");
        }
    }

    protected override void StartUsing()
    {
        base.StartUsing();
        Debug.Log($"Incense: Попытка использования. hasBeenUsed: {hasBeenUsed}, IsEquipped: {IsEquipped()}");

        if (!hasBeenUsed && IsEquipped())
        {
            incenseParticle.Play();
            hasBeenUsed = true;
            Debug.Log("Incense: Благовония зажжены");
        }
        else
        {
            Debug.Log($"Incense: Благовония уже использованы или не экипированы. hasBeenUsed: {hasBeenUsed}, IsEquipped: {IsEquipped()}");
        }
    }

    protected override void ContinueUsing()
    {
        base.ContinueUsing();
        Debug.Log($"Incense: Продолжение использования. IsActive: {IsActive}");
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        Debug.Log($"Incense: Остановка использования. IsActive: {IsActive}");
    }

    private bool IsEquipped()
    {
        bool equipped = equipWeapon != null && equipWeapon.IsEquipped;
        Debug.Log($"Incense: Проверка экипировки. EquipWeapon: {equipWeapon != null}, IsEquipped: {equipped}");
        return equipped;
    }
}
