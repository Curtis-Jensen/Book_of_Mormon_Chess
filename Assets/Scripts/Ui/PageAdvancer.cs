using UnityEngine;

public class PageAdvancer : MonoBehaviour
{
    [SerializeField] private GameObject currentPage;
    [SerializeField] private GameObject nextPage;

    public void AdvancePage()
    {
        nextPage.SetActive(true);
        currentPage.SetActive(false);
    }
}
