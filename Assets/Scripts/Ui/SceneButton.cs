using System;
using UnityEngine;

public class SceneButton : MonoBehaviour
{
    public SceneLoader sceneLoader;
    public int configIndex;

    void OnValidate()
    {
        if (sceneLoader == null || sceneLoader.sceneConfigs == null) return;
        string label = gameObject.name.Trim('"');
        int index = Array.FindIndex(sceneLoader.sceneConfigs, c => c.dropDownOptionName == label);
        if (index >= 0)
            configIndex = index;
    }

    public void OnClick()
    {
        sceneLoader.LoadWithConfig(sceneLoader.sceneConfigs[configIndex]);
    }
}
