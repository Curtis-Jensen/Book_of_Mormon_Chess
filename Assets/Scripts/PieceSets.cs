using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct SpriteSet
{
    public string name;
    public float transformScale;
    public Sprite King, Queen, Rook, Bishop, Knight, Pawn;
}

[System.Serializable]
public struct ColorSet
{
    public Color baseColor;
    public Color kingColor;
}

//[CreateAssetMenu(fileName = "SpriteSets", menuName = "Custom/SpriteSets")]
public class PieceSets : MonoBehaviour
{
    public SpriteSet[] spriteSets;
    public ColorSet[] colorSets;
}
