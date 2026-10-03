using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put on a chapter button next to its SceneButton. Disables the button, greys its
// piece icons, and adds a "Win X to unlock" line until ChapterProgress unlocks it.
[RequireComponent(typeof(Button), typeof(SceneButton))]
public class ChapterLock : MonoBehaviour
{
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
                : $"{chapter}\n<size=55%>Win {ChapterProgress.PrerequisiteOf(chapter)} to unlock</size>";
        }
    }
}
