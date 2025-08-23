using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetBoolean : MonoBehaviour
{
    public string boolVariableName;

    public Toggle toggle;

    public int defaultValue;

    private void Awake()
    {
        toggle.isOn = PlayerPrefs.GetInt(boolVariableName, defaultValue) == 1;
    }

    public void SetBool()
    {
        PlayerPrefs.SetInt(boolVariableName, toggle.isOn ? 1 : 0);
    }
}
