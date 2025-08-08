using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    //The four cardinal directions, to be multiplied upon
    Vector2Int[] moveDirections =
        { new( 1,  0), 
          new(-1,  0),
          new(0,   1),
          new(0,  -1)};

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        foreach (var moveDirection in moveDirections)
        {
            for (int distance = 1; distance < TileHoler.Instance.boardSize; distance++)
            {
                var moveDistance = moveDirection * distance;
                Vector2Int newMove =
                    new((int)transform.position.x + moveDistance.x, (int)transform.position.y + moveDistance.y);

                if (TileHoler.Instance.IsTileEmpty(newMove))
                {
                    validMoves.Add(newMove);
                    continue;
                }
                else if (TileHoler.Instance.IsEnemyPiece(newMove, teamOne))
                {
                    validMoves.Add(newMove);
                }

                break;
            }
        }

        return validMoves;
    }
}
