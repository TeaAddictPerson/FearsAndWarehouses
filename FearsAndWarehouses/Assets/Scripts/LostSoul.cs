using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class LostSoul : MonoBehaviour
{
    [Header("Настройки заблудшей души")]
    public float moveSpeed = 3f;
    public float patrolRadius = 10f;
    public float attackRange = 1.2f;
    public float attackSpeed = 10f;
    public float stunDuration = 15f;
    public float cooldownDuration = 20f;
    public float playerStunDuration = 3f;
    public float minAttackDistance = 3f;
    public float maxAttackDistance = 8f;
    public AudioClip[] footstepSounds;
    public AudioClip attackSound;
    public AudioSource audioSource;

    [Header("Точки появления")]
    public Transform[] spawnPoints;
    public float initialSpawnDelay = 15f;

    [Header("Анимации")]
    private Animator animator;
    private readonly int IdleHash = Animator.StringToHash("Idle");
    private readonly int WalkHash = Animator.StringToHash("Walk");
    private readonly int AttackHash = Animator.StringToHash("ZombieAttack");
    private readonly int RunHash = Animator.StringToHash("RunForward");

    private NavMeshAgent agent;
    private Vector3 spawnPoint;
    private bool isVisible, isAttacking, isStunned, isOnCooldown, isMoving, isAggressive, isActive, isInitialized;
    private bool hasReactedToIncense = false;
    private float lastFootstepTime, currentCooldownTime, currentStunTime;
    private Vector3 attackTarget;

    [SerializeField] private Transform weaponParent;
    [SerializeField] private SkinnedMeshRenderer ghostRenderer;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float footstepInterval = 0.8f;

    private EquipWeapon playerEquipWeapon;
    private FirstPersonController playerController;

    private void Start()
    {
        Initialize();
        StartCoroutine(InitialSpawn());
    }

    private void Initialize()
    {
        if (isInitialized) return;

        agent = GetComponent<NavMeshAgent>() ?? gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.1f;

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        SetVisibility(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerEquipWeapon = player.GetComponent<EquipWeapon>();
            playerController = player.GetComponent<FirstPersonController>();
        }

        animator = GetComponent<Animator>();
        if (!animator) Debug.LogError("LostSoul: Animator not found!");

        isInitialized = true;
    }

    private IEnumerator InitialSpawn()
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        if (spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnAtPoint(point);
        }
    }

    private void SpawnAtPoint(Transform point)
    {
        transform.position = point.position;
        spawnPoint = point.position;
        isActive = true;
        isVisible = false;
        isAttacking = false;
        isStunned = false;
        isOnCooldown = false;
        isMoving = false;
        SetVisibility(false);

        if (animator)
        {
            animator.SetBool(WalkHash, false);
            animator.SetBool(IdleHash, true);
            animator.SetBool(RunHash, false);
        }

        agent.Warp(point.position);
    }

    private void Update()
    {
        if (!isActive) return;

        if (!hasReactedToIncense)
            CheckForIncense();

        if (isAggressive && !isAttacking && !isStunned && !isOnCooldown && !isPreparingAttack && agent.remainingDistance <= attackRange)
        {
            StartCoroutine(AttackRoutine());
        }


        if (!isAggressive && !isAttacking && !isStunned && !isOnCooldown)
        {
            Patrol();
            isMoving = agent.velocity.magnitude > 0.1f;
            HandleFootsteps();

            if (animator)
            {
                animator.SetBool(WalkHash, isMoving);
                animator.SetBool(IdleHash, !isMoving);
                animator.SetBool(RunHash, false);
            }
        }

        if (isStunned) HandleStun();
        else if (isOnCooldown) HandleCooldown();
    }

    private void Patrol()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (RandomPoint(spawnPoint, patrolRadius, out Vector3 point))
                agent.SetDestination(point);
        }
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }

    private void CheckForIncense()
    {
        if (!playerController || !weaponParent || hasReactedToIncense) return;

        foreach (Transform child in weaponParent)
        {
            if (child.CompareTag("Incense") &&
                Vector3.Distance(transform.position, playerController.transform.position) <= detectionRadius)
            {
                hasReactedToIncense = true;
                isAggressive = true;
                SetVisibility(true);
                StartCoroutine(PrepareAttackRoutine());
                break;
            }
        }
    }

    private bool isPreparingAttack = false;

    private IEnumerator PrepareAttackRoutine()
    {
        isPreparingAttack = true;
        isAggressive = true;

        // Позиция игрока
        Vector3 playerPos = playerController.transform.position;
        Vector3 teleportPos = GetRandomAttackPosition();

        if (!NavMesh.SamplePosition(teleportPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Не удалось найти позицию рядом с игроком");
            isPreparingAttack = false;
            yield break;
        }

        // Телепортируем призрака
        agent.Warp(hit.position);
        SetVisibility(true);
        yield return new WaitForSeconds(0.2f);

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = attackSpeed;

            // Включаем бег
            if (animator)
            {
                animator.SetBool(RunHash, true);
                animator.SetBool(WalkHash, false);
                animator.SetBool(IdleHash, false);
            }

            float chaseTime = 0f;
            float maxChaseTime = 5f; // Максимальное время преследования

            // Гонимся за игроком, пока не окажемся в радиусе атаки или не истечет время
            while (Vector3.Distance(transform.position, playerController.transform.position) > attackRange && chaseTime < maxChaseTime)
            {
                Vector3 directionToPlayer = (playerController.transform.position - transform.position).normalized;
                Vector3 targetPosition = playerController.transform.position - directionToPlayer * attackRange;
                agent.SetDestination(targetPosition);

                isMoving = true;
                HandleFootsteps();

                chaseTime += Time.deltaTime;
                yield return null;
            }

            agent.ResetPath();
            agent.isStopped = true;

            // Если время истекло, считаем это уворотом
            if (chaseTime >= maxChaseTime)
            {
                isStunned = true;
                currentStunTime = stunDuration;
                isPreparingAttack = false;
                isMoving = false;

                // Принудительно включаем Idle анимацию
                if (animator)
                {
                    animator.Play("Idle");
                }

                // Ждем время оглушения
                yield return new WaitForSeconds(stunDuration);
                
                isStunned = false;
                
                // После оглушения продолжаем атаковать
                if (hasReactedToIncense)
                {
                    StartCoroutine(PrepareAttackRoutine());
                }
                yield break;
            }
        }

        isPreparingAttack = false;
        StartCoroutine(AttackRoutine());
    }



    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator)
        {
            animator.SetBool(RunHash, false);
            animator.SetTrigger(AttackHash);
        }

        if (attackSound)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(attackSound);
        }

        yield return new WaitForSeconds(1.2f);

        float distToPlayer = Vector3.Distance(transform.position, playerController.transform.position);

        if (distToPlayer <= attackRange)
        {
            // Успешная атака
            playerController.TakeDamage(1);
            yield return StartCoroutine(StunPlayer());
            
            // Скрываем призрака на 20 секунд
            SetVisibility(false);
            yield return new WaitForSeconds(20f);
            
            // Повторяем атаку
            if (hasReactedToIncense)
            {
                StartCoroutine(PrepareAttackRoutine());
            }
        }
        else
        {
            // Неуспешная атака
            isStunned = true;
            currentStunTime = stunDuration;
            
            if (animator)
            {
                animator.SetBool(IdleHash, true);
                animator.SetBool(WalkHash, false);
                animator.SetBool(RunHash, false);
            }
        }

        isAttacking = false;
        isMoving = false;
    }

    private Vector3 GetRandomAttackPosition()
    {
        Vector3 playerPos = playerController.transform.position;

        for (int i = 0; i < 10; i++)
        {
            Vector3 offsetDir = Random.onUnitSphere;
            offsetDir.y = 0;
            float distance = Random.Range(minAttackDistance, maxAttackDistance);
            Vector3 candidate = playerPos + offsetDir.normalized * distance;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }

        return playerPos - playerController.transform.forward * maxAttackDistance;
    }

    private IEnumerator StunPlayer()
    {
        playerController.enabled = false;
        yield return new WaitForSeconds(playerStunDuration);
        playerController.enabled = true;
    }

    private void HandleStun()
    {
        currentStunTime -= Time.deltaTime;
        if (currentStunTime <= 0)
        {
            isStunned = false;
            isOnCooldown = true;
            currentCooldownTime = cooldownDuration;

            if (!hasReactedToIncense)
                SetVisibility(false);
        }
    }

    private void HandleCooldown()
    {
        currentCooldownTime -= Time.deltaTime;
        if (currentCooldownTime <= 0)
        {
            isOnCooldown = false;
            transform.position = spawnPoint + Random.insideUnitSphere * patrolRadius;
        }
    }

    private void SetVisibility(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = visible;
        }
        isVisible = visible;
    }

    private void HandleFootsteps()
    {
        if (isMoving && agent.isOnNavMesh && agent.velocity.magnitude > 0.1f &&
            Time.time - lastFootstepTime >= footstepInterval && footstepSounds.Length > 0)
        {
            if (!audioSource.isPlaying)
            {
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(clip, 1f);
                lastFootstepTime = Time.time;
            }
        }
    }

    public void OnCaught()
    {
        isActive = false;
        SetVisibility(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (spawnPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in spawnPoints)
                if (point) Gizmos.DrawSphere(point.position, 0.5f);
        }
    }
}
