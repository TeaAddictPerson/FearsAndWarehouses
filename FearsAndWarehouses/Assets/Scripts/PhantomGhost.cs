using UnityEngine;

public class PhantomGhost : MonoBehaviour
{
    [Header("Патрулирование")]
    public Transform[] patrolPoints;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("Обнаружение")]
    public float detectionRange = 5f;
    public float fieldOfView = 60f;
    public float eyeHeight = -1.5f;
    public LayerMask obstacleLayer;
    public float minHeight = 0.1f;

    [Header("Преследование")]
    public float chaseSpeed = 5f;
    public float chaseRange = 10f;
    public float damageDistance = 1.5f;

    private Transform player;
    private int currentPatrolIndex = 0;
    private bool isChasing = false;
    private Vector3 lastKnownPlayerPosition;
    private bool hasAttacked = false;

    [HideInInspector] public bool canBeCaught = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Игрок не найден!");
        }

        transform.rotation = Quaternion.Euler(-90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    private void Update()
    {
        if (player == null) return;

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

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        targetRotation = Quaternion.Euler(-90f, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void CheckForPlayer()
    {
        Vector3 ghostEyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 playerHeadPos = player.position + Vector3.up * 1.6f;

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
                    Debug.Log("Призрак начал преследование!");
                }
            }
        }
    }

    private void ChasePlayer()
    {
        Vector3 ghostPosition = transform.position + Vector3.up * eyeHeight;
        Vector3 playerPosition = player.position + Vector3.up * 1.6f;

        float distanceToPlayer = Vector3.Distance(ghostPosition, playerPosition);

        if (distanceToPlayer > chaseRange)
        {
            isChasing = false;
            return;
        }

        Vector3 directionToPlayer = (playerPosition - ghostPosition).normalized;
        if (!Physics.Raycast(ghostPosition, directionToPlayer, distanceToPlayer, obstacleLayer))
        {
            float angle = Vector3.Angle(-transform.up, directionToPlayer);
            if (angle <= fieldOfView / 2f)
            {
                lastKnownPlayerPosition = player.position;
            }
        }

        Vector3 targetDirection = (lastKnownPlayerPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        targetRotation = Quaternion.Euler(-90f, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1f, obstacleLayer))
        {
            if (hit.distance < minHeight)
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + minHeight, transform.position.z);
            }
        }

        Vector3 targetPosition = Vector3.MoveTowards(transform.position, lastKnownPlayerPosition, chaseSpeed * Time.deltaTime);
        targetPosition.y = transform.position.y;
        transform.position = targetPosition;

        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 0.1f && distanceToPlayer > detectionRange)
        {
            isChasing = false;
        }

        if (distanceToPlayer <= damageDistance)
        {
            FirstPersonController playerController = player.GetComponent<FirstPersonController>();
            if (playerController != null)
            {
                playerController.TakeDamage(1);
                if (!canBeCaught)
                {
                    canBeCaught = true;
                    hasAttacked = true;
                    Debug.Log($"{name}: Атаковал игрока — теперь может быть пойман!");
                }
            }
        }
    }

    public bool HasAttackedPlayer()
    {
        return hasAttacked;
    }

    public bool IsChasing()
    {
        return isChasing;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 detectionCenter = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(detectionCenter, detectionRange);

        Gizmos.color = Color.red;
        Vector3 rightDir = Quaternion.Euler(0, fieldOfView / 2f, 0) * -transform.up;
        Vector3 leftDir = Quaternion.Euler(0, -fieldOfView / 2f, 0) * -transform.up;
        Gizmos.DrawRay(detectionCenter, rightDir * detectionRange);
        Gizmos.DrawRay(detectionCenter, leftDir * detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(detectionCenter, chaseRange);
    }
}
