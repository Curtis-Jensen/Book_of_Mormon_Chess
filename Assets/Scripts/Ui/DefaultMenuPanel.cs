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
        // Other scenes set this (PlayerPrefs) to land on a specific panel instead of the defaults
        public const string OpenPanelKey = "openPanelOnLoad";

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

            string requested = PlayerPrefs.GetString(OpenPanelKey, "");
            if (requested == "") return;
            PlayerPrefs.DeleteKey(OpenPanelKey);

            Transform target = transform.Find(requested);
            if (target == null) return;

            // Background stays; every other default panel makes way for the requested one
            foreach (GameObject obj in initiallyActiveObjects)
            {
                if (obj.name != "Background") obj.SetActive(false);
            }
            target.gameObject.SetActive(true);
        }
    }
}
