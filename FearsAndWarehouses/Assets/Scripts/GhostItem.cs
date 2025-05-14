using UnityEngine;

public class GhostItem : MonoBehaviour
{
    [Header("Основные настройки")]
    public float useRange = 2f; // Дальность использования предмета
    public float moveSpeed = 2f; // Скорость выдвижения предмета
    public float maxMoveDistance = 0.5f; // Максимальное расстояние выдвижения
    protected bool shouldMoveForward = false; // Флаг для определения, должен ли предмет отдаляться

    protected bool isUsing = false;
    protected bool isReturning = false;
    protected Vector3 originalPosition;
    protected Vector3 targetPosition;
    protected Transform itemTransform;

    protected virtual void Start()
    {
        // Проверяем, что это не базовый класс
        if (this.GetType() == typeof(GhostItem))
        {
            Debug.LogError("Нельзя использовать базовый класс GhostItem напрямую! Используйте один из дочерних классов: SilverMirror, Cross, HolyWater или Incense.");
            Destroy(this);
            return;
        }

        // Инициализируем только если это дочерний класс
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

        if (isReturning && shouldMoveForward && itemTransform != null)
        {
            ReturnToOriginalPosition();
        }
    }

    protected virtual void StartUsing()
    {
        isUsing = true;
        isReturning = false;
    }

    protected virtual void ContinueUsing()
    {
        if (shouldMoveForward && itemTransform != null)
        {
            itemTransform.localPosition = Vector3.Lerp(itemTransform.localPosition, targetPosition, Time.deltaTime * moveSpeed);
        }
    }

    protected virtual void StopUsing()
    {
        isUsing = false;
        if (shouldMoveForward && itemTransform != null)
        {
            isReturning = true;
        }
    }

    protected virtual void ReturnToOriginalPosition()
    {
        if (itemTransform.localPosition != originalPosition)
        {
            itemTransform.localPosition = Vector3.Lerp(itemTransform.localPosition, originalPosition, Time.deltaTime * moveSpeed);

            // Если предмет достаточно близко к исходной позиции, останавливаем возврат
            if (Vector3.Distance(itemTransform.localPosition, originalPosition) < 0.001f)
            {
                itemTransform.localPosition = originalPosition;
                isReturning = false;
            }
        }
        else
        {
            isReturning = false;
        }
    }

    protected bool IsInRange(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= useRange;
    }
}