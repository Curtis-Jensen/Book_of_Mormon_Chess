using UnityEngine;

namespace Ui
{
    // Put this on the panel that should show first when the game starts.
    // On Awake it activates itself and deactivates its sibling panels,
    // so leaving the wrong panel active in the editor can't break a build.
    public class DefaultMenuPanel : MonoBehaviour
    {
        [Tooltip("Parent whose direct children are the menu panels. Defaults to this object's parent.")]
        [SerializeField] private Transform panelsRoot;

        private void Awake()
        {
            Transform root = panelsRoot != null ? panelsRoot : transform.parent;
            if (root == null)
            {
                gameObject.SetActive(true);
                return;
            }

            foreach (Transform child in root)
            {
                child.gameObject.SetActive(child == transform);
            }
        }
    }
}
