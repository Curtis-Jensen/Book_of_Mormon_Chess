#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class RequiredAttributeValidator : Editor
{
    public override void OnInspectorGUI()
    {
        var targetType = target.GetType();
        var fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            if (field.GetCustomAttribute<SerializeField>() != null || field.IsPublic)
            {
                var value = field.GetValue(target);
                if (value == null)
                {
                    EditorGUILayout.HelpBox($"Required field '{field.Name}' is null on {targetType.Name}", MessageType.Error);
                }
            }
        }

        base.OnInspectorGUI();
    }
}
#endif