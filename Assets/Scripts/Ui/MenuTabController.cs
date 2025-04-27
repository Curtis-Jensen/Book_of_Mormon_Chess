using UnityEngine;
using UnityEngine.UI;

public class MenuTabController : MonoBehaviour
{
    [System.Serializable]
    public struct Tab
    {
        public Button button; // The tab button
        public GameObject panel; // The corresponding panel
    }

    public Tab[] tabs; // Assign in Inspector
    public int defaultTab = 0; // Index of default tab to show on start

    void Start()
    {
        // Add listeners and set default tab
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i; // Capture index for listener
            tabs[i].button.onClick.AddListener(() => ShowTab(index));
        }
        ShowTab(defaultTab);
    }

    void ShowTab(int index)
    {
        // Hide all panels, show selected one
        foreach (var tab in tabs)
        {
            tab.panel.SetActive(false);
        }
        tabs[index].panel.SetActive(true);
    }
}