using UnityEngine;

public class EquipWeapon : MonoBehaviour
{
    public GameObject item;
    public Transform WeaponParent;

    private Rigidbody itemRb;
    private MeshCollider itemCollider;
    private static bool handOccupied = false;
    private bool isEquipped = false;

    void Start()
    {
        if (item != null)
        {
            itemRb = item.GetComponent<Rigidbody>();
            itemCollider = item.GetComponent<MeshCollider>();


            itemRb.isKinematic = false;
            itemCollider.enabled = true;
            item.transform.SetParent(null);
        }
    }

    void Update()
    {
        if (isEquipped && Input.GetKeyDown(KeyCode.F))
        {
            Drop();
        }
    }

    void Drop()
    {
        if (item != null)
        {
            isEquipped = false;
            handOccupied = false;

            WeaponParent.DetachChildren();
            item.transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
            itemRb.isKinematic = false;
            itemCollider.enabled = true;
        }
    }

    void Equip()
    {
        if (item != null && !handOccupied)
        {
            isEquipped = true;
            handOccupied = true;

            itemRb.isKinematic = true;
            itemCollider.enabled = false;

            item.transform.position = WeaponParent.position;
            item.transform.rotation = WeaponParent.rotation;
            item.transform.SetParent(WeaponParent);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isEquipped && !handOccupied && other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Equip();
            }
        }
    }
    
    public bool IsEquipped => isEquipped;
}
