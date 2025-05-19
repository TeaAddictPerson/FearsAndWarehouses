using UnityEngine;

public class Incense : MonoBehaviour
{
    public bool isEquipped = false;

    public void SetEquippedState(bool state)
    {
        isEquipped = state;
    }

    public bool IsEquipped => isEquipped; // ← Это нужно добавить
}
