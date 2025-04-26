using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Data;
using System.IO;

public class Tablet : MonoBehaviour
{
    public GameObject item;
    public Transform WeaponParent;
    public GhostInfoManager ghostInfoManager;

    private Rigidbody itemRb;
    private BoxCollider itemCollider;
    private static bool handOccupied = false;
    private bool isEquipped = false;

    void Start()
    {
        if (item != null)
        {
            itemRb = item.GetComponent<Rigidbody>();
            itemCollider = item.GetComponent<BoxCollider>();
            itemRb.isKinematic = false;
            itemCollider.enabled = true;
            item.transform.SetParent(null);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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

            if (ghostInfoManager != null)
                ghostInfoManager.HideUI();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
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

            item.transform.Rotate(180f, 0f, 180f);

            if (ghostInfoManager != null)
                ghostInfoManager.ShowUI();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
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
}