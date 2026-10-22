using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSkip : MonoBehaviour
{
    private void Update()
    {
        if (InputManager.NextLevelWasPressed)
        {
            LoadNextScene();
        }

        if (InputManager.PreviousLevelWasPressed)
        {
            LoadPreviousScene();
        }
    }

    private void LoadNextScene()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;

        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    private void LoadPreviousScene()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int previousScene = currentScene - 1;

        if (previousScene >= 0)
        {
            SceneManager.LoadScene(previousScene);
        }
    }
}