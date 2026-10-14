using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put on the Settings panel. tabs[i] shows pages[i] and hides the others; the active
// tab gets the gold plate, the rest get the dark one.
public class SettingsTabs : MonoBehaviour
{
    public Button[] tabs;
    public GameObject[] pages;
    public Sprite activeSprite;
    public Sprite inactiveSprite;
    public float activePixelsPerUnitMultiplier = 1f;
    public float inactivePixelsPerUnitMultiplier = 1f;
    public Color activeTextColor = Color.black;
    public Color inactiveTextColor = Color.white;

    void Awake()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            tabs[i].onClick.AddListener(() => Show(index));
        }
    }

    // Always reopen on the first tab
    void OnEnable()
    {
        Show(0);
    }

    public void Show(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = i == index;
            pages[i].SetActive(isActive);

            var image = tabs[i].GetComponent<Image>();
            image.sprite = isActive ? activeSprite : inactiveSprite;
            image.pixelsPerUnitMultiplier = isActive ? activePixelsPerUnitMultiplier : inactivePixelsPerUnitMultiplier;

            var label = tabs[i].GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = isActive ? activeTextColor : inactiveTextColor;
        }
    }
}
