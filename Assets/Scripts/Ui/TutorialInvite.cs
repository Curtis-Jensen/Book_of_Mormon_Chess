using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// In an online game, while the player is waiting on their opponent and hasn't won
// Stripling Warriors yet, keeps a "Play Tutorial" invite in the bottom-left margin.
// Hooks itself in at scene load so the Duel scene needs no extra setup.
public class TutorialInvite : MonoBehaviour
{
    const string Chapter = "Stripling Warriors";
    TurnProgresser turns;
    GameObject invite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (_, _) => Hook();
        Hook(); // The scene that was already loading at startup (e.g. Play Mode straight from the Duel scene) won't fire sceneLoaded
    }

    static void Hook()
    {
        var turns = TurnProgresser.Instance;
        if (turns == null || !turns.correspondenceMode || ChapterProgress.IsWon(Chapter)) return;

        if (FindAnyObjectByType<TutorialInvite>() != null) return;

        var host = new GameObject("Tutorial Invite");
        var invite = host.AddComponent<TutorialInvite>();
        invite.turns = turns;
        invite.Build();
    }

    void Build()
    {
        var template = GameObject.Find("<- Main Menu");
        if (template == null) { Destroy(gameObject); return; }

        // Clone the scene's own Main Menu button so the invite shares its look and its
        // left margin, just anchored to the bottom-left corner instead of the top-left.
        invite = Instantiate(template, template.transform.parent);
        invite.name = "Tutorial Invite Button";

        var rect = (RectTransform)invite.transform;
        var source = (RectTransform)template.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(source.anchoredPosition.x, -source.anchoredPosition.y);
        rect.sizeDelta = source.sizeDelta;

        var button = invite.GetComponent<Button>();
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
        button.onClick.AddListener(OpenSinglePlayerMenu);

        foreach (var label in invite.GetComponentsInChildren<TMP_Text>(true))
            label.text = "Play Tutorial";

        invite.SetActive(false);
    }

    // Single Player lives on the main menu's Chapters Panel, where Stripling Warriors is unlocked
    static void OpenSinglePlayerMenu()
    {
        PlayerPrefs.SetString(Ui.DefaultMenuPanel.OpenPanelKey, "Chapters Panel");
        SceneManager.LoadScene("BOM Main Menu");
    }

    // Visible only while waiting on the opponent -- which is also true right after the
    // player's first move, and when reopening an old game that hasn't been answered yet
    void Update()
    {
        if (turns == null || invite == null) return;
        invite.SetActive(turns.HasInitialState && turns.CurrentTurn != turns.localPlayerIndex);
    }

    void OnDestroy()
    {
        if (invite != null) Destroy(invite);
    }
}
