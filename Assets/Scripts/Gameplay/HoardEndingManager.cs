using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Hard coded for The Nephites’ Last Stand at the moment
public class HoardEndingManager : MonoBehaviour
{
    public GameObject winScreen;

    int[] teamCounts;
    PieceSpawner pieceSpawner;

    void Awake()
    {
        pieceSpawner = FindAnyObjectByType<PieceSpawner>();

        teamCounts = new int[2];
        for (int i = 0; i < teamCounts.Length; i++)
        {
            teamCounts[i] = 0;
        }
    }

    public void ReportSpawn(int playerIndex, int materialValue)
    {
        teamCounts[playerIndex] += materialValue;
    }

    public void ReportDeath(int playerIndex, int materialValue)
    {
        teamCounts[playerIndex] -= materialValue;
    }
    
    public void CheckEnd()
    {
        CheckStalemate();
        CheckExtinction();
    }

    void CheckStalemate()
    {
        // Check if either player has any legal moves
        bool playerHasMoves = false;

        // Check player 1's pieces
        foreach (var piece in pieceSpawner.players[0].pieces)
        {
            if (piece.GetMoves().Count > 0)
            {
                playerHasMoves = true;
                break;
            }
        }
        // If either player has no legal moves, it's a stalemate
        if (!playerHasMoves)
        {
            Debug.Log("Stalemate detected - one player has no legal moves!");
            EndGame();
        }
    }

    void CheckExtinction()
    {
        if (pieceSpawner.players[0].pieces.Count <= 0)
        {
            Debug.Log($"Player 1 has lost all their pieces and thus lost the game!");
            EndGame();
        }
    }

    public void EndGame()
    {
        winScreen.SetActive(true);
    }
}
