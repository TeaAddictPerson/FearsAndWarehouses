using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Poltergeist : MonoBehaviour
{
    [Header("Настройки полтергейста")]
    public float moveSpeed = 3f;
    public float attackRange = 1.2f;
    public float detectionAngle = 120f;
    public float detectionDistance = 10f;
    public AudioClip[] footstepSounds;
    public AudioClip cryingSound;
    public AudioClip pointingSound;
    public AudioSource audioSource;
    public Image bloodOverlay;
    public float bloodFadeDuration = 1f;
    public float bloodDisplayDuration = 5f;
    public float attackRotationSpeed = 10f;
    public float cryingVolume = 0.7f;
    public float cryingMinDistance = 5f;
    public float cryingMaxDistance = 30f;
    public bool debugMode = true;
    public Camera playerCamera;

    [Header("Настройки изгнания")]
    public float exorcismDuration = 3f;

    [Header("Точки появления")]
    public Transform[] spawnPoints;
    public float initialSpawnDelay = 15f;

    [Header("Анимации")]
    private Animator animator;
    private readonly int IdleHash = Animator.StringToHash("Idle");
    private readonly int RunHash = Animator.StringToHash("Run");
    private readonly int CryingHash = Animator.StringToHash("Crying");
    private readonly int PointingHash = Animator.StringToHash("Pointing");
    private readonly int PunchHash = Animator.StringToHash("Punch");

    private Vector3 currentSpawnPoint;
    private bool isVisible, isAttacking, isMoving, isActive, isInitialized;
    private bool hasBeenSpotted = false;
    private float lastFootstepTime;
    private Vector3 attackTarget;
    private AudioSource cryingAudioSource;
    private float targetHeight;

    private FirstPersonController playerController;
    private Transform playerTransform;

    private bool isBeingExorcised = false;
    private Vector3 deathPosition;

    private bool isPointing = false;

    private void Start()
    {
        Debug.Log("Poltergeist: Start called");
        Initialize();
        StartCoroutine(InitialSpawn());
    }

    private void Update()
    {
        if (!isActive)
        {
            if (debugMode) Debug.Log("Poltergeist: Not active");
            return;
        }

        if (!isInitialized)
        {
            if (debugMode) Debug.Log("Poltergeist: Not initialized");
            return;
        }

        if (!hasBeenSpotted)
        {
            if (debugMode) Debug.Log("Poltergeist: Checking if spotted");
            CheckIfSpotted();
        }
        else if (!isAttacking && !isPointing)
        {
            if (debugMode) Debug.Log("Poltergeist: Handling movement");
            HandleMovement();
        }
    }

    private void Initialize()
    {
        Debug.Log("Poltergeist: Initialize called");
        if (isInitialized)
        {
            Debug.Log("Poltergeist: Already initialized");
            return;
        }

        // Добавляем Rigidbody на дочерний объект для правильной физики
        Transform childTransform = transform.GetChild(0); // Получаем первый дочерний объект
        if (childTransform != null)
        {
            // Настраиваем Rigidbody
            Rigidbody rb = childTransform.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = childTransform.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.isKinematic = true; // Делаем кинематическим, чтобы предотвратить физические взаимодействия
                rb.constraints = RigidbodyConstraints.FreezeAll; // Замораживаем все движения
                rb.mass = 1f;
                rb.linearDamping = 1f;
                if (debugMode) Debug.Log("Poltergeist: Added Rigidbody to child object");
            }

            // Фиксируем коллайдер
            Collider childCollider = childTransform.GetComponent<Collider>();
            if (childCollider != null)
            {
                childCollider.isTrigger = true; // Делаем триггером, чтобы избежать физических столкновений
            }
        }
        else
        {
            Debug.LogError("Poltergeist: No child objects found!");
        }

        // Ищем игрока и его компоненты
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log("Poltergeist: Player found");
            playerController = player.GetComponent<FirstPersonController>();
            playerTransform = player.transform;
            
            // Ищем камеру в FirstPersonController
            if (playerController != null)
            {
                playerCamera = playerController.GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    Debug.LogError("Poltergeist: Camera not found in FirstPersonController!");
                    return;
                }
                Debug.Log("Poltergeist: Found player camera in FirstPersonController");
            }
            else
            {
                Debug.LogError("Poltergeist: FirstPersonController not found on player!");
                return;
            }
        }
        else
        {
            Debug.LogError("Poltergeist: Player not found!");
            return;
        }

        // Основной аудиосоурс для шагов и других звуков
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        // Отдельный аудиосоурс для плача
        cryingAudioSource = gameObject.AddComponent<AudioSource>();
        cryingAudioSource.spatialBlend = 1f;
        cryingAudioSource.minDistance = cryingMinDistance;
        cryingAudioSource.maxDistance = cryingMaxDistance;
        cryingAudioSource.rolloffMode = AudioRolloffMode.Linear;
        cryingAudioSource.volume = cryingVolume;
        cryingAudioSource.loop = true;

        SetVisibility(false);

        animator = GetComponent<Animator>();
        if (!animator) Debug.LogError("Poltergeist: Animator not found!");

        isInitialized = true;
        Debug.Log("Poltergeist: Initialization complete");
    }

    private void CheckIfSpotted()
    {
        if (!isInitialized)
        {
            if (debugMode) Debug.LogWarning("Poltergeist: Not initialized!");
            return;
        }

        if (playerCamera == null)
        {
            if (debugMode) Debug.LogError("Poltergeist: Camera is null!");
            return;
        }

        // Получаем направление от камеры к призраку
        Vector3 directionToGhost = transform.position - playerCamera.transform.position;
        float distanceToGhost = directionToGhost.magnitude;
        
        // Вычисляем угол между направлением взгляда камеры и направлением к призраку
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToGhost);

        // Рисуем лучи всегда, не только в debug режиме
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 10f, Color.blue);
        Debug.DrawRay(playerCamera.transform.position, directionToGhost.normalized * 10f, Color.yellow);

        if (debugMode)
        {
            Debug.Log($"Poltergeist: Camera position: {playerCamera.transform.position}");
            Debug.Log($"Poltergeist: Ghost position: {transform.position}");
            Debug.Log($"Poltergeist: Angle: {angle}, Distance: {distanceToGhost}");
        }

        // Проверяем, находится ли призрак в поле зрения игрока
        if (angle <= detectionAngle / 2f && distanceToGhost <= detectionDistance)
        {
            if (debugMode) Debug.Log("Poltergeist: In field of view!");
            
            // Проверяем, нет ли препятствий между игроком и призраком
            RaycastHit hit;
            if (!Physics.Raycast(playerCamera.transform.position, directionToGhost, out hit, distanceToGhost) || 
                hit.collider.transform.IsChildOf(transform))
            {
                if (debugMode) Debug.Log("Poltergeist: Spotted by player!");
                OnSpotted();
            }
        }
    }

    private void OnSpotted()
    {
        if (hasBeenSpotted)
        {
            Debug.Log("Poltergeist: Уже был замечен ранее");
            return;
        }

        Debug.Log("Poltergeist: Впервые замечен игроком");
        hasBeenSpotted = true;
        isPointing = true;
        
        // Поворачиваемся к игроку
        if (playerTransform)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
            Debug.Log("Poltergeist: Повернулся к игроку");
        }
        
        // Останавливаем звук плача
        if (cryingAudioSource)
        {
            cryingAudioSource.Stop();
            Debug.Log("Poltergeist: Остановлен звук плача");
        }

        // Проигрываем анимацию указывания и звук
        if (animator)
        {
            animator.SetBool(CryingHash, false);
            animator.SetBool(IdleHash, false);
            animator.SetBool(RunHash, false);
            animator.SetTrigger(PointingHash);
            Debug.Log("Poltergeist: Запущена анимация указывания");
        }

        if (pointingSound && audioSource)
        {
            audioSource.PlayOneShot(pointingSound);
            Debug.Log("Poltergeist: Проигран звук указывания");
        }

        // Запускаем корутину для ожидания окончания анимации указывания
        StartCoroutine(WaitForPointingAnimation());
    }

    private IEnumerator WaitForPointingAnimation()
    {
        Debug.Log("Poltergeist: Ожидание окончания анимации указывания");
        // Ждем окончания анимации указывания (примерно 2 секунды)
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Poltergeist: Анимация указывания завершена, начинаем преследование");
        isPointing = false;
        isMoving = true;
    }

    private void HandleMovement()
    {
        if (!playerCamera || !playerTransform)
        {
            if (debugMode) Debug.LogError("Poltergeist: Missing player references!");
            return;
        }

        // Получаем направление от камеры к призраку
        Vector3 directionToGhost = transform.position - playerCamera.transform.position;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToGhost);

        // Рисуем лучи для отладки
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 10f, Color.blue);
        Debug.DrawRay(playerCamera.transform.position, directionToGhost.normalized * 10f, Color.yellow);

        if (angle <= detectionAngle / 2f)
        {
            // Игрок смотрит на призрака - стоим на месте
            if (animator)
            {
                animator.SetBool(RunHash, false);
                animator.SetBool(IdleHash, true);
            }
            isMoving = false;
            
            // Останавливаем звук шагов
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            if (debugMode) Debug.Log("Poltergeist: Player is looking at ghost - staying still");
        }
        else
        {
            // Игрок не смотрит - двигаемся к нему
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            directionToPlayer.y = 0; // Игнорируем вертикальную составляющую при повороте
            
            // Поворачиваемся к игроку
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            
            // Двигаемся вперед, сохраняя целевую высоту
            Vector3 movement = transform.forward * moveSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + movement;
            newPosition.y = targetHeight; // Фиксируем высоту
            transform.position = newPosition;

            if (debugMode)
            {
                Debug.Log($"Poltergeist: Moving towards player. Distance: {Vector3.Distance(transform.position, playerTransform.position)}");
                Debug.Log($"Poltergeist: Movement vector: {movement}");
            }

            if (animator)
            {
                animator.SetBool(IdleHash, false);
                animator.SetBool(RunHash, true);
            }

            isMoving = true;
            HandleFootsteps();

            // Проверяем расстояние до игрока для атаки
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                if (debugMode) Debug.Log($"Poltergeist: In attack range! Distance: {distanceToPlayer}");
                StartCoroutine(AttackRoutine());
            }
        }
    }

    private void HandleFootsteps()
    {
        if (isMoving && Time.time - lastFootstepTime >= 0.8f && footstepSounds.Length > 0)
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

    private void SetVisibility(bool visible)
    {
        // Реализация логики изменения видимости призрака
    }

    private IEnumerator InitialSpawn()
    {
        Debug.Log("Poltergeist: Waiting for initial spawn delay");
        yield return new WaitForSeconds(initialSpawnDelay);
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Poltergeist: No spawn points assigned!");
            yield break;
        }

        Debug.Log("Poltergeist: Spawning at random point");
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        SpawnAtPoint(point);
    }

    private void SpawnAtPoint(Transform point)
    {
        Debug.Log($"Poltergeist: Spawning at point {point.name}");
        transform.position = point.position;
        currentSpawnPoint = point.position;
        targetHeight = point.position.y;
        isActive = true;
        isVisible = true;
        isAttacking = false;
        isMoving = false;
        hasBeenSpotted = false;
        isPointing = false;
        SetVisibility(true);

        if (animator)
        {
            animator.SetBool(CryingHash, true);
            animator.SetBool(IdleHash, false);
            animator.SetBool(RunHash, false);
            Debug.Log("Poltergeist: Started crying animation");
        }

        // Запускаем звук плача
        if (cryingSound && cryingAudioSource)
        {
            cryingAudioSource.clip = cryingSound;
            cryingAudioSource.Play();
            Debug.Log("Poltergeist: Started crying sound");
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        Debug.Log("Poltergeist: Starting attack");

        // Поворачиваем игрока к призраку
        Vector3 directionToGhost = transform.position - playerTransform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToGhost);
        
        float rotationTime = 0f;
        float rotationDuration = 0.5f;
        
        while (rotationTime < rotationDuration)
        {
            rotationTime += Time.deltaTime;
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                targetRotation,
                rotationTime / rotationDuration
            );
            yield return null;
        }

        // Проигрываем анимацию атаки
        if (animator)
        {
            animator.SetTrigger(PunchHash);
        }

        yield return new WaitForSeconds(0.5f);

        // Наносим урон
        playerController.TakeDamage(1);
        StartCoroutine(ShowBloodEffect());

        yield return new WaitForSeconds(1f);

        // Выбираем новую точку появления
        if (spawnPoints.Length > 0)
        {
            Transform newPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnAtPoint(newPoint);
        }

        isAttacking = false;
    }

    private IEnumerator ShowBloodEffect()
    {
        if (bloodOverlay == null) yield break;

        // Плавное появление
        float elapsedTime = 0f;
        Color startColor = bloodOverlay.color;
        startColor.a = 0f;
        Color targetColor = bloodOverlay.color;
        targetColor.a = 0.4f; // Устанавливаем максимальную прозрачность на 0.4 (101/255)

        bloodOverlay.gameObject.SetActive(true);
        bloodOverlay.color = startColor;

        while (elapsedTime < bloodFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.4f, elapsedTime / bloodFadeDuration); // Плавно увеличиваем до 0.4
            bloodOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }

        bloodOverlay.color = targetColor;

        // Ждем время отображения
        yield return new WaitForSeconds(bloodDisplayDuration);

        // Плавное исчезновение
        elapsedTime = 0f;
        while (elapsedTime < bloodFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0.4f, 0f, elapsedTime / bloodFadeDuration); // Плавно уменьшаем от 0.4
            bloodOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }

        bloodOverlay.color = startColor;
        bloodOverlay.gameObject.SetActive(false);
    }

    public void StartExorcism()
    {
        if (isBeingExorcised)
        {
            Debug.Log("Poltergeist: Изгнание уже начато");
            return;
        }
        
        Debug.Log("Poltergeist: Начато изгнание");
        isBeingExorcised = true;
        isActive = false;
        deathPosition = transform.position;
        Debug.Log($"Poltergeist: Позиция смерти установлена: {deathPosition}");
        
        // Останавливаем все звуки
        if (audioSource)
        {
            audioSource.Stop();
            Debug.Log("Poltergeist: Остановлен основной звук");
        }
        if (cryingAudioSource)
        {
            cryingAudioSource.Stop();
            Debug.Log("Poltergeist: Остановлен звук плача");
        }
        
        // Останавливаем анимации
        if (animator)
        {
            animator.SetBool(CryingHash, false);
            animator.SetBool(IdleHash, false);
            animator.SetBool(RunHash, false);
            Debug.Log("Poltergeist: Остановлены все анимации");
        }
        
        StartCoroutine(ExorcismRoutine());
    }

    private IEnumerator ExorcismRoutine()
    {
        Debug.Log($"Poltergeist: Ожидание изгнания ({exorcismDuration} секунд)");
        yield return new WaitForSeconds(exorcismDuration);
        
        // Ищем объект с тегом Dust и проигрываем его частицы
        GameObject dust = GameObject.FindGameObjectWithTag("Dust");
        if (dust != null)
        {
            Vector3 dustPosition = deathPosition;
            dustPosition.y += 2f; // Поднимаем частицы на 2 единицы выше
            Debug.Log($"Poltergeist: Найден объект Dust, перемещаем на позицию {dustPosition}");
            dust.transform.position = dustPosition;

            // Проигрываем систему частиц
            ParticleSystem particleSystem = dust.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                Debug.Log("Poltergeist: Проигрываем систему частиц");
                particleSystem.Clear(); // Очищаем предыдущие частицы
                particleSystem.Play(); // Запускаем проигрывание
            }
            else
            {
                Debug.LogError("Poltergeist: На объекте Dust не найдена система частиц!");
            }
        }
        else
        {
            Debug.LogError("Poltergeist: Объект с тегом Dust не найден в сцене!");
        }
        
        Debug.Log("Poltergeist: Деактивация призрака");
        gameObject.SetActive(false);
    }
} 