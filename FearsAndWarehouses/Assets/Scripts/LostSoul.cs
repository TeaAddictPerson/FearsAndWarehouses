using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class LostSoul : MonoBehaviour
{
    [Header("Настройки заблудшей души")]
    public float moveSpeed = 3f;
    public float patrolRadius = 10f;
    public float attackRange = 2f;
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
    [SerializeField] private float footstepInterval = 0.5f;

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

        if (isAggressive && !isAttacking && !isStunned && !isOnCooldown)
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
        if (!playerController || !weaponParent) return;

        foreach (Transform child in weaponParent)
        {
            if (child.CompareTag("Incense") &&
                Vector3.Distance(transform.position, playerController.transform.position) <= detectionRadius)
            {
                hasReactedToIncense = true;
                isAggressive = true;
                SetVisibility(true);
                agent.enabled = false;
                break;
            }
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(0.5f);

        Vector3 attackPos = GetRandomAttackPosition();
        transform.position = attackPos;
        SetVisibility(true); // Призрак становится видим при атаке

        Vector3 dirToPlayer = (playerController.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dirToPlayer);

        if (animator)
        {
            animator.SetBool(WalkHash, false);
            animator.SetBool(IdleHash, false);
            animator.SetBool(RunHash, true);
            animator.SetTrigger(AttackHash);
        }

        if (attackSound != null) audioSource.PlayOneShot(attackSound);

        attackTarget = playerController.transform.position;
        agent.enabled = true;
        agent.SetDestination(attackTarget);

        while (agent.remainingDistance > attackRange && !agent.pathPending)
        {
            yield return null;
        }

        agent.isStopped = true;
        agent.enabled = false;

        if (Vector3.Distance(transform.position, attackTarget) < attackRange)
        {
            playerController.TakeDamage(1);
            yield return StartCoroutine(StunPlayer());
        }
        else
        {
            isStunned = true;
            currentStunTime = stunDuration;
        }

        isAttacking = false;
        isOnCooldown = true;
        currentCooldownTime = cooldownDuration;
        SetVisibility(false);
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }

    private Vector3 GetRandomAttackPosition()
    {
        Vector3 playerPos = playerController.transform.position;
        Vector3 offset = Random.insideUnitSphere * Random.Range(minAttackDistance, maxAttackDistance);
        offset.y = 0;
        Vector3 desiredPos = playerPos + offset;

        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, maxAttackDistance, NavMesh.AllAreas))
            return hit.position;

        return playerPos + offset.normalized * minAttackDistance;
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
        if (isMoving && Time.time - lastFootstepTime >= footstepInterval && footstepSounds.Length > 0)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            audioSource.PlayOneShot(clip);
            lastFootstepTime = Time.time;
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
