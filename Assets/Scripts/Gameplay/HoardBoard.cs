using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HoardBoard : Board
{
    public GameObject pawn;
    public SpriteSet spriteSet;
    public ColorSet[] colorSets;
    public TMP_Text spawnDisplay;
    public string displayPrefix;

    public int waveNumber;

    private void Start()
    {
        NewWave();
    }

    private void NewWave()
    {
        waveNumber++;
        for (int i = 0; i < waveNumber; i++)
        {
            var spawnPoint = ChooseSpawn();
            SpawnPiece(spawnPoint);
            DisplaySpawnCount();
        }
    }

    /// <summary>
    /// Chooses a random empty tile for AI pawn spawning, starting at top rank.
    /// 🆙 Start at top rank (y=boardSize-1), move down
    /// 📋 Create list for empty x-coordinates in current y
    /// 🔲 Check each x for empty tiles
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

    /// <summary>
    /// Changing things here?  Check Pawn.QueenPromotion() too.
    /// 
    /// 🧑🏻 Designate the player for later
    /// 
    /// 🎨 Color the piece.  If it's a king, use the special king color
    /// 
    /// 🏗️ Instantiate the piece prefab at the specified tile 
    /// 
    /// 🔍 Retrieve the Piece component for configuration
    /// 
    /// 📛 Assign a descriptive name to the piece GameObject 
    /// 
    /// ⚖️ Set piece properties for team and player ownership  
    /// 
    /// 🤖 Register the piece with AI manager if player is AI 
    /// </summary>
    private void SpawnPiece(Vector2 spawnPoint)
    {
        var aiIndex = 1;
        var player = players[aiIndex]; //🧑🏻
        var pieceInstance =
        Instantiate(pawn, tiles[(int)spawnPoint.x, (int)spawnPoint.y].transform); //🏗️
        var spriteRenderer = pieceInstance.GetComponent<SpriteRenderer>();
        var pieceScript = pieceInstance.GetComponent<Pawn>(); //🔍
        pieceScript.queenSprite = spriteSet.GetType().GetField("Queen").GetValue(spriteSet) as Sprite;
        tiles[(int)spawnPoint.x, (int)spawnPoint.y].GetComponent<Tile>().piece = pieceScript;


        spriteRenderer.sprite =
            spriteSet.GetType().GetField(pawn.name).GetValue(spriteSet) as Sprite;
        pieceInstance.transform.localScale
            = new Vector3(spriteSet.transformScale, spriteSet.transformScale, 1);

        var colorSelection = PlayerPrefs.GetInt(player.name + "color");//🎨

        spriteRenderer.color = colorSets[colorSelection].baseColor;

        pieceInstance.name = $"{pieceInstance.name} {player.name}";//📛

        pieceScript.teamOne = player.teamOne;//⚖️
        pieceScript.playerIndex = aiIndex;

        aiManager.aiPieces.Add(pieceScript);
    }

    private void DisplaySpawnCount()
    {
        spawnDisplay.text = displayPrefix + waveNumber.ToString();
    }

    protected override void ChangeTurn()
    {
        if(aiManager.ChooseMove().moveTo.x == -100)
        {
            NewWave();
        }

        base.ChangeTurn();
    }
}
