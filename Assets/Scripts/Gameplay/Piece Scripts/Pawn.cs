using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 🚨TECH DEBT TODO🚨: Partially uses Piece.Die() for queening!
// 🚨TECH DEBT TODO🚨: Queening unit test!
public class Pawn : Piece
{
    public GameObject queenPrefab;

    int endRow;

    override protected void Start()
    {
        if (faction == Faction.Nephite)
        {
            endRow = PlayerPrefs.GetInt("boardSize", 7) - 1;
        }
        else
        {
            endRow = 0;
        }

        base.Start();
    }

    public override void MoveEnd()
    {
        base.MoveEnd();
        if (transform.position.y != endRow) return;

        QueenPromotion();
    }

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        int forward = 1;
        //If a Lamanie, go down, otherwise, go up
        if (faction == Faction.Lamanite)
        {
            forward = -1;
        }

        validMoves = GetForwardMoves(validMoves, forward);

        if (!firstTurnTaken && validMoves.Count != 0)
        {
            validMoves = GetForwardMoves(validMoves, forward * 2);
        }

        validMoves = GetDiagonalMoves(validMoves, forward);

        return validMoves;
    }

    List<Vector2Int> GetForwardMoves(List<Vector2Int> validMoves, int forward)
    {
        // Check one square forward
        Vector2Int forwardMove =
            new((int)transform.position.x, (int)transform.position.y + forward);
        if (IsTileEmpty(forwardMove))
        {
            validMoves.Add(forwardMove);
        }

        return validMoves;
    }

    List<Vector2Int> GetDiagonalMoves(List<Vector2Int> validMoves, int forward)
    {
        // Check diagonal capture moves
        Vector2Int[] diagonalMoves = new Vector2Int[]
        {
            new ((int)transform.position.x - 1, (int)transform.position.y + forward),
            new ((int)transform.position.x + 1, (int)transform.position.y + forward)
        };

        foreach (var move in diagonalMoves)
        {
            if (IsEnemyPiece(move))
            {
                validMoves.Add(move);
            }
        }

        return validMoves;
    }

    // ⚔️ Pawns threaten diagonals regardless of whether an enemy is there —
    // so we declare threats from attack squares only, not forward walking squares
    public override void DeclareThreats()
    {
        int forward = faction == Faction.Lamanite ? -1 : 1;

        Vector2Int[] attackSquares =
        {
            new((int)transform.position.x - 1, (int)transform.position.y + forward),
            new((int)transform.position.x + 1, (int)transform.position.y + forward)
        };

        foreach (var position in attackSquares)
        {
            if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSize) continue;

            var tile = TurnManager.Instance.tiles[position.x, position.y];
            tile.GetComponent<TileThreats>().threatenedBy.Add(this);
        }
    }

    public void QueenPromotion()
    {
        var pieceSpawner = FindObjectOfType<PieceSpawner>();
        pieceSpawner.SpawnPiece(queenPrefab, transform.position, playerIndex);

        Debug.LogWarning($"{name} spawned at: {transform.position}");

        Die(instantiateEffects: false);
    }
}
