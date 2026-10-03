using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SceneConfig
{
    public string dropDownOptionName;
    public string sceneName;
    public GameObject[] backRowPrefabs;

    // Optional: a different back row for the Lamanite (odd-index) players, e.g. the
    // Stripling Warriors chapter pits the player's Stripling Warriors against Lamanite Queens.
    // Leave empty and every side uses backRowPrefabs. Index 0 is the king slot, same as above.
    public GameObject[] opponentBackRowPrefabs;
}

public class SceneLoader : MonoBehaviour
{
    public SceneConfig[] sceneConfigs;

    void Start()
    {
        // Set by the victory screen's Next Chapter button
        string chapter = ChapterProgress.TakeAutoStart();
        if (chapter == null) return;

        SceneConfig config = Array.Find(sceneConfigs, c => c.dropDownOptionName == chapter);
        if (config != null) LoadWithConfig(config);
    }

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

        // Online games never count toward unlocking training chapters
        ChapterProgress.SetCurrentChapter(isCorrespondence ? null : config.dropDownOptionName);

        PlayerPrefs.SetInt("backRowCount", config.backRowPrefabs.Length);
        for (int i = 0; i < config.backRowPrefabs.Length; i++)
            PlayerPrefs.SetString($"backRowPrefab_{i}", config.backRowPrefabs[i].name);

        // Always write the count so a previous chapter's opponent row never leaks into this one
        int opponentCount = config.opponentBackRowPrefabs?.Length ?? 0;
        PlayerPrefs.SetInt("opponentBackRowCount", opponentCount);
        for (int i = 0; i < opponentCount; i++)
            PlayerPrefs.SetString($"opponentBackRowPrefab_{i}", config.opponentBackRowPrefabs[i].name);
        LoadScene(config.sceneName);
    }

    //Called by the main menu button to be hardcoded to one scene.  Also called by SetupNewScene to load the selected scene
    public void LoadScene(string sceneName = "Main Menu")
    {
        SceneManager.LoadScene(sceneName);
    }
}
