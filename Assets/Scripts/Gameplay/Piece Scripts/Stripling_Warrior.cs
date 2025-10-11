using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class Stripling_Warrior : Piece
{
    //The four cardinal directions, and diagonal moves
    Vector2Int[] moveDirections =
        { new( 1,  0),
          new(-1,  0),
          new(0,   1),
          new(0,  -1),
          new( 1,  1),
          new(-1, -1),
          new(-1,  1),
          new( 1, -1) };

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        foreach (var moveDirection in moveDirections)
        {
            Vector2Int newMove =
                new((int)transform.position.x + moveDirection.x, (int)transform.position.y + moveDirection.y);

            bool emptyOrEnemy =
                TurnManager.Instance.IsTileEmpty(newMove) || TurnManager.Instance.IsEnemyPiece(newMove, teamOne);

            if (emptyOrEnemy)
            {
                validMoves.Add(newMove);
            }
        }

        return validMoves;
    }

    public override void Die()
    {
        TurnManager.Instance.selectedPiece.Die();
    }
}
