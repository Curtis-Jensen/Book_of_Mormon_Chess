using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class King : Piece
{
    //The four cardinal directions, and diagonal moves, to be multiplied upon
    Vector2Int[] moveDirections =
        { new( 1,  0),
          new(-1,  0),
          new(0,   1),
          new(0,  -1),
          new( 1,  1),
          new(-1, -1),
          new(-1,  1),
          new( 1, -1)};

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        foreach (var moveDirection in moveDirections)
        {
            Vector2Int newMove =
                new((int)transform.position.x + moveDirection.x, (int)transform.position.y + moveDirection.y);

            if (Board.Instance.IsTileEmpty(newMove))
            {
                validMoves.Add(newMove);
                continue;
            }
            else if (Board.Instance.IsEnemyPiece(newMove, isWhite))
            {
                validMoves.Add(newMove);
            }
        }

        return validMoves;
    }
}
