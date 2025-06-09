using UnityEngine;

public class Cross : GhostItem
{
    [Header("Настройки креста")]
    public Transform weaponParent;
    public float repulsionRadius = 2f;

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
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetPosition, Time.deltaTime * moveSpeed);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, repulsionRadius);
        float coneAngle = 45f;

        foreach (var hit in hits)
        {
            bool isPoltergeist = hit.CompareTag("Poltergeist") ||
                                (hit.transform.parent != null && hit.transform.parent.CompareTag("Poltergeist"));

            if (isPoltergeist)
            {
                Poltergeist ghost = hit.GetComponent<Poltergeist>();
                if (ghost == null && hit.transform.parent != null)
                {
                    ghost = hit.transform.parent.GetComponent<Poltergeist>();
                }

                if (ghost != null)
                {
                    Vector3 directionToGhost = (ghost.transform.position - weaponParent.position).normalized;
                    float angleToGhost = Vector3.Angle(weaponParent.forward, directionToGhost);

                    if (angleToGhost <= coneAngle)
                    {
                        ghost.StartExorcism();
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
        weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, originalPosition, Time.deltaTime * moveSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, repulsionRadius);

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
