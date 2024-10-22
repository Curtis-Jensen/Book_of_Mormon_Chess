using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public TextMeshProUGUI sizeInput;
    public TextMeshProUGUI colorInput;

    public void LoadResizableScene(string sceneName)
    {
        PlayerPrefs.SetInt("boardSize", GetBoardSize());

        LoadScene(sceneName);
    }

    int GetBoardSize()
    {
        var cleanedText = sizeInput.text.Remove(sizeInput.text.Length - 1, 1);

        return int.Parse(cleanedText);
    }


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
