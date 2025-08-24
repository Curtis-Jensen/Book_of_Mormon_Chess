using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HoardBoard : TileHolder
{
    public GameObject pawn;
    public PieceSets pieceSets;
    public TMP_Text spawnDisplay;
    public string displayPrefix;
    public int waveNumber;

    PieceSpawner pieceSpawner;

    void Start()
    {
        pieceSpawner = FindAnyObjectByType<PieceSpawner>();
        NewWave();
    }

    private void NewWave()
    {
        waveNumber++;
        for (int i = 0; i < waveNumber; i++)
        {
            var spawnPoint = ChooseSpawn();
            pieceSpawner.SpawnPiece(pawn, spawnPoint, 1);
            DisplaySpawnCount();
        }
    }

    /// <summary>
    /// Chooses a random empty tile for AI pawn spawning, starting at top rank.
    /// 🆙 Start at top rank (y=boardSize-1), move down
    /// 📋 Create list for empty x-coordinates in current y
    /// 🔲 For each x
    /// ✅ Add x to list if tile is empty
    /// 🎲 Pick random x from empty list if available
    /// 📍 Return position (x, y) of chosen tile
    /// ⚠️ Log error if no empty tiles found
    /// 🔙 Fallback to (0, top rank) if board is full
    /// </summary>
    /// <returns>Vector2 position of the spawn tile</returns>
    private Vector2 ChooseSpawn()
    {
        for (int y = boardSize - 1; y >= 0; y--) // 🆙
        {
            List<int> emptyXs = new(); // 📋
            for (int x = 0; x < boardSize; x++) // 🔲
            {
                if (tiles[x, y].piece == null) // ✅
                {
                    emptyXs.Add(x); // ✅
                }
            }
            if (emptyXs.Count > 0) // 🎲
            {
                int randomX = emptyXs[Random.Range(0, emptyXs.Count)]; // 🎲
                return new Vector2(randomX, y); // 📍
            }
        }
        Debug.LogError("No empty tiles available for pawn spawn"); // ⚠️
        return new Vector2(0, boardSize - 1); // 🔙
    }

    private void DisplaySpawnCount()
    {
        spawnDisplay.text = displayPrefix + waveNumber.ToString();
    }

    protected override void ChangeTurn()
    {
        Debug.Log(aiManager.ChooseMove(playerTurn).moveTo.x);
        if (aiManager.ChooseMove(playerTurn).moveTo.x == -100)
        {
            NewWave();
        }

        base.ChangeTurn();
    }
}
