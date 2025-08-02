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

[CreateAssetMenu(fileName = "SpriteSets", menuName = "Custom/SpriteSets")]
public class SpriteSets : ScriptableObject
{
    public SpriteSet[] spriteSets;
}
