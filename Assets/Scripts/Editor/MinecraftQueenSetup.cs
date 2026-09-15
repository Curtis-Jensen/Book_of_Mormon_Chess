using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// One-off setup for the animated Minecraft End Crystal Queen: slices the
// sprite sheet, builds the spin animation, and registers it as a Queen
// style option. Run via -executeMethod MinecraftQueenSetup.Run.
public static class MinecraftQueenSetup
{
    const string SheetPath = "Assets/Prefabs/Pieces/Secular Pieces/end_crystal.png";
    const string ClipPath = "Assets/Prefabs/Pieces/Secular Pieces/EndCrystal_Spin.anim";
    const string ControllerPath = "Assets/Prefabs/Pieces/Secular Pieces/EndCrystal.controller";
    const int Columns = 12;
    const int Rows = 10;
    const int FrameWidth = 150;
    const int FrameHeight = 160;
    const float FrameRate = 30f;
    const float TargetHeight = 1.2f; // matches PieceScaleWindow's default

    [MenuItem("Tools/BOM Chess/Setup Minecraft Queen (End Crystal)")]
    public static void Run()
    {
        var sprites = SliceSheet();
        var clip = BuildClip(sprites);
        var controller = BuildController(clip);
        RegisterQueenOption(sprites[0], controller);

        Debug.Log($"[MinecraftQueenSetup] Done: {sprites.Length} frames sliced, clip/controller built, Queen option registered.");
    }

    static Sprite[] SliceSheet()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(SheetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;

        var metas = new SpriteMetaData[Columns * Rows];
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                int frame = row * Columns + col;
                metas[frame] = new SpriteMetaData
                {
                    name = $"end_crystal_{frame}",
                    rect = new Rect(col * FrameWidth, (Rows - 1 - row) * FrameHeight, FrameWidth, FrameHeight),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                };
            }
        }
        importer.spritesheet = metas;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAllAssetsAtPath(SheetPath)
            .OfType<Sprite>()
            .OrderBy(s => int.Parse(s.name.Substring("end_crystal_".Length)))
            .ToArray();
    }

    static AnimationClip BuildClip(Sprite[] sprites)
    {
        var clip = new AnimationClip { frameRate = FrameRate };
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        var binding = new EditorCurveBinding
        {
            path = "",
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite",
        };
        var keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe { time = i / FrameRate, value = sprites[i] };
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AssetDatabase.CreateAsset(clip, ClipPath);
        return clip;
    }

    static AnimatorController BuildController(AnimationClip clip)
    {
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddMotion(clip);
        return controller;
    }

    static void RegisterQueenOption(Sprite firstFrame, AnimatorController controller)
    {
        var pieceSetsGuid = AssetDatabase.FindAssets("t:PieceSets").First();
        var pieceSets = AssetDatabase.LoadAssetAtPath<PieceSets>(AssetDatabase.GUIDToAssetPath(pieceSetsGuid));

        var so = new SerializedObject(pieceSets);
        var queenOptions = so.FindProperty("queenOptions");

        int index = queenOptions.arraySize;
        for (int i = 0; i < queenOptions.arraySize; i++)
        {
            if (queenOptions.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == "Minecraft")
            {
                index = i;
                break;
            }
        }
        if (index == queenOptions.arraySize) queenOptions.arraySize++;

        var option = queenOptions.GetArrayElementAtIndex(index);
        option.FindPropertyRelative("name").stringValue = "Minecraft";
        option.FindPropertyRelative("sprite").objectReferenceValue = firstFrame;
        option.FindPropertyRelative("animatorController").objectReferenceValue = controller;

        var spriteHeightInUnits = firstFrame.rect.height / firstFrame.pixelsPerUnit;
        option.FindPropertyRelative("transformScale").floatValue = TargetHeight / spriteHeightInUnits;

        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }
}
