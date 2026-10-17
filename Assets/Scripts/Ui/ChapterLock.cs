using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Put on a chapter button next to its SceneButton. Disables the button, greys its
// piece icons, and adds a "Locked" line until ChapterProgress unlocks it.
[RequireComponent(typeof(Button), typeof(SceneButton))]
public class ChapterLock : MonoBehaviour, IPointerClickHandler
{
    const int ClicksToUnlock = 3;
    const float ClickWindow = 1.5f;
    int clickCount;
    float lastClickTime;

    [SerializeField] Color lockedIconColor = new Color(0.45f, 0.42f, 0.38f, 0.8f);
    [SerializeField] Color unlockedIconColor = new Color(1f, 0.82f, 0.35f);

    void OnEnable()
    {
        var sceneButton = GetComponent<SceneButton>();
        string chapter = sceneButton.sceneLoader.sceneConfigs[sceneButton.configIndex].dropDownOptionName;
        bool unlocked = ChapterProgress.IsUnlocked(chapter);

        // The button's own disabled tint darkens the gold frame
        GetComponent<Button>().interactable = unlocked;

        foreach (var image in GetComponentsInChildren<Image>(true))
        {
            if (image.gameObject != gameObject && image.name.StartsWith("Icon"))
                image.color = unlocked ? unlockedIconColor : lockedIconColor;
        }

        var label = GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = unlocked
                ? chapter
                : $"{chapter}\n<size=60%>Locked</size>";
        }
    }

    // Mash a locked chapter 3 times to skip ahead: marks every chapter before it as won
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GetComponent<Button>().interactable) return;

        if (Time.unscaledTime - lastClickTime > ClickWindow) clickCount = 0;
        lastClickTime = Time.unscaledTime;
        if (++clickCount < ClicksToUnlock) return;
        clickCount = 0;

        var sceneButton = GetComponent<SceneButton>();
        string chapter = sceneButton.sceneLoader.sceneConfigs[sceneButton.configIndex].dropDownOptionName;
        int index = System.Array.IndexOf(ChapterProgress.Order, chapter);
        for (int i = 0; i < index; i++)
            PlayerPrefs.SetInt($"chapterWon_{ChapterProgress.Order[i]}", 1);
        PlayerPrefs.Save();

        foreach (var lockComponent in FindObjectsByType<ChapterLock>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            lockComponent.enabled = false;
            lockComponent.enabled = true;
        }
    }
}
