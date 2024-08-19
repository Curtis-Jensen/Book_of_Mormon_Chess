using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    //The four cardinal directions, to be multiplied upon
    Vector3Int[] moveDirections =
        { new Vector3Int( 1, 0, 0), 
          new Vector3Int(-1, 0, 0),
          new Vector3Int(0,  1, 0),
          new Vector3Int(0, -1, 0)};

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        Vector2Int forwardMove =
            new((int)transform.position.x, (int)transform.position.y + 1);
        if (Board.Instance.IsTileEmpty(forwardMove) || Board.Instance.IsEnemyPiece(forwardMove, isWhite))
        {
            validMoves.Add(forwardMove);
        }
        else
        {
            return null;
        }
        //Go in every direction (1,0), (-1,0), (0,1), (0,-1)
        //Check every square, as large as the board
        //Cannot go past other pieces

        return validMoves;
    }
}
