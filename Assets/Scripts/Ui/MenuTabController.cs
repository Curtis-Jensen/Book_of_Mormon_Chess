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

    [SerializeField] GameObject firstMenuPanel; // The initial menu panel
    [SerializeField] GameObject tabButtonsContainer; // Container for tab buttons
    [SerializeField] Button backButton; // Button to return to main menu
    [SerializeField] Tab[] tabs; // Assign in Inspector

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
        ShowTab(0);
    }

    public void ShowTab(int index)
    {
        // Hide all panels, show selected one
        foreach (var tab in tabs)
        {
            tab.panel.SetActive(false);
        }
        tabs[index].panel.SetActive(true);
    }
}