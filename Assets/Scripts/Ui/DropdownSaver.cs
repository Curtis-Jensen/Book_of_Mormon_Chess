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

        // Do NOT call Save() here. OnValidate fires for ANY Inspector edit on this
        // component -- including editing Default Value itself -- and Save() writes
        // the dropdown's current runtime value (unrelated to the field you just
        // edited), silently overwriting PlayerPrefs back to the old value.
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
            // dropdown.value here is still whatever was configured on the dropdown itself
            // in the Editor (Awake/SeedIfMissing runs before this), so a missing key falls
            // back to that -- the dropdown's own Value field is the one source of truth.
            dropdown.value = PlayerPrefs.GetInt(toSave, dropdown.value);
            Save();
        }
    }

    public void Save()
    {
        PlayerPrefs.SetInt(toSave, dropdown.value);
    }

    // Called by SettingsDefaultsSeeder so the dropdown's own configured Value becomes
    // the saved default even if this GameObject is still inactive.
    public void SeedIfMissing()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        if (!PlayerPrefs.HasKey(toSave))
        {
            PlayerPrefs.SetInt(toSave, dropdown.value);
        }
    }
}
