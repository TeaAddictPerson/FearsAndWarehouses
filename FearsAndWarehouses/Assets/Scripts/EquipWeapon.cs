using UnityEngine;

public class EquipWeapon : MonoBehaviour
{
    public GameObject item;
    public Transform WeaponParent;

    private Rigidbody itemRb;
    private MeshCollider itemCollider;

    void Start()
    {
        if (item != null)
        {
            itemRb = item.GetComponent<Rigidbody>();
            itemCollider = item.GetComponent<MeshCollider>();
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.F))
        {
            Drop();
        }
    }

    void Drop()
    {
        if (item != null)
        {
            WeaponParent.DetachChildren();
            item.transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f); // поправил логическую ошибку
            itemRb.isKinematic = false;
            itemCollider.enabled = true;
        }
    }

    void Equip()
    {
        if (item != null)
        {
            itemRb.isKinematic = true;
            itemCollider.enabled = false;

            item.transform.position = WeaponParent.position;
            item.transform.rotation = WeaponParent.rotation;

            item.transform.SetParent(WeaponParent);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                Equip();
            }
        }
    }
}
