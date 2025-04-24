using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownSaver : MonoBehaviour
{
    public void Save()
    {
        var dropdown = gameObject.GetComponent<TMP_Dropdown>();

        PlayerPrefs.SetInt("style", dropdown.value);
    }
}
