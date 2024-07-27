using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();

        //If it's white, go up, if it's black, go down
        int direction = isWhite ? 1 : -1;
        // Check one square forward
        Vector2Int forwardMove = new Vector2Int((int)transform.position.x, (int)transform.position.y + direction);
        validMoves.Add(forwardMove);

        return validMoves;
    }
}
