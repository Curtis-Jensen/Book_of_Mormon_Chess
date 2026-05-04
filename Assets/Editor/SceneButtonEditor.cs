using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneButton))]
public class SceneButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SceneButton button = (SceneButton)target;

        button.sceneLoader = (SceneLoader)EditorGUILayout.ObjectField(
            "Scene Loader", button.sceneLoader, typeof(SceneLoader), true);

        if (button.sceneLoader != null && button.sceneLoader.sceneConfigs != null
            && button.sceneLoader.sceneConfigs.Length > 0)
        {
            string[] names = System.Array.ConvertAll(
                button.sceneLoader.sceneConfigs, c => c.dropDownOptionName);
            button.configIndex = EditorGUILayout.Popup("Scene Config", button.configIndex, names);
        }
        else
        {
            button.configIndex = EditorGUILayout.IntField("Config Index", button.configIndex);
            EditorGUILayout.HelpBox("Assign a Scene Loader to see config names.", MessageType.Info);
        }

        if (GUI.changed)
            EditorUtility.SetDirty(button);
    }
}
