using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

// Hard coded for The Nephites’ Last Stand at the moment
public class HoardEndingManager : MonoBehaviour
{
    public GameObject winScreen;
    [HideInInspector] public bool gameOver = false;
    int[] teamCounts;
    protected PieceSpawner pieceSpawner;

    //Initialized objects
    void Awake()
    {
        pieceSpawner = FindAnyObjectByType<PieceSpawner>();

        teamCounts = new int[2];
        for (int i = 0; i < teamCounts.Length; i++)
        {
            teamCounts[i] = 0;
        }
    }

    //Updates material
    public void ReportSpawn(int playerIndex, int materialValue)
    {
        teamCounts[playerIndex] += materialValue;
    }

    //Updates material
    public void ReportDeath(int playerIndex, int materialValue)
    {
        teamCounts[playerIndex] -= materialValue;
    }

    //More of a method holder method
    public virtual void CheckEnd()
    {
        CheckStalemate();
        CheckExtinction();
    }

    protected void CheckStalemate(int playerIndex = 0)
    {
        // Check if either player has any legal moves
        bool playerHasMoves = false;

        // Check player 1's pieces
        foreach (var piece in pieceSpawner.players[playerIndex].pieces)
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
            EndGame(playerIndex);
        }
    }

    protected void CheckExtinction(int playerIndex = 0)
    {
        if (pieceSpawner.players[playerIndex].pieces.Count <= 0)
        {
            Debug.Log($"Player 1 has lost all their pieces and thus lost the game!");
            EndGame(playerIndex);
        }
    }

    public virtual void EndGame(int playerIndex)
    {
        winScreen.SetActive(true);
        gameOver = true;
    }
}
