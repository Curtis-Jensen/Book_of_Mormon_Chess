using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PieceSpawner))]
public sealed class PieceSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        var changed = EditorGUI.EndChangeCheck();

        serializedObject.ApplyModifiedProperties();

        if (!changed)
            return;

        //Debug.Log($"[PieceSpawnerEditor] Inspector changed for '{target.name}'. Writing isAi -> PlayerPrefs...", target);
        WritePrefsFromSerialized(target, serializedObject.FindProperty("players"));
    }

    private static void WritePrefsFromSerialized(Object context, SerializedProperty playersProp)
    {
        for (var i = 0; i < 4; i++)
        {
            var playerProp = playersProp.GetArrayElementAtIndex(i);
            var isAiProp = playerProp?.FindPropertyRelative("isAi");
            var isAi = isAiProp != null && isAiProp.boolValue;

            var key = $"{i + 1}isAI";
            var intVal = isAi ? 1 : 0;
            PlayerPrefs.SetInt(key, intVal);
            //Debug.Log($"[PieceSpawnerEditor] Set PlayerPrefs '{key}' = {intVal}", context);
        }

        PlayerPrefs.Save();
        //Debug.Log("[PieceSpawnerEditor] PlayerPrefs saved.", context);
    }
}

