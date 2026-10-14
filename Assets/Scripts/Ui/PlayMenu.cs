using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Put on the Play panel. Hovering or clicking a mode in the list describes it on the
// right; the Start button launches whichever mode is selected.
public class PlayMenu : MonoBehaviour
{
    [Serializable]
    public class Mode
    {
        public Button listButton;
        public string kind;
        public string title;
        [TextArea] public string description;
        public string startLabel = "Start";

        [Tooltip("SceneConfig.dropDownOptionName to load. Leave empty to open panelToOpen instead.")]
        public string sceneConfigName;
        [Tooltip("-1 leaves the Duos setting alone, 0 forces two players, 1 forces four.")]
        public int fourPlayerMode = -1;
        public GameObject panelToOpen;
    }

    public Mode[] modes;
    public SceneLoader sceneLoader;
    public TMP_Text kindText;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text startText;
    public Button startButton;

    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public float selectedPixelsPerUnitMultiplier = 1f;
    public float unselectedPixelsPerUnitMultiplier = 1f;
    public Color selectedTextColor = Color.black;
    public Color unselectedTextColor = Color.white;

    int selected;

    void Awake()
    {
        for (int i = 0; i < modes.Length; i++)
        {
            int index = i;
            modes[i].listButton.onClick.AddListener(() => Select(index));

            var hover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            hover.callback.AddListener(_ => Select(index));
            modes[i].listButton.gameObject.AddComponent<EventTrigger>().triggers.Add(hover);
        }
        startButton.onClick.AddListener(StartSelected);
    }

    void OnEnable()
    {
        Select(selected);
    }

    public void Select(int index)
    {
        selected = index;
        for (int i = 0; i < modes.Length; i++)
        {
            bool isSelected = i == index;
            var image = modes[i].listButton.GetComponent<Image>();
            image.sprite = isSelected ? selectedSprite : unselectedSprite;
            image.pixelsPerUnitMultiplier = isSelected ? selectedPixelsPerUnitMultiplier : unselectedPixelsPerUnitMultiplier;

            var label = modes[i].listButton.GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = isSelected ? selectedTextColor : unselectedTextColor;
        }

        var mode = modes[index];
        kindText.text = mode.kind;
        titleText.text = mode.title;
        descriptionText.text = mode.description;
        startText.text = mode.startLabel;
    }

    void StartSelected()
    {
        var mode = modes[selected];
        if (mode.panelToOpen != null)
        {
            mode.panelToOpen.SetActive(true);
            gameObject.SetActive(false);
            return;
        }

        if (mode.fourPlayerMode >= 0) PlayerPrefs.SetInt("4playerMode", mode.fourPlayerMode);

        var config = Array.Find(sceneLoader.sceneConfigs, c => c.dropDownOptionName == mode.sceneConfigName);
        if (config == null)
        {
            Debug.LogError($"[PlayMenu] No SceneConfig named '{mode.sceneConfigName}' on the SceneLoader.");
            return;
        }
        sceneLoader.LoadWithConfig(config);
    }
}
