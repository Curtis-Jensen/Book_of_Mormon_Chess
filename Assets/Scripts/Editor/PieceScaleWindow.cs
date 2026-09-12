using UnityEditor;
using UnityEngine;

// Computes transformScale per PieceStyleOption from the sprite's real pixel size,
// so every piece renders at the same height on the board regardless of what
// resolution the source image came in at. Replaces hand-tuning the scale slider
// per set until it "looks right."
public class PieceScaleWindow : EditorWindow
{
    PieceSets pieceSets;
    float targetHeight = 1.2f;

    [MenuItem("Tools/BOM Chess/Auto Scale Piece Sprites")]
    public static void Open()
    {
        GetWindow<PieceScaleWindow>("Auto Scale Pieces");
    }

    void OnEnable()
    {
        var guids = AssetDatabase.FindAssets("t:PieceSets");
        if (guids.Length > 0)
        {
            pieceSets = AssetDatabase.LoadAssetAtPath<PieceSets>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }

    void OnGUI()
    {
        pieceSets = (PieceSets)EditorGUILayout.ObjectField("Piece Sets", pieceSets, typeof(PieceSets), false);
        targetHeight = EditorGUILayout.FloatField("Target Height (world units)", targetHeight);

        EditorGUILayout.HelpBox(
            "Sets transformScale on every piece style option so all sprites render at the " +
            "same height, based on each sprite's actual pixel size and PPU. No manual resizing.",
            MessageType.Info);

        if (pieceSets == null)
        {
            return;
        }

        if (GUILayout.Button("Auto-Scale All Options"))
        {
            ScaleAll();
        }
    }

    void ScaleAll()
    {
        int count = 0;
        count += ScaleArray(pieceSets.kingOptions);
        count += ScaleArray(pieceSets.queenOptions);
        count += ScaleArray(pieceSets.rookOptions);
        count += ScaleArray(pieceSets.bishopOptions);
        count += ScaleArray(pieceSets.knightOptions);
        count += ScaleArray(pieceSets.pawnOptions);
        count += ScaleArray(pieceSets.striplingWarriorOptions);

        EditorUtility.SetDirty(pieceSets);
        AssetDatabase.SaveAssets();
        Debug.Log($"[PieceScaleWindow] Auto-scaled {count} piece style option(s) to a height of {targetHeight} world units.");
    }

    int ScaleArray(PieceStyleOption[] options)
    {
        if (options == null) return 0;

        int count = 0;
        foreach (var option in options)
        {
            if (option?.sprite == null) continue;

            var spriteHeightInUnits = option.sprite.rect.height / option.sprite.pixelsPerUnit;
            option.transformScale = targetHeight / spriteHeightInUnits;
            count++;
        }
        return count;
    }
}
