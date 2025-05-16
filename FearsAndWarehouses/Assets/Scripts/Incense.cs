using UnityEngine;

public class Incense : StaticGhostItem
{
    [Header("Настройки благовоний")]
    public ParticleSystem incenseParticle; // Частицы дыма благовоний
    private EquipWeapon equipWeapon;
    private bool hasBeenUsed = false;

    private void Awake()
    {
        equipWeapon = GetComponent<EquipWeapon>();
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
        }
    }

    protected override void StartUsing()
    {
        base.StartUsing();

        if (!hasBeenUsed && IsEquipped())
        {
            incenseParticle.Play();
            hasBeenUsed = true;
        }
    }

    protected override void ContinueUsing()
    {
        base.ContinueUsing();
        // Ничего не делаем: частицы запускаются один раз и больше не останавливаются
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        // Также ничего не делаем: частицы продолжают идти
    }

    private bool IsEquipped()
    {
        return equipWeapon != null && equipWeapon.IsEquipped;
    }
}
