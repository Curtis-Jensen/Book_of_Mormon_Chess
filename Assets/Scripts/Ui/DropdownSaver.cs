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

    public void Awake()
    {
        dropdown = gameObject.GetComponent<TMP_Dropdown>();
        dropdown.value = PlayerPrefs.GetInt(toSave, defaultValue);
        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetInt(toSave, dropdown.value);
    }
}
