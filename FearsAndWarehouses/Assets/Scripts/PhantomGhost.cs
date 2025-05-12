using UnityEngine;
using System.Collections;

public class PhantomGhost : MonoBehaviour
{
    [Header("Настройки патрулирования")]
    public Transform[] patrolPoints; // Точки патрулирования
    public float moveSpeed = 3f; // Скорость передвижения
    public float rotationSpeed = 5f; // Скорость поворота

    [Header("Настройки обнаружения")]
    public float detectionRange = 5f; // Дальность обнаружения игрока
    public float fieldOfView = 60f; // Угол обзора в градусах
    public float eyeHeight = -1.5f; // Высота глаз призрака
    public LayerMask obstacleLayer; // Слой препятствий для проверки видимости
    public float minHeight = 0.1f; // Минимальная высота над землей

    [Header("Настройки преследования")]
    public float chaseSpeed = 5f; // Скорость преследования
    public float chaseRange = 10f; // Максимальная дистанция преследования
    public float damageDistance = 1.5f; // Дистанция для нанесения урона

    private Transform player; // Ссылка на игрока
    private int currentPatrolIndex = 0; // Текущая точка патрулирования
    private bool isChasing = false; // Флаг преследования
    private Vector3 lastKnownPlayerPosition; // Последняя известная позиция игрока

    private void Start()
    {
        // Находим игрока по тегу
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Игрок не найден на сцене!");
        }

        // Устанавливаем начальный поворот модели
        transform.rotation = Quaternion.Euler(-90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    private void Update()
    {
        if (player == null) return;

        // Поддерживаем правильный поворот модели
        transform.rotation = Quaternion.Euler(-90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
            CheckForPlayer();
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        // Получаем текущую точку патрулирования
        Transform targetPoint = patrolPoints[currentPatrolIndex];

        // Поворачиваемся к точке
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        // Сохраняем поворот -90 по X, но применяем поворот по Y и Z
        targetRotation = Quaternion.Euler(-90f, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Двигаемся к точке
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        // Если достигли точки, переходим к следующей
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

   private void CheckForPlayer()
{
    Vector3 ghostEyePos = transform.position + Vector3.up * eyeHeight;
    Vector3 playerHeadPos = player.position + Vector3.up * 1.6f; // Примерная высота головы игрока

    float distanceToPlayer = Vector3.Distance(ghostEyePos, playerHeadPos);

    if (distanceToPlayer <= detectionRange)
    {
        Vector3 directionToPlayer = (playerHeadPos - ghostEyePos).normalized;

        if (!Physics.Raycast(ghostEyePos, directionToPlayer, distanceToPlayer, obstacleLayer))
        {
            float angle = Vector3.Angle(-transform.up, directionToPlayer);

            if (angle <= fieldOfView / 2f)
            {
                isChasing = true;
                lastKnownPlayerPosition = player.position;
                Debug.Log("Призрак обнаружил игрока и начал преследование!");
            }
        }
    }
}


    private void ChasePlayer()
    {
        // Проверяем расстояние на уровне ног
        Vector3 ghostPosition = transform.position + Vector3.up * eyeHeight;
        Vector3 playerPosition = player.position + Vector3.up * 1.6f; // Высота головы игрока


        float distanceToPlayer = Vector3.Distance(ghostPosition, playerPosition);

        // Если игрок слишком далеко, прекращаем преследование
        if (distanceToPlayer > chaseRange)
        {
            isChasing = false;
            return;
        }

        // Проверяем, видим ли мы игрока
        Vector3 directionToPlayer = (playerPosition - ghostPosition).normalized;
        if (!Physics.Raycast(ghostPosition, directionToPlayer, distanceToPlayer, obstacleLayer))
        {
            // Проверяем, находится ли игрок в поле зрения
            float angle = Vector3.Angle(-transform.up, directionToPlayer);
            if (angle <= fieldOfView / 2f)
            {
                lastKnownPlayerPosition = player.position;
            }
        }

        // Поворачиваемся к последней известной позиции игрока
        Vector3 targetDirection = (lastKnownPlayerPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        // Сохраняем поворот -90 по X
        targetRotation = Quaternion.Euler(-90f, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Проверяем высоту над землей
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 1f, obstacleLayer))
        {
            // Если высота меньше минимальной, поднимаем призрака
            if (hit.distance < minHeight)
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + minHeight, transform.position.z);
            }
        }

        // Двигаемся к последней известной позиции
        Vector3 targetPosition = Vector3.MoveTowards(transform.position, lastKnownPlayerPosition, chaseSpeed * Time.deltaTime);
        // Сохраняем текущую высоту при движении
        targetPosition.y = transform.position.y;
        transform.position = targetPosition;

        // Если достигли последней известной позиции и не видим игрока, прекращаем преследование
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 0.1f && distanceToPlayer > detectionRange)
        {
            isChasing = false;
        }

        // Проверяем дистанцию для нанесения урона
        if (distanceToPlayer <= damageDistance)
        {
            // Наносим урон игроку
            FirstPersonController playerController = player.GetComponent<FirstPersonController>();
            if (playerController != null)
            {
                playerController.TakeDamage(1);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация дальности обнаружения
        Gizmos.color = Color.yellow;
        Vector3 detectionCenter = transform.position + Vector3.up * eyeHeight;
        Gizmos.DrawWireSphere(detectionCenter, detectionRange);

        // Визуализация поля зрения
        Gizmos.color = Color.red;
        Vector3 rightDir = Quaternion.Euler(0, fieldOfView / 2f, 0) * -transform.up;
        Vector3 leftDir = Quaternion.Euler(0, -fieldOfView / 2f, 0) * -transform.up;
        Gizmos.DrawRay(detectionCenter, rightDir * detectionRange);
        Gizmos.DrawRay(detectionCenter, leftDir * detectionRange);

        // Визуализация дальности преследования
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(detectionCenter, chaseRange);
    }
}