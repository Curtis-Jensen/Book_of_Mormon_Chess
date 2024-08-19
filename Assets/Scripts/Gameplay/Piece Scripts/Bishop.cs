using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Bishop : Piece
{
    //The four cardinal directions, to be multiplied upon
    Vector2Int[] moveDirections =
        { new Vector2Int( 1,  1),
          new Vector2Int(-1, -1),
          new Vector2Int(-1,  1),
          new Vector2Int( 1, -1)};

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        foreach (var moveDirection in moveDirections)
        {
            for (int distance = 1; distance < Board.Instance.boardSize; distance++)
            {
                var moveDistance = moveDirection * distance;
                Vector2Int newMove =
                    new((int)transform.position.x + moveDistance.x, (int)transform.position.y + moveDistance.y);

                if (Board.Instance.IsTileEmpty(newMove))
                {
                    validMoves.Add(newMove);
                    continue;
                }
                else if (Board.Instance.IsEnemyPiece(newMove, isWhite))
                {
                    validMoves.Add(newMove);
                }

                break;
            }
        }

        return validMoves;
    }
}
