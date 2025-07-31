using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SpriteSet : ScriptableObject
{
    public float transformScale;
    public Sprite King, Queen, Rook, Bishop, Knight, Pawn;
}

[CreateAssetMenu(fileName = "SpriteSets", menuName = "Custom/SpriteSets")]
public class SpriteSets : ScriptableObject
{
    public SpriteSet[] spriteSets;
}
