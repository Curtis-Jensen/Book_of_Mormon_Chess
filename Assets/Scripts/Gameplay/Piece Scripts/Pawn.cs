using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Pawn : Piece
{
    public GameObject queenPrefab;
    public Sprite queenSprite;

    void Update()
    {
        var teamOneEnd = transform.position.y == TileHolder.Instance.boardSize - 1 && teamOne;
        var teamTwoEnd = transform.position.y == 0 && !teamOne;
        //If the end has been reached
        if (teamTwoEnd || teamOneEnd)
        {
            QueenPromotion();
            Destroy(gameObject);
        }
    }

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        //If it's light, go up, if it's dark, go down
        int forward = teamOne ? 1 : -1;

        validMoves = GetForwardMoves (validMoves, forward);

        if(!firstTurnTaken && validMoves.Count != 0)
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
        if (TileHolder.Instance.IsTileEmpty(forwardMove))
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
            if (TileHolder.Instance.IsEnemyPiece(move, teamOne))
            {
                validMoves.Add(move);
            }
        }

        return validMoves;
    }

    public void QueenPromotion()
    {
        // Get the tile position of this pawn
        Vector2Int position = new((int)transform.position.x, (int)transform.position.y);

        // Remove pawn from AI manager if present
        AiManager.Instance.aiPieces.Remove(this);

        // Destroy the pawn GameObject
        Destroy(gameObject);

        // Spawn the queen using the Spawner
        Piece queen = PieceSpawner.Instance.SpawnPiece(
            queenPrefab,
            position,
            playerIndex,
            AiManager.Instance.PiecesExist() && teamOne == AiManager.Instance.aiPieces[0].teamOne,
            queenSprite,
            queenColor
        );
    }
}
