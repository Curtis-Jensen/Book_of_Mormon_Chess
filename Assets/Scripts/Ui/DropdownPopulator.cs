using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Automatically populates dropdown menus with piece options based on SceneConfigs.
/// Generates dropdown options from available game mode configurations.
/// Saving of the selected option is handled by DropdownSaver.
/// </summary>
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

    /// <summary>
    /// Populates the dropdown with options based on SceneConfigs and their associated piece prefabs.
    /// Automatically generates readable display names from the piece prefab names.
    /// </summary>
    public void PopulateDropdown()
    {
        gameModeDropdown.ClearOptions();
        List<string> options = new();

        // Add an option for each SceneConfig
        foreach (var spriteSet in pieceSets.spriteSets)
        {
            Debug.Log(spriteSet.name);
            options.Add(spriteSet.name);
        }

        gameModeDropdown.AddOptions(options);
    }
}
