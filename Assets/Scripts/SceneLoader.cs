using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 🎯 Public variable to set the scene name in the Inspector
    public string sceneName;

    // 🎮 Method to be called when the button is pressed
    public void LoadScene()
    {
        // 🔄 Load the scene with the specified name
        SceneManager.LoadScene(sceneName);
    }
}
