using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    public PieceSets pieceSets;
    public Player[] players;

    TileHoler tileHolder;
    AiManager aiManager;

    void Start()
    {
        tileHolder = GetComponent<TileHoler>();
        aiManager = GetComponent<AiManager>();
    }

    /// <summary>
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
    /// <param name="piecePrefab">The prefab of the chess piece to spawn</param>
    /// <param name="x">The board position (x-coordinate) to spawn the piece </param>
    /// <param name="playerIndex">Index of the player owning the piece</param>
    public Piece SpawnPiece(GameObject piecePrefab, Vector2 position, int playerIndex)
    {
        var player = players[playerIndex]; //🧑🏻
        var pieceInstance =
        Instantiate(piecePrefab, tileHolder.tiles[(int)position.x, (int)position.y].transform); //🏗️
        var spriteRenderer = pieceInstance.GetComponent<SpriteRenderer>();
        var pieceScript = pieceInstance.GetComponent<Piece>(); //🔍

        var spriteSet = pieceSets.spriteSets[PlayerPrefs.GetInt(player.name + "style")]; //🎨

        spriteRenderer.sprite =
            spriteSet.GetType().GetField(piecePrefab.name).GetValue(spriteSet) as Sprite;
        pieceInstance.transform.localScale
            = new Vector3(spriteSet.transformScale, spriteSet.transformScale, 1);

        var colorSelection = PlayerPrefs.GetInt(player.name + "color");//🎨
        if (pieceScript is King)
        {
            spriteRenderer.color = pieceSets.colorSets[colorSelection].kingColor;
        }
        else
        {
            spriteRenderer.color = pieceSets.colorSets[colorSelection].baseColor;
        }

        pieceInstance.name = $"{pieceInstance.name} {player.name} {position.x + 1}";//📛

        pieceScript.teamOne = player.teamOne;//⚖️
        pieceScript.playerIndex = playerIndex;

        if (player.isAi)//🤖
        {
            aiManager.aiPieces.Add(pieceScript);
        }

        return pieceScript;
    }
}
