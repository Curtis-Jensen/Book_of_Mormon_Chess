using UnityEditor;
using UnityEngine;

// -------------------------------------------------------
// DevMusicWindow
// Small editor window accessible via Tools > Dev Music.
// Lets you toggle the play-mode music pausing on or off
// without touching any code -- handy when you want to
// keep Firefox audio going during a play session, or
// silence everything without server involvement.
// -------------------------------------------------------
public class DevMusicWindow : EditorWindow
{
    // -------------------------------------------------------
    // ShowWindow()
    // Opens (or focuses) the Dev Music window.
    // Registered as a menu item under Tools.
    // -------------------------------------------------------
    [MenuItem("Tools/Dev Music")]
    public static void ShowWindow()
    {
        var window = GetWindow<DevMusicWindow>("Dev Music");
        window.minSize = new Vector2(220, 80);
        window.maxSize = new Vector2(220, 80);
    }

    // -------------------------------------------------------
    // OnGUI()
    // Draws the toggle and a manual resume button.
    // Called by Unity whenever the window needs to repaint.
    // -------------------------------------------------------
    void OnGUI()
    {
        EditorGUILayout.Space(8);

        bool wasEnabled = DevMusicPauser.IsEnabled;
        bool isEnabled = EditorGUILayout.Toggle("Pause music during Play", wasEnabled);

        if (isEnabled != wasEnabled)
        {
            DevMusicPauser.IsEnabled = isEnabled;
            // If turning off while in play mode, resume music immediately
            if (!isEnabled && EditorApplication.isPlaying)
                DevMusicPauser.PostToServer("/play");
        }

        EditorGUILayout.Space(4);

        if (GUILayout.Button("Resume music now"))
            DevMusicPauser.PostToServer("/play");
    }
}
