using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DropdownPopulator))]
public class DropdownPopulatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var populator = (DropdownPopulator)target;
        if (GUILayout.Button("Populate Now"))
        {
            populator.PopulateNow();
        }
    }
}
