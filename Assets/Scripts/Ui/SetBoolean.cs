using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetBoolean : MonoBehaviour
{
    public string boolVariableName;

    public Toggle toggle;

    private void Awake()
    {
        int defaultValue = toggle.isOn ? 1 : 0;
        toggle.isOn = PlayerPrefs.GetInt(boolVariableName, defaultValue) == 1;
        SetBool();
    }

    public void SetBool()
    {
        PlayerPrefs.SetInt(boolVariableName, toggle.isOn ? 1 : 0);
    }
}
