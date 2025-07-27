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
        toggle.isOn = PlayerPrefs.GetInt("isAi", 1) == 1 ? true : false;
    }
}
