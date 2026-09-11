using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Automatically populates dropdown menus with piece options based on SceneConfigs.
/// Generates dropdown options from available game mode configurations.
/// Saving of the selected option is handled by DropdownSaver.
/// </summary>
// ExecuteAlways makes Start() (and therefore PopulateDropdown) run in the Editor as
// soon as the component loads, not just when you press Play -- so the dropdown's
// options always reflect PieceSets without needing a Play-mode test to see it.
[ExecuteAlways]
[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownPopulator : MonoBehaviour
{
    [SerializeField] private PieceSets pieceSets;
    private TMP_Dropdown gameModeDropdown;

    private void Start()
    {
        gameModeDropdown = GetComponent<TMP_Dropdown>();
        PopulateDropdown();
    }

    // Also exposed as a button in the custom Inspector (see Editor/DropdownPopulatorEditor.cs)
    // for forcing a refresh after editing PieceSets without waiting for a scene reload.
    [ContextMenu("Populate Now")]
    public void PopulateNow()
    {
        gameModeDropdown = GetComponent<TMP_Dropdown>();
        PopulateDropdown();
    }

    /// <summary>
    /// Populates the dropdown with options based on SceneConfigs and their associated piece prefabs.
    /// Automatically generates readable display names from the piece prefab names.
    /// </summary>
    public void PopulateDropdown()
    {
        // ClearOptions() resets the dropdown's value to 0 as a side effect, which would
        // wipe out whatever DropdownSaver already loaded from PlayerPrefs in OnEnable
        // (OnEnable always runs before Start, so that value is already correct here).
        int savedValue = gameModeDropdown.value;

        gameModeDropdown.ClearOptions();
        List<string> options = new();

        // Add an option for each SceneConfig
        foreach (var spriteSet in pieceSets.spriteSets)
        {
            options.Add(spriteSet.name);
        }

        gameModeDropdown.AddOptions(options);

        gameModeDropdown.value = savedValue;
        gameModeDropdown.RefreshShownValue();
    }
}
