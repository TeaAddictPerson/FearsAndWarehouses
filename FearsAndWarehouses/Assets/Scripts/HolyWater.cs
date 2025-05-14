using UnityEngine;

public class HolyWater : StaticGhostItem
{
    [Header("Настройки святой воды")]
    public Transform weaponParent; // Точка удержания предмета
    public ParticleSystem holyWaterParticle; // Система частиц святой воды

    private void Start()
    {
        if (holyWaterParticle == null)
        {
            Debug.LogError("HolyWater: holyWaterParticle не назначен!");
        }
    }

    protected override void StartUsing()
    {
        base.StartUsing();
        if (holyWaterParticle != null)
        {
            holyWaterParticle.Play();
        }
    }

    protected override void ContinueUsing()
    {
        base.ContinueUsing();
        if (holyWaterParticle != null && !holyWaterParticle.isPlaying)
        {
            holyWaterParticle.Play();
        }
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        if (holyWaterParticle != null)
        {
            holyWaterParticle.Stop();
        }
    }
}