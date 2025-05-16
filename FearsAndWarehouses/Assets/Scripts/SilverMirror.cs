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
        // Используем weaponParent как itemTransform, чтобы управлять позицией
        itemTransform = weaponParent;

        // Можно тут задать свои значения скорости и максимального перемещения
        maxMoveDistance = 0.5f;
        moveSpeed = 4f;

        base.Start(); // Важно вызвать базовый старт после настройки полей
    }

    protected override void StartUsing()
    {
        base.StartUsing();
        isCatching = true;
        Debug.Log("Зеркало: Начато использование (StartUsing)");
    }

    protected override void ContinueUsing()
    {
        base.ContinueUsing();

        if (isCatching)
        {
            Debug.Log("Зеркало: Ищем призраков в конусе...");

            // Сначала ищем все в большом радиусе (absorptionRadius)
            Collider[] hits = Physics.OverlapSphere(transform.position, absorptionRadius);
            bool foundPhantom = false;

            float coneAngle = 45f; // Половина угла конуса в градусах (можно менять)

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
                            Debug.Log($"Зеркало: Призрак {ghost.name} найден в конусе и будет пойман!");
                            StartCoroutine(CatchGhost(ghost));
                            foundPhantom = true;
                            break;
                        }
                        else
                        {
                            Debug.Log($"Зеркало: Призрак {ghost?.name} найден, но ещё не активен для поимки.");
                        }
                    }
                }
            }

            if (!foundPhantom)
            {
                Debug.Log("Зеркало: Подходящих призраков в конусе не найдено.");
            }
        }
    }

    protected override void StopUsing()
    {
        base.StopUsing();
        isCatching = false;
        Debug.Log("Зеркало: Использование завершено (StopUsing)");
    }

    private IEnumerator CatchGhost(PhantomGhost ghost)
    {
        isCatching = false;
        Debug.Log($"Зеркало: Поглощаем призрака {ghost.name}...");

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

        // Находим партиклы и перемещаем их на место смерти призрака
        GameObject dustParticles = GameObject.FindGameObjectWithTag("Dust");
        if (dustParticles != null)
        {
            Vector3 ghostPosition = ghost.transform.position;
            dustParticles.transform.position = ghostPosition;
            ParticleSystem particles = dustParticles.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Play();
            }
        }

        Debug.Log($"Зеркало: Призрак {ghost.name} пойман.");
        ghost.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        // Радиус для базового круга у основания конуса (можно уменьшить, т.к. конус — длинный)
        float coneLength = 5f; // увеличиваем длину конуса (например, 5 метров)
        float coneAngle = 45f; // угол конуса в градусах

        // Позиция и направление конуса
        Vector3 origin = weaponParent ? weaponParent.position : transform.position;
        Vector3 forward = weaponParent ? weaponParent.forward : transform.forward;

        // Нарисуем линию центрального луча конуса
        Gizmos.DrawLine(origin, origin + forward * coneLength);

        // Вычислим радиус основания конуса (высота * тангенс угла)
        float baseRadius = coneLength * Mathf.Tan(coneAngle * Mathf.Deg2Rad);

        // Нарисуем окружность основания конуса
        int segments = 24;
        Vector3 prevPoint = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (360f / segments) * i;
            // Вектор радиуса окружности в локальных координатах
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
