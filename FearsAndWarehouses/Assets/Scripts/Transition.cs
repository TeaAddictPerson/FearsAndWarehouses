using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    public int scene_number;

    public void Trans()
    {
        Debug.Log(" нопка нажалась");
        SceneManager.LoadScene(scene_number);
    }
}
