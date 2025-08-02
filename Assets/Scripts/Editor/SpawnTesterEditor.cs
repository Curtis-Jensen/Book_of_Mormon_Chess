using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnTester))]
public class SpawnTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add space then button
        GUILayout.Space(10);
        if (GUILayout.Button("Test Spawn"))
        {
            // Call method on target
            SpawnTester tester = (SpawnTester)target;
            tester.TestSpawn();
        }
    }
}
