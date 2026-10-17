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
    // Optional: assign in the prefab to style it yourself; otherwise one is built at runtime.
    public Button closeButton;

    public void Set(string opponent, string status, Action onClick, Action onClose = null)
    {
        if (opponentText != null) opponentText.text = opponent;
        if (statusText != null) statusText.text = status;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());

        if (onClose == null) return;
        if (closeButton == null) closeButton = BuildCloseButton();
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => onClose());
    }

    // Tints the whole row so it stands out in the list (e.g. games waiting on your move).
    public void Highlight(Color tint)
    {
        if (button != null && button.targetGraphic != null) button.targetGraphic.color = tint;
    }

    // Plain black "X" pinned to the row's right edge, built in code so no prefab edit is required.
    Button BuildCloseButton()
    {
        var go = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(transform, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-10f, 0f);
        rect.sizeDelta = new Vector2(40f, 40f);
        // Fully transparent image keeps the whole square clickable without drawing a background.
        go.GetComponent<Image>().color = Color.clear;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        var labelRect = (RectTransform)labelGo.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        var label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = "X";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 28;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.black;
        label.raycastTarget = false;

        return go.GetComponent<Button>();
    }
}
