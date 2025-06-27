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

    private int spawnCount;

    protected override void FinishTurn()
    {
        // Spawn pawn after move
        var spawnPoint = ChooseSpawn();
        SpawnPiece(spawnPoint);
        DisplaySpawnCount();
    }

    private Vector2 ChooseSpawn()
    {
        for (int y = boardSize - 1; y >= 0; y--) // Start at top rank (y=boardSize-1), move down
        {
            List<int> emptyXs = new(); // List to store empty x-coordinates for current y
            for (int x = 0; x < boardSize; x++) // Check each x in current y
{
                if (tiles[x, y].piece == null) // If tile is empty
                {
                    emptyXs.Add(x); // Add x to empty list
                }
            }
            if (emptyXs.Count > 0) // If empty tiles exist in this y
            {
                int randomX = emptyXs[Random.Range(0, emptyXs.Count)]; // Pick random empty x
                return new Vector2(randomX, y); // Return position (x, y)
            }
        }
        Debug.LogError("No empty tiles available for pawn spawn"); // Log error if board is full
        return new Vector2(0, boardSize - 1); // Fallback to (0, top rank), may overwrite
    }

    /// <summary>
    /// Changing things here?  Check Pawn.QueenPromotion() too.
    /// 
    /// 🧑🏻Designate the player for later
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
        Debug.Log(colorSelection);

        spriteRenderer.color = colorSets[colorSelection].baseColor;

        pieceInstance.name = $"{pieceInstance.name} {player.name}";//📛

        pieceScript.teamOne = player.teamOne;//⚖️
        pieceScript.playerIndex = aiIndex;

        aiManager.aiPieces.Add(pieceScript);
    }

    private void DisplaySpawnCount()
    {
        spawnCount++;
        spawnDisplay.text = displayPrefix + spawnCount.ToString();
    }
}
