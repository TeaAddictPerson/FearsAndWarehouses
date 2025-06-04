using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    public int scene_number;

    public void Trans()
    {
        SceneManager.LoadScene(scene_number);
    }
}
