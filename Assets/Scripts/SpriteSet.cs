using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteSet", menuName = "Custom/SpriteSet")]
public class SpriteSet : ScriptableObject
{
    public Sprite King, Queen, Rook, Bishop, Knight, Pawn;
}
