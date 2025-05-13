using UnityEngine;

public class SilverMirror : GhostItem
{
    [Header("Настройки зеркала")]
    public float absorptionRange = 1.5f; // Дальность поглощения призрака
    public float absorptionTime = 2f; // Время, необходимое для поглощения
    private float currentAbsorptionTime = 0f;

    protected override void ContinueUsing()
    {
        base.ContinueUsing();

        // Проверяем наличие призрака в радиусе действия
        Collider[] colliders = Physics.OverlapSphere(transform.position, absorptionRange);
        foreach (Collider collider in colliders)
        {
            PhantomGhost ghost = collider.GetComponent<PhantomGhost>();
            if (ghost != null)
            {
                currentAbsorptionTime += Time.deltaTime;
                if (currentAbsorptionTime >= absorptionTime)
                {
                    // Уничтожаем призрака
                    Destroy(ghost.gameObject);
                    currentAbsorptionTime = 0f;
                }
                return;
            }
        }
        currentAbsorptionTime = 0f;
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        currentAbsorptionTime = 0f;
    }
} 