using System.Collections.Generic;
using UnityEngine;

namespace Ui
{
    // Put this on an always-active object (e.g. the Canvas) and list the
    // objects that should be active when the game starts. Deactivates every
    // direct child of this object, then reactivates the listed ones, so
    // forgetting to reset panel visibility while working on menus can't
    // break a build.
    public class DefaultMenuPanel : MonoBehaviour
    {
        [SerializeField] private List<GameObject> initiallyActiveObjects;

        private void Awake()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            foreach (GameObject obj in initiallyActiveObjects)
            {
                obj.SetActive(true);
            }
        }
    }
}
