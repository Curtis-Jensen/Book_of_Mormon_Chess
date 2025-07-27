using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public TextMeshProUGUI sizeInput;
    public Toggle ai;
    public string sceneName;

    public void SetupNewScene()
    {
        PlayerPrefs.SetInt("boardSize", GetInputText(sizeInput));
        PlayerPrefs.SetInt("isAi", ai.isOn ? 1 : 0);

        LoadScene();
    }

    #region Private Methods
    int GetInputText(TextMeshProUGUI input)
    {
        //Clean for spaces I think
        var cleanedText = input.text.Remove(input.text.Length - 1, 1);

        return int.Parse(cleanedText);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
    #endregion
}
