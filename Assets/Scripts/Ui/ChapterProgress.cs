using System;
using UnityEngine;

// Training chapters unlock in order: winning a chapter unlocks the next one.
// Names must match SceneConfig.dropDownOptionName on the main menu's SceneLoader.
// Anything not in this list (e.g. The Nephites' Last Stand) is always unlocked.
public static class ChapterProgress
{
    public static readonly string[] Order =
    {
        "Pawns", "Rooks", "Bishops", "Knights", "Queens", "Kings", "Stripling Warriors", "All Units"
    };

    const string CurrentChapterKey = "currentChapter";

    static string WonKey(string chapter) => $"chapterWon_{chapter}";

    public static bool IsWon(string chapter) => PlayerPrefs.GetInt(WonKey(chapter), 0) == 1;

    public static bool IsUnlocked(string chapter)
    {
        int index = Array.IndexOf(Order, chapter);
        return index <= 0 || IsWon(Order[index - 1]);
    }

    // Name of the chapter that unlocks this one, or null if it's already open
    public static string PrerequisiteOf(string chapter)
    {
        int index = Array.IndexOf(Order, chapter);
        return index <= 0 ? null : Order[index - 1];
    }

    // Called by SceneLoader so the Duel scene knows which chapter it's playing
    public static void SetCurrentChapter(string chapter) => PlayerPrefs.SetString(CurrentChapterKey, chapter ?? "");

    public static string CurrentChapter => PlayerPrefs.GetString(CurrentChapterKey, "");

    // Chapter after this one in the unlock order, or null at the end / outside the list
    public static string NextOf(string chapter)
    {
        int index = Array.IndexOf(Order, chapter);
        return index < 0 || index + 1 >= Order.Length ? null : Order[index + 1];
    }

    // Returns the chapter this win newly unlocked, or null if nothing new opened up
    public static string MarkCurrentChapterWon()
    {
        string chapter = CurrentChapter;
        if (Array.IndexOf(Order, chapter) < 0) return null;

        bool alreadyWon = IsWon(chapter);
        PlayerPrefs.SetInt(WonKey(chapter), 1);
        PlayerPrefs.Save();

        return alreadyWon ? null : NextOf(chapter);
    }

    static string SetupKey(string chapter) => $"chapterSetup_{chapter}";

    // The Duel scene has no chapter list, so the menu stores each chapter's setup
    // ("sceneName|Prefab,Prefab") for the victory screen's Next Chapter button
    public static void SaveSetups(SceneConfig[] configs)
    {
        foreach (var config in configs)
        {
            if (Array.IndexOf(Order, config.dropDownOptionName) < 0) continue;

            var prefabNames = Array.ConvertAll(config.backRowPrefabs, prefab => prefab.name);
            PlayerPrefs.SetString(SetupKey(config.dropDownOptionName), $"{config.sceneName}|{string.Join(",", prefabNames)}");
        }
    }

    // Same PlayerPrefs handoff SceneLoader.LoadWithConfig does, from the saved setup
    public static bool TryLaunch(string chapter)
    {
        string setup = PlayerPrefs.GetString(SetupKey(chapter), "");
        string[] parts = setup.Split('|');
        if (parts.Length != 2) return false;

        string[] prefabNames = parts[1].Split(',');
        PlayerPrefs.SetInt("correspondenceMode", 0);
        SetCurrentChapter(chapter);
        PlayerPrefs.SetInt("backRowCount", prefabNames.Length);
        for (int i = 0; i < prefabNames.Length; i++)
            PlayerPrefs.SetString($"backRowPrefab_{i}", prefabNames[i]);

        UnityEngine.SceneManagement.SceneManager.LoadScene(parts[0]);
        return true;
    }

    public static void ResetAll()
    {
        foreach (string chapter in Order)
            PlayerPrefs.DeleteKey(WonKey(chapter));
        PlayerPrefs.Save();
    }
}
