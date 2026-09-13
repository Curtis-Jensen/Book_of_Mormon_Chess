using UnityEngine;

// One shared "Customize Pieces" panel reused by every player's own button, instead of
// duplicating the whole panel per player. Opening it for a given player retargets each
// child dropdown's DropdownSaver key to that player's PlayerPrefs prefix and reloads
// its saved value, so the same dropdowns edit whichever player's styles you opened it for.
public class PieceCustomizationPanelController : MonoBehaviour
{
    public void OpenForPlayer(string playerName)
    {
        gameObject.SetActive(true);

        foreach (var saver in GetComponentsInChildren<DropdownSaver>(includeInactive: true))
        {
            var populator = saver.GetComponent<DropdownPopulator>();
            saver.toSave = playerName + "style" + populator.PieceTypeName;
            saver.LoadSavedValue();
        }
    }
}
