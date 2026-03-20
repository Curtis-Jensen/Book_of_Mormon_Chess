using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using TMPro;
using UnityEngine.UI;

// 🚨TECH DEBT TODO🚨: Rename EndingManager to ClassicEndingManager and HoardEndingManager to EndingManager!🚨
public class HoardEndingManager : MonoBehaviour
{
    // Delegate and event for game ending
    public delegate void GameEndHandler(int playerIndex);
    public static event GameEndHandler OnGameEnd;

    public TextMeshProUGUI materialCountText;

    public Color tieColor;
    public Color nephiteWinColor;
    public Color lamaniteWinColor;

    public Slider[] materialSliders = new Slider[2];

    public GameObject winScreen;
    [HideInInspector] public bool gameOver = false;
    protected PieceSpawner pieceSpawner;

    //Initialized objects
    void Awake()
    {
        pieceSpawner = FindAnyObjectByType<PieceSpawner>();
    }

    //More of a method holder method.  Hardcoded for the first (0) player moving because only the Nephites can lose.
    public virtual void CheckEnd()
    {
        if (CheckNoMoves()) EndGame(0);
    }

    public bool CheckNoMoves(int playerIndex = 0)
    {
        // Check if either player has any legal moves
        bool playerHasMoves = false;

        // Check player's pieces
        foreach (var piece in pieceSpawner.players[playerIndex].pieces)
        {
            var moves = piece.GetMoves();

            if (moves.Count > 0)
            {
                playerHasMoves = true;
                break;
            }
        }

        return !playerHasMoves;
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
              $"Game Mode: {PlayerPrefs.GetString("gameMode")}");
    }
}
