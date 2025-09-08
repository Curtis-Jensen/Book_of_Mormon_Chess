using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Hard coded for The Nephites’ Last Stand at the moment
public class HoardEndingManager : MonoBehaviour
{
    public GameObject winScreen;

    int[] teamCounts;

    void Awake()
    {
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
        var pieceSpawner = FindAnyObjectByType<PieceSpawner>();

        // Check if either player has any legal moves
        bool player1HasMoves = false;

        // Check player 1's pieces
        foreach (var piece in pieceSpawner.players[0].pieces)
        {
            if (piece.GetMoves().Count > 0)
            {
                player1HasMoves = true;
                break;
            }
        }
        // If either player has no legal moves, it's a stalemate
        if (!player1HasMoves)
        {
            Debug.Log("Stalemate detected - one player has no legal moves!");
            EndGame();
        }

        //Check if all pieces are gone
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
