using UnityEngine;

public class StaticGhostItem : MonoBehaviour
{
    protected virtual void StartUsing() { }
    protected virtual void ContinueUsing() { }
    protected virtual void StopUsing() { }

protected virtual bool IsItemEquipped()
{
    var equipWeapon = GetComponentInParent<EquipWeapon>();
    return equipWeapon != null && equipWeapon.IsEquipped;
}



    void Update()
    {
        if (!IsItemEquipped())
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartUsing();
        }

        if (Input.GetKey(KeyCode.E))
        {
            ContinueUsing();
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            StopUsing();
        }
    }
}
