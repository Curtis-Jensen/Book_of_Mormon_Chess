using UnityEditor;

public class BuildScript
{
    public static void BuildWindows()
    {
        BuildPipeline.BuildPlayer(
            new[] { "Assets/Scenes/MainScene.unity" }, // Add all your scenes here
            "Builds/Windows/MyGame.exe",
            BuildTarget.StandaloneWindows64,
            BuildOptions.None
        );
    }
}
