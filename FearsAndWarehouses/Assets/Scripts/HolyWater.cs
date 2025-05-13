using UnityEngine;

public class HolyWater : GhostItem
{
    [Header("Настройки святой воды")]
    public GameObject waterParticleSystem; // Префаб системы частиц для воды
    public float waterRange = 2f; // Дальность разбрызгивания воды
    private GameObject currentWaterEffect;

    protected override void StartUsing()
    {
        base.StartUsing();
        // Создаем эффект воды
        if (waterParticleSystem != null)
        {
            currentWaterEffect = Instantiate(waterParticleSystem, transform.position + transform.forward * waterRange, transform.rotation);
            currentWaterEffect.transform.parent = transform;
        }
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        // Уничтожаем эффект воды
        if (currentWaterEffect != null)
        {
            Destroy(currentWaterEffect);
            currentWaterEffect = null;
        }
    }

    protected override void ContinueUsing()
    {
        base.ContinueUsing();
        
        // Проверяем попадание воды на призраков
        if (currentWaterEffect != null)
        {
            Collider[] colliders = Physics.OverlapSphere(currentWaterEffect.transform.position, waterRange);
            foreach (Collider collider in colliders)
            {
                PhantomGhost ghost = collider.GetComponent<PhantomGhost>();
                if (ghost != null)
                {
                    // Наносим урон призраку
                    // TODO: Добавить систему урона для призраков
                }
            }
        }
    }
} 