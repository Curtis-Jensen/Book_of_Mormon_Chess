using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteSet", menuName = "Custom/SpriteSet")]
public class SpriteSet : ScriptableObject
{
    public float transformScale;
    public Sprite King, Queen, Rook, Bishop, Knight, Pawn;
}
