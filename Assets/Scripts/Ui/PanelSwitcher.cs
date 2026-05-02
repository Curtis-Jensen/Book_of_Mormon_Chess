using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject newPanel;

    public void Switch()
    {
        newPanel.SetActive(true);
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}
