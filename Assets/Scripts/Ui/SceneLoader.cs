using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SceneConfig
{
    public string dropDownOptionName;
    public string sceneName;
    public GameObject[] backRowPrefabs;
}

public class SceneLoader : MonoBehaviour
{
    public SceneConfig[] sceneConfigs;

    //Called by the main menu so it knows which scene to go to
    public void SetupNewScene()
    {
        string sceneName = PlayerPrefs.GetString("gameMode");
        SceneConfig selectedConfig = Array.Find(sceneConfigs, config => config.dropDownOptionName == sceneName);
        LoadWithConfig(selectedConfig);
    }

    public void LoadWithConfig(SceneConfig config)
    {
        // Every non-correspondence entry point (main menu buttons, SetupNewScene) comes
        // through here, so clear the flag here by default. CorrespondenceMenu sets
        // "correspondenceMode" to 1 itself right before calling LoadWithConfig(config, true) --
        // without this, a stale "1" left over from a past correspondence game would leak into
        // a later local/AI match and make TurnProgresser skip AiTurn() and try to push moves
        // with no game doc loaded.
        LoadWithConfig(config, isCorrespondence: false);
    }

    public void LoadWithConfig(SceneConfig config, bool isCorrespondence)
    {
        if (!isCorrespondence) PlayerPrefs.SetInt("correspondenceMode", 0);

        PlayerPrefs.SetInt("backRowCount", config.backRowPrefabs.Length);
        for (int i = 0; i < config.backRowPrefabs.Length; i++)
            PlayerPrefs.SetString($"backRowPrefab_{i}", config.backRowPrefabs[i].name);
        LoadScene(config.sceneName);
    }

    //Called by the main menu button to be hardcoded to one scene.  Also called by SetupNewScene to load the selected scene
    public void LoadScene(string sceneName = "Main Menu")
    {
        SceneManager.LoadScene(sceneName);
    }
}
