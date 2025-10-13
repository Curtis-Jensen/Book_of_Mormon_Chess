using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class King : Piece
{
    public bool inCheck;

    //The four cardinal directions, and diagonal moves
    Vector2Int[] moveDirections =
        { new( 1,  0),
          new(-1,  0),
          new(0,   1),
          new(0,  -1),
          new( 1,  1),
          new(-1, -1),
          new(-1,  1),
          new( 1, -1)};

    protected override void Start()
    {
        TurnManager.Instance.OnMoveEnd += IsInCheck;

        base.Start();
    }

    public void IsInCheck()
    {
        var kingPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        inCheck = false;
        // Check all enemy pieces to see if they can attack the king
        var enemyPieces = FindObjectOfType<PieceSpawner>().players[teamOne ? 1 : 0].pieces;
        foreach (var enemyPiece in enemyPieces)
        {
            if (enemyPiece.GetMoves().Contains(kingPosition))
            {
                Debug.Log($"Check!");
                inCheck = true;
            }
        }
    }

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
        var endingManager = FindAnyObjectByType<EndingManager>();
        endingManager.EndGame(teamOne ? 0 : 1);

        base.Die();
    }
}
