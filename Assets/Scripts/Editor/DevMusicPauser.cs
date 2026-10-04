using UnityEditor;
using System.Diagnostics;

// -------------------------------------------------------
// DevMusicPauser
// Pauses/resumes YouTube Music in Firefox when Unity
// enters or exits Play Mode. Posts to a local HTTP server
// (dev-music-server.py) which a Tampermonkey userscript
// polls to know when to click the play/pause button.
//
// Can be toggled on/off via Tools > Dev Music.
// Requires: dev-music-server.py running (auto-starts on login).
// -------------------------------------------------------
[InitializeOnLoad]
public static class DevMusicPauser
{
    const string ServerUrl = "http://localhost:9842";
    const string EnabledPrefKey = "DevMusic.Enabled";

    public static bool IsEnabled
    {
        get => EditorPrefs.GetBool(EnabledPrefKey, true);
        set => EditorPrefs.SetBool(EnabledPrefKey, value);
    }

    // -------------------------------------------------------
    // DevMusicPauser() [static constructor]
    // Runs once when the Unity editor loads (or recompiles).
    // Hooks into the play mode state change event so we can
    // react whenever the editor enters or exits Play Mode.
    // -------------------------------------------------------
    static DevMusicPauser()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    // -------------------------------------------------------
    // OnPlayModeChanged(state)
    // Fires on every play mode transition.
    // - ExitingEditMode: user just hit Play, tell server to pause
    // - EnteredEditMode: user stopped playing, tell server to play
    // Bails out early if the feature is disabled via the toggle.
    // -------------------------------------------------------
    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!IsEnabled) return;

        if (state == PlayModeStateChange.ExitingEditMode)
            PostToServer("/pause");
        else if (state == PlayModeStateChange.EnteredEditMode)
            PostToServer("/play");
    }

    // -------------------------------------------------------
    // PostToServer(endpoint)
    // Fires a curl POST to the local music server as a
    // fire-and-forget process. Fails silently if the server
    // isn't running -- music just won't pause, no crash.
    // TECH DEBT: Silent failure makes this hard to debug.
    // Propose: log a warning if curl exits non-zero, when revisiting.
    // -------------------------------------------------------
    public static void PostToServer(string endpoint)
    {
        var process = new Process();
        process.StartInfo.FileName = "curl";
        process.StartInfo.Arguments = $"-s -X POST {ServerUrl}{endpoint}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
    }
}
