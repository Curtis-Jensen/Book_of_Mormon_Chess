using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class SceneConfig
{
    public string dropDownOptionName;
    public string sceneName;
    public GameObject[] backRowPrefabs;
}

public class SceneLoader : MonoBehaviour
{
    public TextMeshProUGUI sizeInput;
    public TMP_Dropdown modeDropdown;
    public SceneConfig[] sceneConfigs;

    //Called by the main menu so it knows which scene to go to
    public void SetupNewScene()
    {
        PlayerPrefs.SetInt("boardSize", GetInputText(sizeInput));

        var sceneName = modeDropdown.options[modeDropdown.value].text;

        LoadScene(sceneName);
    }

    #region Private Methods
    int GetInputText(TextMeshProUGUI input)
    {
        //Clean for spaces I think
        var cleanedText = input.text.Remove(input.text.Length - 1, 1);

        return int.Parse(cleanedText);
    }

    //Called by the main menu button to be hardcoded to one scene.  Also called by SetupNewScene to load the selected scene
    public void LoadScene(string sceneName = "Main Menu")
    {
        SceneManager.LoadScene(sceneName);
    }
    #endregion
}
