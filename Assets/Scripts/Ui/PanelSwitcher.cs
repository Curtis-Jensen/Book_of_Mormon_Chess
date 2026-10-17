using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject newPanel;

    public void Switch()
    {
        newPanel.SetActive(true);
        OwningPanel().SetActive(false);
    }

    // The menu panel is the ancestor sitting directly under DefaultMenuPanel. Using the immediate
    // parent breaks when ChapterScroll reparents buttons into its "Scroll Content" wrapper:
    // we'd hide only the wrapper, and the panel would come back blank.
    GameObject OwningPanel()
    {
        var host = GetComponentInParent<Ui.DefaultMenuPanel>();
        if (host != null)
        {
            Transform t = transform;
            while (t.parent != null && t.parent != host.transform) t = t.parent;
            if (t.parent == host.transform) return t.gameObject;
        }
        return transform.parent.gameObject;
    }
}
