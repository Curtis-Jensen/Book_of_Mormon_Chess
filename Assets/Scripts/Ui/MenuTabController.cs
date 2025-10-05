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

    public GameObject firstMenuPanel; // The initial menu panel
    public Button nextButton; // Button to go to tabbed menu
    public GameObject tabButtonsContainer; // Container for tab buttons
    public Button backButton; // Button to return to main menu
    public Tab[] tabs; // Assign in Inspector
    public int defaultTab = 0; // Index of default tab to show on start

    void Start()
    {
        // Initialize menu state
        ShowMainMenu();

        // Add listeners for tabs
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i; // Capture index for listener
            tabs[i].button.onClick.AddListener(() => ShowTab(index));
        }

        // Add back button listener
        backButton.onClick.AddListener(ShowMainMenu);
        
        nextButton.onClick.AddListener(() => ShowTab(0));
        nextButton.onClick.AddListener(ShowTabbedMenu);
    }

    public void ShowMainMenu()
    {
        // Hide all tab panels and the tab buttons
        foreach (var tab in tabs)
        {
            tab.panel.SetActive(false);
        }
        tabButtonsContainer.SetActive(false);
        
        // Show main menu
        firstMenuPanel.SetActive(true);
    }

    public void ShowTabbedMenu()
    {
        // Hide main menu
        firstMenuPanel.SetActive(false);
        
        // Show tab buttons and back button
        tabButtonsContainer.SetActive(true);
        backButton.gameObject.SetActive(true);
        
        // Show default tab
        ShowTab(defaultTab);
    }

    public void ShowTab(int index = 0)
    {
        // Hide all panels, show selected one
        foreach (var tab in tabs)
        {
            tab.panel.SetActive(false);
        }
        tabs[index].panel.SetActive(true);
    }
}