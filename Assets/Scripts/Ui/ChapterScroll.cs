using UnityEngine;
using UnityEngine.UI;

// Put on the Chapters Panel. Wraps its children in a vertical ScrollRect so that when the
// screen is shorter than minContentHeight (canvas units) the buttons keep a nice height and
// the panel scrolls instead of squashing them. Children whose name contains a pinned keyword
// (the Back button) stay fixed to the panel.
[RequireComponent(typeof(RectTransform))]
public class ChapterScroll : MonoBehaviour
{
    [SerializeField] float minContentHeight = 450f;
    [SerializeField] string[] pinnedNameKeywords = { "Back" };

    RectTransform content;
    RectTransform viewport;

    void Awake()
    {
        var panel = (RectTransform)transform;

        viewport = NewStretchedChild("Scroll Viewport", panel);
        viewport.gameObject.AddComponent<RectMask2D>();
        content = NewStretchedChild("Scroll Content", viewport);
        content.pivot = new Vector2(0.5f, 1f);

        // Snapshot first: reparenting changes the child list while we iterate
        var children = new Transform[panel.childCount];
        for (int i = 0; i < children.Length; i++) children[i] = panel.GetChild(i);

        foreach (var child in children)
        {
            if (child == viewport) continue;
            if (IsPinned(child.name)) continue;
            child.SetParent(content, false);
        }

        // Keep the viewport underneath pinned children so the Back button stays clickable
        viewport.SetAsFirstSibling();

        var scroll = panel.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
    }

    void OnEnable() => Resize();
    void OnRectTransformDimensionsChange() => Resize();

    bool IsPinned(string childName)
    {
        foreach (var keyword in pinnedNameKeywords)
            if (childName.Contains(keyword)) return true;
        return false;
    }

    void Resize()
    {
        if (content == null) return;
        float shortfall = Mathf.Max(0f, minContentHeight - ((RectTransform)transform).rect.height);
        // Anchored full-height at the top; sizeDelta.y adds the extra height below the screen
        content.offsetMin = new Vector2(0f, -shortfall);
        content.offsetMax = Vector2.zero;
    }

    static RectTransform NewStretchedChild(string childName, Transform parent)
    {
        var go = new GameObject(childName, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }
}
