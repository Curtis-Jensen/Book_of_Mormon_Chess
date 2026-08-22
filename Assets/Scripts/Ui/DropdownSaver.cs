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

        Save();
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
    }

    // Called by SettingsDefaultsSeeder so this control's own configured defaultValue
    // becomes the saved value even if this GameObject is still inactive.
    public void SeedIfMissing()
    {
        if (!PlayerPrefs.HasKey(toSave))
        {
            PlayerPrefs.SetInt(toSave, defaultValue);
        }
    }
}
