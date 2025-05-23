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

        // Проверяем наличие полтергейста в конусе
        Collider[] hits = Physics.OverlapSphere(transform.position, repulsionRadius);
        float coneAngle = 45f;

        Debug.Log($"Cross: Проверка в радиусе {repulsionRadius}. Найдено объектов: {hits.Length}");

        foreach (var hit in hits)
        {
            // Проверяем тег на основном объекте и его родителе
            bool isPoltergeist = hit.CompareTag("Poltergeist") || 
                                (hit.transform.parent != null && hit.transform.parent.CompareTag("Poltergeist"));

            if (isPoltergeist)
            {
                Debug.Log($"Cross: Найден полтергейст в радиусе. Объект: {hit.name}, Родитель: {hit.transform.parent?.name}");
                
                // Получаем компонент Poltergeist с родительского объекта, если он есть
                Poltergeist ghost = hit.GetComponent<Poltergeist>();
                if (ghost == null && hit.transform.parent != null)
                {
                    ghost = hit.transform.parent.GetComponent<Poltergeist>();
                }

                if (ghost != null)
                {
                    Vector3 directionToGhost = (ghost.transform.position - weaponParent.position).normalized;
                    float angleToGhost = Vector3.Angle(weaponParent.forward, directionToGhost);

                    Debug.Log($"Cross: Угол до полтергейста: {angleToGhost}, максимальный угол: {coneAngle}");

                    if (angleToGhost <= coneAngle)
                    {
                        Debug.Log("Cross: Полтергейст в конусе, начинаем изгнание");
                        ghost.StartExorcism();
                        Debug.Log("Cross: Изгнание начато");
                        break;
                    }
                }
                else
                {
                    Debug.LogError($"Cross: Компонент Poltergeist не найден на объекте {hit.name} или его родителе!");
                }
            }
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

        // Визуализация конуса
        float coneLength = 5f;
        float coneAngle = 45f;

        Vector3 origin = weaponParent ? weaponParent.position : transform.position;
        Vector3 forward = weaponParent ? weaponParent.forward : transform.forward;

        Gizmos.DrawLine(origin, origin + forward * coneLength);

        float baseRadius = coneLength * Mathf.Tan(coneAngle * Mathf.Deg2Rad);

        int segments = 24;
        Vector3 prevPoint = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (360f / segments) * i;
            Vector3 circlePoint = Quaternion.AngleAxis(angle, forward) * (weaponParent.right * baseRadius);
            Vector3 worldPoint = origin + forward * coneLength + circlePoint;

            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, worldPoint);
                Gizmos.DrawLine(origin, worldPoint);
            }
            prevPoint = worldPoint;
        }
    }
}