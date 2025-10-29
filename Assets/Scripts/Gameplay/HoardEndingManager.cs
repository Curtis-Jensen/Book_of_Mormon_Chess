using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

// Hard coded for The Nephites’ Last Stand at the moment
public class HoardEndingManager : MonoBehaviour
{
    // Delegate and event for game ending
    public delegate void GameEndHandler(int playerIndex);
    public static event GameEndHandler OnGameEnd;

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
        CheckNoMoves();
    }

    protected void CheckNoMoves(int playerIndex = 0)
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
            EndGame(playerIndex);
        }
    }

    public virtual void EndGame(int playerIndex)
    {
        EndGameLogging();

        winScreen.SetActive(true);
        gameOver = true;

        // Invoke the event with the player index
        OnGameEnd?.Invoke(playerIndex);
    }

    void EndGameLogging()
    {
        Debug.Log($"Game Over! Stats:\n" +
              $"Nephite Pieces: {pieceSpawner.players[0].pieces.Count}\n" + 
              $"Lamanite Pieces: {pieceSpawner.players[1].pieces.Count}\n" +
              $"Board Size: {PlayerPrefs.GetInt("boardSize")}\n" +
              $"Game Mode: {PlayerPrefs.GetString("gameMode")}\n" +
              $"Material Count - Nephites: {teamCounts[0]}, Lamanites: {teamCounts[1]}");
    }
}
