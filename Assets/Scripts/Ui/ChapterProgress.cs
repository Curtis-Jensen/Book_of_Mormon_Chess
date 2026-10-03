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

    public static void MarkCurrentChapterWon()
    {
        string chapter = PlayerPrefs.GetString(CurrentChapterKey, "");
        if (Array.IndexOf(Order, chapter) < 0) return;

        PlayerPrefs.SetInt(WonKey(chapter), 1);
        PlayerPrefs.Save();
    }

    public static void ResetAll()
    {
        foreach (string chapter in Order)
            PlayerPrefs.DeleteKey(WonKey(chapter));
        PlayerPrefs.Save();
    }
}
