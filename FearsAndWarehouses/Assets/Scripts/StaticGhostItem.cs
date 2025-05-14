using UnityEngine;

public class StaticGhostItem : MonoBehaviour
{
    [Header("Основные настройки")]
    public float useRange = 2f; // Дальность использования предмета

    protected bool isUsing = false;

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
    }

    protected virtual void StopUsing()
    {
        isUsing = false;
    }

    protected bool IsInRange(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= useRange;
    }
}