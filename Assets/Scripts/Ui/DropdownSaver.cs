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

    public void Start()
    {
        dropdown = gameObject.GetComponent<TMP_Dropdown>();
        dropdown.value = PlayerPrefs.GetInt(toSave, defaultValue);
        //We have to save to to make sure they don't both start with the 0 color
        PlayerPrefs.SetInt(toSave, dropdown.value);
    }

    public void Save()
    {
        PlayerPrefs.SetInt(toSave, dropdown.value);
    }
}
