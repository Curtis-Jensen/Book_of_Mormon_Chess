using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public bool hasMoved;

    // Override the Move method
    public override void Move(Vector3 newPosition)
    {
        // Pawn-specific move logic
        base.Move(newPosition);  // Call the base class's move method, if needed
        hasMoved = true;
    }
}
