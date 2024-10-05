using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetBoolean : MonoBehaviour
{
    public string boolVariableName;

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.isOn = PlayerPrefs.GetInt("bool" + boolVariableName) == 1 ? true : false;
    }

    public void SetBool()
    {
        var isOnInt = toggle.isOn ? 1 : 0;

        PlayerPrefs.SetInt("bool" + boolVariableName, isOnInt);
    }
}
