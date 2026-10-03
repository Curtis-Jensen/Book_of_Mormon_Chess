using UnityEditor;
using UnityEngine;

// Quick toggles for testing the training chapter unlock order
public static class ChapterProgressMenu
{
    [MenuItem("BOM Chess/Chapters/Reset Progress")]
    static void ResetProgress()
    {
        ChapterProgress.ResetAll();
        Debug.Log("Chapter progress reset -- only Pawns is unlocked.");
    }

    [MenuItem("BOM Chess/Chapters/Unlock All")]
    static void UnlockAll()
    {
        foreach (string chapter in ChapterProgress.Order)
            PlayerPrefs.SetInt($"chapterWon_{chapter}", 1);
        PlayerPrefs.Save();
        Debug.Log("All chapters unlocked.");
    }
}
