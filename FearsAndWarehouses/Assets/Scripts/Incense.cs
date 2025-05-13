using UnityEngine;

public class Incense : GhostItem
{
    [Header("Настройки благовоний")]
    public GameObject smokeParticleSystem; // Префаб системы частиц для дыма
    public float smokeDuration = 3f; // Длительность эффекта дыма
    private GameObject currentSmokeEffect;
    private float smokeTimer = 0f;

    protected override void Update()
    {
        // Переопределяем базовый Update для работы по нажатию, а не удержанию
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartSmoke();
        }

        if (currentSmokeEffect != null)
        {
            smokeTimer += Time.deltaTime;
            if (smokeTimer >= smokeDuration)
            {
                StopSmoke();
            }
        }
    }

    private void StartSmoke()
    {
        if (smokeParticleSystem != null && currentSmokeEffect == null)
        {
            currentSmokeEffect = Instantiate(smokeParticleSystem, transform.position, transform.rotation);
            currentSmokeEffect.transform.parent = transform;
            smokeTimer = 0f;
        }
    }

    private void StopSmoke()
    {
        if (currentSmokeEffect != null)
        {
            Destroy(currentSmokeEffect);
            currentSmokeEffect = null;
            smokeTimer = 0f;
        }
    }

    // Отключаем базовую функциональность удержания
    protected override void StartUsing() { }
    protected override void ContinueUsing() { }
    protected override void StopUsing() { }
} 