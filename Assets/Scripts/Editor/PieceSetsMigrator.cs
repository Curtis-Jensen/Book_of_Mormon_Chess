using UnityEditor;
using UnityEngine;

// One-off migration from the old bundled SpriteSet[] (one full 6-piece set per style)
// to the new per-piece-type PieceStyleOption[] arrays on PieceSets, so mixing a King
// from one set with a Queen from another becomes possible. Run this once, check the
// Inspector, then delete this file and the old spriteSets field on PieceSets.
public static class PieceSetsMigrator
{
    [MenuItem("Tools/BOM Chess/Migrate Piece Sets To Per-Piece Styles")]
    public static void Migrate()
    {
        var guids = AssetDatabase.FindAssets("t:PieceSets");
        if (guids.Length == 0)
        {
            Debug.LogError("[PieceSetsMigrator] No PieceSets asset found.");
            return;
        }

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var pieceSets = AssetDatabase.LoadAssetAtPath<PieceSets>(path);
            MigrateAsset(pieceSets);
            EditorUtility.SetDirty(pieceSets);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[PieceSetsMigrator] Migrated {guids.Length} PieceSets asset(s). Check the Inspector before deleting the old spriteSets field.");
    }

    static void MigrateAsset(PieceSets pieceSets)
    {
        int count = pieceSets.spriteSets.Length;

        pieceSets.kingOptions = new PieceStyleOption[count];
        pieceSets.queenOptions = new PieceStyleOption[count];
        pieceSets.rookOptions = new PieceStyleOption[count];
        pieceSets.bishopOptions = new PieceStyleOption[count];
        pieceSets.knightOptions = new PieceStyleOption[count];
        pieceSets.pawnOptions = new PieceStyleOption[count];
        pieceSets.striplingWarriorOptions = new PieceStyleOption[count];

        for (int i = 0; i < count; i++)
        {
            var set = pieceSets.spriteSets[i];

            pieceSets.kingOptions[i] = ToOption(set, set.King);
            pieceSets.queenOptions[i] = ToOption(set, set.Queen);
            pieceSets.rookOptions[i] = ToOption(set, set.Rook);
            pieceSets.bishopOptions[i] = ToOption(set, set.Bishop);
            pieceSets.knightOptions[i] = ToOption(set, set.Knight);
            pieceSets.pawnOptions[i] = ToOption(set, set.Pawn);
            pieceSets.striplingWarriorOptions[i] = ToOption(set, set.StriplingWarrior);
        }
    }

    static PieceStyleOption ToOption(SpriteSet set, Sprite sprite)
    {
        return new PieceStyleOption
        {
            name = set.name,
            transformScale = set.transformScale,
            sprite = sprite
        };
    }
}
