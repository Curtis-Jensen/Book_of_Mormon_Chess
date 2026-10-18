using UnityEngine;
using UnityEngine.UI;

// Two-tab Settings screen: shows one content root at a time and dims the selected tab button.
public class SettingsTabs : MonoBehaviour
{
    [SerializeField] Button[] tabButtons;
    [SerializeField] GameObject[] tabContents;
    [SerializeField, Range(0f, 1f)] float inactiveAlpha = 0.55f;

    void Awake()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => Show(index));
        }
    }

    void OnEnable() => Show(0);

    void Show(int index)
    {
        for (int i = 0; i < tabContents.Length; i++)
        {
            tabContents[i].SetActive(i == index);
            tabButtons[i].targetGraphic.canvasRenderer.SetAlpha(i == index ? 1f : inactiveAlpha);
        }
    }
}
