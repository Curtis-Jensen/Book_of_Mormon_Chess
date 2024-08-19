using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    public TextMeshProUGUI inputText;

    [HideInInspector]
    public int boardSize;

    public void LoadResizableScene()
    {
        boardSize = GetBoardSize();

        DontDestroyOnLoad(gameObject);
        LoadScene();
    }

    int GetBoardSize()
    {
        var cleanedText = inputText.text.Remove(1, 1);

        return int.Parse(cleanedText);
    }


    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
