using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "PieceColors", menuName = "Custom/PieceColors")]
public class PieceSets : ScriptableObject
{
    public ColorSet[] colorSets;
    public SpriteSet[] spriteSets;
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
    public float transformScale;
    public Sprite King, Queen, Rook, Bishop, Knight, Pawn;
}