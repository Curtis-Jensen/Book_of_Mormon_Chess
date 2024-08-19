using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public TextMeshProUGUI inputText;

    [HideInInspector]
    public int boardSize;

    public void LoadResizableScene(string sceneName)
    {
        boardSize = GetBoardSize();

        DontDestroyOnLoad(gameObject);
        LoadScene(sceneName);
        //DestroyOtherSceneLoaders();
    }

    int GetBoardSize()
    {
        var cleanedText = inputText.text.Remove(inputText.text.Length - 1, 1);

        return int.Parse(cleanedText);
    }


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// For some reason this still destroys itself, so it is not being called
    /// </summary>
    void DestroyOtherSceneLoaders()
    {
        var sceneLoaders = FindObjectsByType(typeof(SceneLoader), FindObjectsSortMode.InstanceID);

        foreach (var sceneLoader in sceneLoaders)
        {
            if(sceneLoader != gameObject)
            {
                Destroy(sceneLoader);
            }
        }
    }
}
