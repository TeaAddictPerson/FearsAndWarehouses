using UnityEngine;
using System.Collections;

public class SilverMirror : GhostItem
{
    [Header("Настройки зеркала")]
    public Transform weaponParent;
    public float absorptionRadius = 1.5f;

    private bool isCatching = false;

    protected override void Start()
    {
        itemTransform = weaponParent;
        maxMoveDistance = 0.5f;
        moveSpeed = 4f;
        base.Start();
    }

    protected override void StartUsing()
    {
        base.StartUsing();
        isCatching = true;
    }

    protected override void ContinueUsing()
    {
        base.ContinueUsing();

        if (isCatching)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, absorptionRadius);
            bool foundPhantom = false;

            float coneAngle = 45f;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Phantom"))
                {
                    Vector3 directionToPhantom = (hit.transform.position - weaponParent.position).normalized;
                    float angleToPhantom = Vector3.Angle(weaponParent.forward, directionToPhantom);

                    if (angleToPhantom <= coneAngle)
                    {
                        PhantomGhost ghost = hit.GetComponent<PhantomGhost>();
                        if (ghost != null && (ghost.canBeCaught || ghost.IsChasing()))
                        {
                            StartCoroutine(CatchGhost(ghost));
                            foundPhantom = true;
                            break;
                        }
                    }
                }
            }
        }
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        isCatching = false;
    }

    private IEnumerator CatchGhost(PhantomGhost ghost)
    {
        isCatching = false;

        Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>();
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    foreach (var mat in renderer.materials)
                    {
                        Color color = mat.color;
                        color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
                        mat.color = color;
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        GameObject dustParticles = GameObject.FindGameObjectWithTag("Dust");
        if (dustParticles != null)
        {
            dustParticles.transform.position = ghost.transform.position;
            ParticleSystem particles = dustParticles.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Play();
            }
        }

        ghost.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

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