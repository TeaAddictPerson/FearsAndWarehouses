using UnityEngine;

public class HolyWater : StaticGhostItem
{
    [Header("Настройки святой воды")]
    public ParticleSystem holyWaterParticle; // Система частиц святой воды
    public float exorcismRadius = 2f; // Радиус действия святой воды
    public float exorcismDuration = 5f; // Время, необходимое для изгнания
    private EquipWeapon equipWeapon;
    private float currentExorcismTime = 0f;
    private LostSoul currentTarget = null;

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

        // Проверяем наличие призрака в радиусе действия
        CheckForGhost();
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        if (holyWaterParticle.isPlaying)
        {
            holyWaterParticle.Stop();
        }
        currentExorcismTime = 0f;
        currentTarget = null;
    }

    private bool IsEquipped()
    {
        return equipWeapon != null && equipWeapon.IsEquipped;
    }

    private void CheckForGhost()
    {
        if (!IsEquipped()) return;

        // Создаем сферу перед игроком для обнаружения призрака
        Vector3 playerForward = Camera.main.transform.forward;
        Vector3 checkPosition = Camera.main.transform.position + playerForward * (exorcismRadius / 2f);
        
        Collider[] colliders = Physics.OverlapSphere(checkPosition, exorcismRadius);
        
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("LostSoul"))
            {
                LostSoul ghost = collider.GetComponent<LostSoul>();
                if (ghost != null && ghost.isStunned)
                {
                    currentTarget = ghost;
                    currentExorcismTime += Time.deltaTime;
                    
                    if (currentExorcismTime >= exorcismDuration)
                    {
                        ghost.Exorcise();
                        currentExorcismTime = 0f;
                        currentTarget = null;
                    }
                    return;
                }
            }
        }
        
        // Если призрак не найден, сбрасываем таймер
        currentExorcismTime = 0f;
        currentTarget = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (Camera.main != null)
        {
            Vector3 playerForward = Camera.main.transform.forward;
            Vector3 checkPosition = Camera.main.transform.position + playerForward * (exorcismRadius / 2f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(checkPosition, exorcismRadius);
        }
    }
}
