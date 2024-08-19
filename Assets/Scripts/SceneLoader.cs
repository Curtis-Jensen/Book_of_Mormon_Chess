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
    }

    int GetBoardSize()
    {
        var cleanedText = inputText.text.Remove(1, 1);

        return int.Parse(cleanedText);
    }


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
