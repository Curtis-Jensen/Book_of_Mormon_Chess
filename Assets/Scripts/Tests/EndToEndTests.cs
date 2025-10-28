using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class EndToEndTests : MonoBehaviour
{
    public SceneLoader sceneLoader;

    public void StartTest()
    {
        // Load first scene from sceneLoader configs
        var firstConfig = sceneLoader.sceneConfigs[0];
        PlayerPrefs.SetString("gameMode", firstConfig.sceneName);
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
            tester.StartTest();
        }
    }
}
#endif