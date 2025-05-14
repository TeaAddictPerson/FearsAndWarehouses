using UnityEngine;

public class Cross : GhostItem
{
    [Header("Настройки креста")]
    public Transform weaponParent; // Родительский объект для движения
    public float repulsionRadius = 2f; // Радиус отталкивания призраков

    private void Start()
    {
        base.Start();
        shouldMoveForward = true;
        itemTransform = weaponParent;
        originalPosition = weaponParent.localPosition;
        targetPosition = originalPosition + Vector3.forward * maxMoveDistance;
    }

    protected override void StartUsing()
    {
        base.StartUsing();
    }

    protected override void ContinueUsing()
    {
        base.ContinueUsing();
        if (shouldMoveForward)
        {
            // Плавно двигаем крест вперед
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetPosition, Time.deltaTime * moveSpeed);
        }
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        // Возвращаем крест в исходное положение
        weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, originalPosition, Time.deltaTime * moveSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация радиуса отталкивания
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, repulsionRadius);
    }
} 