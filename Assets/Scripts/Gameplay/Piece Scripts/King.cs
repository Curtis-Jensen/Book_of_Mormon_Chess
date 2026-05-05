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

    /*
        🧩 Get all pieces from all players

        🔍 Check pieces from each faction

        ⏭️ Skip if it's our own piece or if it's an inanimate piece

        🏃 We can return early since we found a check
    */
    public void IsInCheck()
    {
        var kingPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        inCheck = false;

        var pieceSpawner = FindObjectOfType<PieceSpawner>(); // 🧩
        var allPlayers = pieceSpawner.players;

        foreach (var player in allPlayers) // 🔍
        {
            foreach (var piece in player.pieces)
            {
                
                if (piece.faction == faction || piece.faction == Faction.Inanimate) // ⏭️
                    continue;

                if (piece.GetMoves().Contains(kingPosition))
                {
                    Debug.LogError($"Check!");
                    inCheck = true;
                    return; // 🏃
                }
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
                IsTileEmpty(newMove) || IsEnemyPiece(newMove);

            if (emptyOrEnemy)
            {
                validMoves.Add(newMove);
            }
        }

        return validMoves;
    }

    public override void Die(bool instantiateEffects = true)
    {
        var endingManager = FindAnyObjectByType<EndingManager>();
        endingManager.EndGame(playerIndex);

        base.Die();
    }
}
