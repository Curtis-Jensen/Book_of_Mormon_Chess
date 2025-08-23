using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Knight : Piece
{
    //The four cardinal directions, and diagonal moves, to be multiplied upon
    Vector2Int[] moveDirections =
        { new( 2, 1),
          new(-2, 1),
          new( 2,-1),
          new(-2,-1),
          new( 1, 2),
          new(-1, 2),
          new( 1,-2),
          new(-1,-2)};

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        foreach (var moveDirection in moveDirections)
        {
            Vector2Int newMove =
                new((int)transform.position.x + moveDirection.x, (int)transform.position.y + moveDirection.y);

            bool emptyOrEnemy =
                TileHolder.Instance.IsTileEmpty(newMove) || TileHolder.Instance.IsEnemyPiece(newMove, teamOne);

            if (emptyOrEnemy)
            {
                validMoves.Add(newMove);
            }
        }

        return validMoves;
    }
}