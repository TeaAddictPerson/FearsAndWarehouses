using UnityEngine;

public class HolyWater : StaticGhostItem
{
    [Header("Настройки святой воды")]
    public ParticleSystem holyWaterParticle; // Система частиц святой воды
    private EquipWeapon equipWeapon;

    private void Awake()
    {
        equipWeapon = GetComponent<EquipWeapon>();
    }

    private void Start()
    {
        if (holyWaterParticle == null)
        {
            Debug.LogError("HolyWater: holyWaterParticle не назначен!");
        }
        else
        {
            holyWaterParticle.Stop();
        }
    }

    protected override void StartUsing()
    {
        base.StartUsing();
        if (IsEquipped() && !holyWaterParticle.isPlaying)
        {
            holyWaterParticle.Play();
        }
    }

    protected override void ContinueUsing()
    {
        base.ContinueUsing();
        if (IsEquipped() && !holyWaterParticle.isPlaying)
        {
            holyWaterParticle.Play();
        }
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        if (holyWaterParticle.isPlaying)
        {
            holyWaterParticle.Stop();
        }
    }

    private bool IsEquipped()
    {
        return equipWeapon != null && equipWeapon.IsEquipped;
    }
}
