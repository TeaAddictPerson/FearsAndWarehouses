using UnityEngine;

public class ShowCursor : MonoBehaviour
{
    void Start()
    {

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
