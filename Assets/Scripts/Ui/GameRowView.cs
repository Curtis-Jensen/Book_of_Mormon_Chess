using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One row in the "My Games" list -- opponent name, whose-turn status, and a
// click handler to resume that specific game. Instantiated per-row by MyGamesMenu.
public class GameRowView : MonoBehaviour
{
    public TMP_Text opponentText;
    public TMP_Text statusText;
    public Button button;

    public void Set(string opponent, string status, Action onClick)
    {
        if (opponentText != null) opponentText.text = opponent;
        if (statusText != null) statusText.text = status;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());
    }
}
