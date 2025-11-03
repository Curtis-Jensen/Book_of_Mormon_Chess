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
        // Find matching config and save prefab configuration
        string sceneName = PlayerPrefs.GetString("gameMode");        
        SceneConfig selectedConfig = Array.Find(sceneConfigs, config => config.dropDownOptionName == sceneName);
        
        // Store number of prefabs
        PlayerPrefs.SetInt("backRowCount", selectedConfig.backRowPrefabs.Length);

        // Store prefab names in order
        for (int i = 0; i < selectedConfig.backRowPrefabs.Length; i++)
        {
            PlayerPrefs.SetString($"backRowPrefab_{i}", selectedConfig.backRowPrefabs[i].name);
        }

        LoadScene(selectedConfig.sceneName);
    }

    #region Private Methods
    //Called by the main menu button to be hardcoded to one scene.  Also called by SetupNewScene to load the selected scene
    public void LoadScene(string sceneName = "Main Menu")
    {
        SceneManager.LoadScene(sceneName);
    }
    #endregion
}
