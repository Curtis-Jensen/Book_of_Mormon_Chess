using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        //If it's white, go up, if it's black, go down
        int forward = isWhite ? 1 : -1;

        // Check one square forward
        Vector2Int forwardMove = 
            new ((int)transform.position.x, (int)transform.position.y + forward);
        if (Board.Instance.IsTileEmpty(forwardMove))
        {
            validMoves.Add(forwardMove);
        }

        // Check diagonal capture moves
        Vector2Int[] diagonalMoves = new Vector2Int[]
        {
            new ((int)transform.position.x - 1, (int)transform.position.y + forward),
            new ((int)transform.position.x + 1, (int)transform.position.y + forward)
        };

        foreach (var move in diagonalMoves)
        {
            if (Board.Instance.IsEnemyPiece(move, isWhite))
            {
                validMoves.Add(move);
            }
        }

        return validMoves;
    }
}
