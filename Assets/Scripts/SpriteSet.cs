using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteSet", menuName = "Custom/SpriteSet")]
public class SpriteSet : ScriptableObject
{
    public string setName; [Tooltip("e.g., 'Classic', 'Modern'")]
    public Sprite king, queen, rook, bishop, knight, pawn;
}
