using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    private void Update()
    {
        if (InputManager.RestartWasPressed)
        {
            SceneManager.LoadScene(0);
        }
    }
}