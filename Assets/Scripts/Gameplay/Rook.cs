using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    //The four cardinal directions, to be multiplied upon
    Vector2Int[] moveDirections =
        { new Vector2Int( 1, 0), 
          new Vector2Int(-1, 0),
          new Vector2Int(0,  1),
          new Vector2Int(0, -1)};

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        foreach (var moveDirection in moveDirections)
        {
            Vector2Int newMove =
                new((int)transform.position.x + moveDirection.x, (int)transform.position.y + moveDirection.y);

            if (Board.Instance.IsTileEmpty(newMove) || Board.Instance.IsEnemyPiece(newMove, isWhite))
            {
                validMoves.Add(newMove);
            }
        }
        //Go in every direction (1,0), (-1,0), (0,1), (0,-1)
        //Check every square, as large as the board
        //Cannot go past other pieces

        return validMoves;
    }
}
