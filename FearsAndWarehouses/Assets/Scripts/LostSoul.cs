using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class LostSoul : MonoBehaviour
{
    [Header("Настройки заблудшей души")]
    public float moveSpeed = 3f;
    public float patrolRadius = 10f;
    public float footstepInterval = 1f;
    public float attackRange = 2f;
    public float attackSpeed = 10f;
    public float stunDuration = 15f;
    public float cooldownDuration = 20f;
    public float playerStunDuration = 3f;
    public float detectionRadius = 5f; // Радиус обнаружения благовоний
    public float minAttackDistance = 3f; // Минимальная дистанция для атаки
    public float maxAttackDistance = 8f; // Максимальная дистанция для атаки
    public AudioClip[] footstepSounds;
    public AudioClip attackSound;
    public AudioSource audioSource;

    [Header("Точки появления")]
    public Transform[] spawnPoints;
    public float initialSpawnDelay = 15f; // Задержка перед первым появлением

    [Header("Анимации")]
    private Animator animator;
    private readonly int IdleHash = Animator.StringToHash("Idle");
    private readonly int WalkHash = Animator.StringToHash("Walk");
    private readonly int AttackHash = Animator.StringToHash("ZombieAttack");

    private Vector3 spawnPoint;
    private bool isVisible = false;
    private bool isAttacking = false;
    private bool isStunned = false;
    private bool isOnCooldown = false;
    private float lastFootstepTime;
    private Vector3 attackTarget;
    private float currentCooldownTime;
    private float currentStunTime;
    private EquipWeapon playerEquipWeapon;
    private FirstPersonController playerController;
    private bool isActive = false;
    private bool isInitialized = false;
    private NavMeshAgent agent;
    private bool isMoving = false;
    private bool isAggressive = false;

    private void Start()
    {
        Initialize();
        StartCoroutine(InitialSpawn());
    }

    private void Initialize()
    {
        if (isInitialized) return;

        // Инициализация NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.1f;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Настраиваем аудио источник
        audioSource.spatialBlend = 1f; // 3D звук
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        
        SetVisibility(false);
        
        // Находим компоненты игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerEquipWeapon = player.GetComponent<EquipWeapon>();
            playerController = player.GetComponent<FirstPersonController>();
        }

        // Получаем компонент аниматора
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("LostSoul: Animator component not found!");
        }

        isInitialized = true;
    }

    private IEnumerator InitialSpawn()
    {
        // Ждем 15 секунд перед первым появлением
        yield return new WaitForSeconds(initialSpawnDelay);

        // Выбираем случайную точку появления
        if (spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnAtPoint(spawnPoint);
        }
    }

    private void SpawnAtPoint(Transform point)
    {
        transform.position = point.position;
        spawnPoint = point.position;
        isActive = true;
        SetVisibility(false);
        isVisible = false;
        isAttacking = false;
        isStunned = false;
        isOnCooldown = false;
        isMoving = false;

        // Запускаем анимацию простоя
        if (animator != null)
        {
            animator.SetBool(WalkHash, false);
            animator.SetBool(IdleHash, true);
        }

        // Устанавливаем начальную позицию для NavMeshAgent
        agent.Warp(point.position);

        Debug.Log($"LostSoul: Появился в точке {point.position}");
    }

    private void Update()
    {
        if (!isActive) return;

        if (isAggressive)
        {
            if (!isAttacking && !isStunned && !isOnCooldown)
            {
                CheckForIncense();
            }
        }
        else
        {
            CheckForIncense(); // Проверяем благовония постоянно
            if (!isVisible && !isAttacking && !isStunned && !isOnCooldown)
            {
                Patrol();
                isMoving = agent.velocity.magnitude > 0.1f;
                if (isMoving)
                {
                    PlayFootsteps();
                    if (animator != null)
                    {
                        animator.SetBool(WalkHash, true);
                        animator.SetBool(IdleHash, false);
                    }
                }
                else
                {
                    if (animator != null)
                    {
                        animator.SetBool(WalkHash, false);
                        animator.SetBool(IdleHash, true);
                    }
                }
            }
        }

        if (isStunned)
        {
            HandleStun();
            if (animator != null)
            {
                animator.SetBool(WalkHash, false);
                animator.SetBool(IdleHash, true);
            }
        }
        else if (isOnCooldown)
        {
            HandleCooldown();
            if (animator != null)
            {
                animator.SetBool(WalkHash, false);
                animator.SetBool(IdleHash, true);
            }
        }
    }

    private void Patrol()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(spawnPoint, patrolRadius, out point))
            {
                agent.SetDestination(point);
            }
        }
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private void PlayFootsteps()
    {
        if (Time.time - lastFootstepTime >= footstepInterval && !audioSource.isPlaying)
        {
            if (footstepSounds.Length > 0)
            {
                AudioClip randomFootstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.clip = randomFootstep;
                audioSource.pitch = 1f;
                audioSource.Play();
            }
            lastFootstepTime = Time.time;
        }
    }

    private void CheckForIncense()
    {
        Debug.Log("LostSoul: Начало проверки благовоний");
        
        if (playerEquipWeapon != null)
        {
            Debug.Log($"LostSoul: EquipWeapon найден, IsEquipped: {playerEquipWeapon.IsEquipped}");
            
            if (playerEquipWeapon.IsEquipped)
            {
                if (playerEquipWeapon.item != null)
                {
                    Debug.Log($"LostSoul: Предмет в руке: {playerEquipWeapon.item.name}");
                    
                    Incense incense = playerEquipWeapon.item.GetComponent<Incense>();
                    if (incense != null)
                    {
                        Debug.Log($"LostSoul: Компонент Incense найден, IsActive: {incense.IsEquipped}");
                        
                        float distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
                        Debug.Log($"LostSoul: Дистанция до игрока: {distanceToPlayer}, Радиус обнаружения: {detectionRadius}");
                        
                        if (distanceToPlayer <= detectionRadius)
                        {
                            if (incense.IsEquipped)
                            {
                                Debug.Log($"LostSoul: Обнаружены активные благовония на дистанции {distanceToPlayer}");
                                isAggressive = true;
                                agent.enabled = false; // Отключаем NavMeshAgent
                                StartCoroutine(AttackRoutine());
                            }
                            else
                            {
                                Debug.Log($"LostSoul: Благовония в руке, но не активны. Дистанция: {distanceToPlayer}");
                                isAggressive = false;
                                if (!isAttacking && !isStunned && !isOnCooldown)
                                {
                                    agent.enabled = true;
                                }
                            }
                        }
                        else
                        {
                            Debug.Log($"LostSoul: Игрок вне радиуса обнаружения");
                        }
                    }
                    else
                    {
                        Debug.Log("LostSoul: Предмет в руке не является благовониями");
                    }
                }
                else
                {
                    Debug.Log("LostSoul: Предмет в руке отсутствует");
                }
            }
            else
            {
                Debug.Log("LostSoul: Предмет не экипирован");
            }
        }
        else
        {
            Debug.Log("LostSoul: EquipWeapon не найден");
        }

        if (!isAggressive && !isAttacking && !isStunned && !isOnCooldown)
        {
            agent.enabled = true;
        }
    }

    private IEnumerator AttackRoutine()
    {
        while (isAggressive && !isStunned && !isOnCooldown)
        {
            // Проверяем, что благовония все еще активны
            if (playerEquipWeapon != null && playerEquipWeapon.IsEquipped && 
                playerEquipWeapon.item != null)
            {
                Incense incense = playerEquipWeapon.item.GetComponent<Incense>();
                if (incense != null && incense.IsEquipped)
                {
                    // Выбираем точку появления рядом с игроком
                    Vector3 attackPosition = GetRandomAttackPosition();
                    transform.position = attackPosition;
                    SetVisibility(true);
                    isVisible = true;

                    // Поворачиваемся к игроку
                    Vector3 directionToPlayer = (playerController.transform.position - transform.position).normalized;
                    transform.rotation = Quaternion.LookRotation(directionToPlayer);

                    // Запускаем атаку
                    yield return StartCoroutine(PerformAttack());

                    // После атаки
                    SetVisibility(false);
                    isVisible = false;
                    isAttacking = false;

                    // Ждем кулдаун
                    isOnCooldown = true;
                    currentCooldownTime = cooldownDuration;
                    yield return new WaitForSeconds(cooldownDuration);
                    isOnCooldown = false;
                }
                else
                {
                    isAggressive = false;
                    agent.enabled = true;
                    yield break;
                }
            }
            else
            {
                isAggressive = false;
                agent.enabled = true;
                yield break;
            }
        }
    }

    private Vector3 GetRandomAttackPosition()
    {
        Vector3 playerPos = playerController.transform.position;
        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(minAttackDistance, maxAttackDistance);
        randomDirection.y = 0;
        Vector3 targetPosition = playerPos + randomDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, maxAttackDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return playerPos + randomDirection.normalized * minAttackDistance;
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        Vector3 playerPosition = playerController.transform.position;
        attackTarget = playerPosition;
        
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        // Запускаем анимацию атаки
        if (animator != null)
        {
            animator.SetBool(WalkHash, false);
            animator.SetBool(IdleHash, false);
            animator.SetTrigger(AttackHash);
        }

        Vector3 startPosition = transform.position;
        float attackProgress = 0f;

        while (attackProgress < 1f)
        {
            attackProgress += Time.deltaTime * attackSpeed;
            transform.position = Vector3.Lerp(startPosition, attackTarget, attackProgress);
            yield return null;
        }

        // Проверяем, попал ли призрак в игрока
        if (Vector3.Distance(transform.position, playerPosition) < attackRange)
        {
            if (playerController != null)
            {
                playerController.TakeDamage(1);
                StartCoroutine(StunPlayer());
            }
        }

        isAttacking = false;
        isStunned = true;
        currentStunTime = stunDuration;
    }

    private IEnumerator StunPlayer()
    {
        if (playerController != null)
        {
            playerController.enabled = false; // Отключаем управление
            yield return new WaitForSeconds(playerStunDuration);
            playerController.enabled = true; // Включаем управление обратно
        }
    }

    private void HandleStun()
    {
        currentStunTime -= Time.deltaTime;
        if (currentStunTime <= 0)
        {
            isStunned = false;
            isOnCooldown = true;
            currentCooldownTime = cooldownDuration;
            SetVisibility(false);
            isVisible = false;
        }
    }

    private void HandleCooldown()
    {
        currentCooldownTime -= Time.deltaTime;
        if (currentCooldownTime <= 0)
        {
            isOnCooldown = false;
            // Выбираем новую точку появления
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection.y = 0;
            transform.position = spawnPoint + randomDirection;
        }
    }

    private void SetVisibility(bool visible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }

    public void OnCaught()
    {
        isActive = false;
        SetVisibility(false);
    }

    private void OnDrawGizmosSelected()
    {
        // Отрисовка радиуса патрулирования
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // Отрисовка радиуса обнаружения благовоний
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Отрисовка точек появления
        if (spawnPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.5f);
                }
            }
        }
    }
} 