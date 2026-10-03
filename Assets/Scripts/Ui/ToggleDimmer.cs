using UnityEngine;
using UnityEngine.UI;

// Dims and disables a set of rows whenever a toggle is off,
// e.g. the Player 3 / Player 4 settings while Duos is unchecked.
public class ToggleDimmer : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    [SerializeField] CanvasGroup[] dependentRows;
    [SerializeField] float dimmedAlpha = 0.35f;

    void OnEnable()
    {
        toggle.onValueChanged.AddListener(Apply);
        Apply(toggle.isOn);
    }

    void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(Apply);
    }

    void Apply(bool isOn)
    {
        foreach (var row in dependentRows)
        {
            row.alpha = isOn ? 1f : dimmedAlpha;
            row.interactable = isOn;
            row.blocksRaycasts = isOn;
        }
    }
}
