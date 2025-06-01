using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public TextMeshProUGUI sizeInput;
    public Toggle ai;


    public void SetupNewScene(string sceneName)
    {
        PlayerPrefs.SetInt("boardSize", GetInputText(sizeInput));
        PlayerPrefs.SetInt("isAi", ai.isOn ? 1 : 0);

        LoadScene(sceneName);
    }

    #region Private Methods
    int GetInputText(TextMeshProUGUI input)
    {
        //Clean for spaces I think
        var cleanedText = input.text.Remove(input.text.Length - 1, 1);

        return int.Parse(cleanedText);
    }


    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    #endregion
}
