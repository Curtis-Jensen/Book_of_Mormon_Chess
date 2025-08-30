using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Pawn : Piece
{
    public GameObject queenPrefab;

    int endRow;

    override protected void Start()
    {
        if (teamOne)
        {
            endRow = PlayerPrefs.GetInt("boardSize") - 1;
        }
        else
        {
            endRow = 0;
        }

        base.Start();
    }

    void Update()
    {
        if (transform.position.y != endRow) return;

        QueenPromotion();
        Destroy(gameObject);
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
        if (TurnManager.Instance.IsTileEmpty(forwardMove))
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
            if (TurnManager.Instance.IsEnemyPiece(move, teamOne))
            {
                validMoves.Add(move);
            }
        }

        return validMoves;
    }

    public void QueenPromotion()
    {
        var pieceSpawner = FindObjectOfType<PieceSpawner>();
        var queen = pieceSpawner.SpawnPiece(queenPrefab, transform.position, playerIndex);

        var AI = FindObjectOfType<AiManager>();
        AI.aiPieces[playerIndex].Remove(this);

        endingManager.ReportDeath(playerIndex, materialValue);
        
        Debug.LogWarning($"{queen.name} spawned at: {transform.parent.position}");
    }
}
