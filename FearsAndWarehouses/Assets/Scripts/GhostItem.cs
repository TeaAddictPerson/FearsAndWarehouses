using UnityEngine;

public abstract class GhostItem : MonoBehaviour
{
    [Header("Основные настройки")]
    public float useRange = 2f; // Дальность использования предмета
    public float moveSpeed = 2f; // Скорость выдвижения предмета
    public float maxMoveDistance = 0.5f; // Максимальное расстояние выдвижения

    protected bool isUsing = false;
    protected Vector3 originalPosition;
    protected Vector3 targetPosition;
    protected Transform itemTransform;

    protected virtual void Start()
    {
        itemTransform = transform;
        originalPosition = itemTransform.localPosition;
        targetPosition = originalPosition + Vector3.forward * maxMoveDistance;
    }

    protected virtual void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            if (!isUsing)
            {
                StartUsing();
            }
            ContinueUsing();
        }
        else if (isUsing)
        {
            StopUsing();
        }
    }

    protected virtual void StartUsing()
    {
        isUsing = true;
    }

    protected virtual void ContinueUsing()
    {
        // Плавное движение предмета вперед
        itemTransform.localPosition = Vector3.Lerp(
            itemTransform.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    protected virtual void StopUsing()
    {
        isUsing = false;
        // Возвращаем предмет в исходное положение
        itemTransform.localPosition = Vector3.Lerp(
            itemTransform.localPosition,
            originalPosition,
            moveSpeed * Time.deltaTime
        );
    }

    protected bool IsInRange(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= useRange;
    }
} 