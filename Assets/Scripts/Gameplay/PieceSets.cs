using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "PieceColors", menuName = "Custom/PieceColors")]
public class PieceSets : ScriptableObject
{
    public ColorSet[] colorSets;

    // Old bundled-set data. Kept around only so the migration tool
    // (Tools/BOM Chess/Migrate Piece Sets To Per-Piece Styles) has something to read
    // from -- delete this field and the migration tool once the new per-type arrays
    // below are confirmed correct in the Inspector.
    public SpriteSet[] spriteSets;

    // New data: one option list per piece type, so a player can mix a King from one
    // set with a Queen from another instead of picking one SpriteSet for everything.
    public PieceStyleOption[] kingOptions;
    public PieceStyleOption[] queenOptions;
    public PieceStyleOption[] rookOptions;
    public PieceStyleOption[] bishopOptions;
    public PieceStyleOption[] knightOptions;
    public PieceStyleOption[] pawnOptions;
    public PieceStyleOption[] striplingWarriorOptions;

    public PieceStyleOption[] GetOptions(string pieceTypeName)
    {
        switch (pieceTypeName)
        {
            case "King": return kingOptions;
            case "Queen": return queenOptions;
            case "Rook": return rookOptions;
            case "Bishop": return bishopOptions;
            case "Knight": return knightOptions;
            case "Pawn": return pawnOptions;
            case "StriplingWarrior": return striplingWarriorOptions;
            default: throw new System.ArgumentException($"No piece style options for '{pieceTypeName}'");
        }
    }
}

[System.Serializable]
public class PieceStyleOption
{
    public string name;
    public float transformScale;
    public Sprite sprite;
}

[System.Serializable]
public class ColorSet
{
    public string name;
    public Color baseColor;
    public Color kingColor;
}

//[CreateAssetMenu(fileName = "SpriteSet", menuName = "Custom/SpriteSet")]
[System.Serializable]
public class SpriteSet// : ScriptableObject
{
    public string name;
    public float transformScale;
    public Sprite King, Queen, Rook, Bishop, Knight, Pawn, StriplingWarrior;
}