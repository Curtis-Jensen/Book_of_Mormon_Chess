using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownSaver : MonoBehaviour
{
    TMP_Dropdown dropdown;
    public string toSave;
    public int defaultValue;
    [SerializeField] private SceneConfig[] sceneConfigs;
    
    private void Reset()
    {
        // This ensures we have the dropdown component when the script is first added
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnValidate()
    {
        // This ensures we always have a reference to the dropdown
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }
        LoadSavedValue();
    }

    private void OnEnable()
    {
        LoadSavedValue();
    }

    private void LoadSavedValue()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }
        
        if (dropdown != null)
        {
            dropdown.value = PlayerPrefs.GetInt(toSave, defaultValue);
            Save();
        }
    }

    public void Save()
    {
        PlayerPrefs.SetInt(toSave, dropdown.value);
        SaveConfigPrefabs();
    }

    /// <summary>
    /// Saves the selected config's prefabs to PlayerPrefs so BoardSetup can load them.
    /// </summary>
    private void SaveConfigPrefabs()
    {
        if (sceneConfigs == null || sceneConfigs.Length == 0)
            return;

        int selectedIndex = dropdown.value;
        if (selectedIndex < 0 || selectedIndex >= sceneConfigs.Length)
            return;

        SceneConfig selectedConfig = sceneConfigs[selectedIndex];
        if (selectedConfig.backRowPrefabs == null || selectedConfig.backRowPrefabs.Length == 0)
            return;

        PlayerPrefs.SetString("gameMode", selectedConfig.dropDownOptionName);
        PlayerPrefs.SetInt("backRowCount", selectedConfig.backRowPrefabs.Length);

        for (int i = 0; i < selectedConfig.backRowPrefabs.Length; i++)
        {
            if (selectedConfig.backRowPrefabs[i] != null)
            {
                PlayerPrefs.SetString($"backRowPrefab_{i}", selectedConfig.backRowPrefabs[i].name);
            }
        }

        PlayerPrefs.Save();
    }
}
