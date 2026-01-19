using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class EndToEndTests : MonoBehaviour
{
    private int currentSceneIndex = 0;
    private SceneLoader sceneLoader;

    private void OnEnable()
    {
        EndingManager.OnGameEnd += OnGameEnded;
    }

    private void OnDisable()
    {
        EndingManager.OnGameEnd -= OnGameEnded;
    }

    public void StartTest()
    {
        sceneLoader = FindObjectOfType<SceneLoader>();
        if (sceneLoader == null)
        {
            Debug.LogError("SceneLoader not found!");
            return;
        }

        // Make this GameObject persist between scenes
        DontDestroyOnLoad(gameObject);

        // Start with first scene
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (sceneLoader.sceneConfigs == null || currentSceneIndex >= sceneLoader.sceneConfigs.Length)
        {
            Debug.Log("End-to-end test completed!");
            return;
        }

        var config = sceneLoader.sceneConfigs[currentSceneIndex];
        PlayerPrefs.SetString("gameMode", config.dropDownOptionName);
        PlayerPrefs.SetInt("boardSize", 8); // Default size
        sceneLoader.SetupNewScene();
    }

    private void OnGameEnded(int losingPlayerIndex)
    {
        int winningPlayerIndex = losingPlayerIndex == 0 ? 1 : 0;
        Debug.Log($"Game ended in scene {currentSceneIndex} with player {winningPlayerIndex} winning (player {losingPlayerIndex} lost)");
        currentSceneIndex++;
        
        // Wait a bit to see the victory animation before moving to next scene
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Adjust delay as needed
        LoadNextScene();
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
                //EditorApplication.EnterPlaymode();
                Debug.LogError("You must be in Play Mode to run the test.  Perhaps one day we can have it work, but at the moment when it enters play mode it forgets all other instructions.  Using Player prefs or something might help?");
            }

            tester.StartTest();
        }
    }
}
#endif