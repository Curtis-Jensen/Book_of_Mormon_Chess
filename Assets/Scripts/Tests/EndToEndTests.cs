using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class EndToEndTests : MonoBehaviour
{
    public void StartTest()
    {
        var sceneLoader = FindObjectOfType<SceneLoader>();

        // Make this GameObject persist between scenes
        DontDestroyOnLoad(gameObject);

        // Load first scene from sceneLoader configs
        var firstConfig = sceneLoader.sceneConfigs[0];
        PlayerPrefs.SetString("gameMode", firstConfig.dropDownOptionName);
        PlayerPrefs.SetInt("boardSize", 8); // Default size
        sceneLoader.SetupNewScene();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EndToEndTests))]
public class EndToEndTestsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EndToEndTests tester = (EndToEndTests)target;
        
        if (GUILayout.Button("Run First Scene Test"))
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.EnterPlaymode();
            }

            tester.StartTest();
        }
    }
}
#endif