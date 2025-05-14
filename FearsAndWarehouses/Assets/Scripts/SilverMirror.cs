using UnityEngine;

public class SilverMirror : GhostItem
{
    [Header("Настройки зеркала")]
    public Transform weaponParent; // Родительский объект для движения
    public float absorptionRadius = 1.5f; // Радиус поглощения призраков

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
            // Плавно двигаем зеркало вперед
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetPosition, Time.deltaTime * moveSpeed);
        }
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        // Возвращаем зеркало в исходное положение
        weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, originalPosition, Time.deltaTime * moveSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация радиуса поглощения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, absorptionRadius);
    }
} 