using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    public TextMeshProUGUI boardSizeText;

    int boardSize;

    public void LoadResizableScene()
    {
        Debug.Log(boardSizeText.text);
        boardSize = int.Parse(boardSizeText.text);
        Debug.Log(boardSize + boardSize);
        LoadScene();
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
