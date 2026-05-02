using UnityEngine;

public class Flipbook : MonoBehaviour
{
    [SerializeField] private GameObject[] pages; // One GameObject per game mode page
    [SerializeField] private SceneLoader sceneLoader; // Reference to get sceneConfigs
    
    private int currentPageIndex = 0;

    private void Start()
    {
        ShowPage(currentPageIndex);
    }

    /// <summary>
    /// Move to the next page, wrapping around to the first page if at the end.
    /// </summary>
    public void NextPage()
    {
        currentPageIndex = (currentPageIndex + 1) % pages.Length;
        ShowPage(currentPageIndex);
    }

    /// <summary>
    /// Move to the previous page, wrapping around to the last page if at the start.
    /// </summary>
    public void PreviousPage()
    {
        currentPageIndex = (currentPageIndex - 1 + pages.Length) % pages.Length;
        ShowPage(currentPageIndex);
    }

    /// <summary>
    /// Get the name of the current game mode (from SceneConfigs).
    /// </summary>
    public string GetCurrentModeName()
    {
        return sceneLoader.sceneConfigs[currentPageIndex].dropDownOptionName;
    }

    /// <summary>
    /// Show the page at the given index and hide all others.
    /// </summary>
    private void ShowPage(int pageIndex)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == pageIndex);
        }
    }
}
