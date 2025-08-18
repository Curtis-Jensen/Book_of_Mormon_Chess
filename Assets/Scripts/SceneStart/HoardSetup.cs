using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class HoardSetup : ResizableSetup
{
    List<GameObject> tiles =  new();
    
    void ArrangePieces(int[] pieceChoices)
    {
        var topRightTile = tiles.Count - 1;

        ArrangeBackRows (topRightTile, pieceChoices);

        ArrangePawns(topRightTile);
    }

    void ArrangeBackRows(int topRightTile, int[] pieceChoices)
    {
        var playerIndex = 0;
        spriteSet = pieceSets.spriteSets[PlayerPrefs.GetInt(players[playerIndex].name + "skin")];
        for (int x = 0; x < boardSize; x++)
        {
            SpawnPiece(backPiecePrefabs[pieceChoices[x]], x, playerIndex);
        }
    }
    
    void ArrangePawns(int topRightTile)
    {
        var playerIndex = 0;
        spriteSet = pieceSets.spriteSets[PlayerPrefs.GetInt(players[playerIndex].name + "skin")];
        for (int x = boardSize; x < boardSize + boardSize; x++)
        {
            Pawn pawnInstance = (Pawn)SpawnPiece(pawn, x, playerIndex);
            pawnInstance.queenSprite = spriteSet.GetType().GetField("Queen").GetValue(spriteSet) as Sprite;
        }
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
    /// <param name="piecePrefab">The prefab of the chess piece to spawn</param>
    /// <param name="x">The board position (x-coordinate) to spawn the piece </param>
    /// <param name="playerIndex">Index of the player owning the piece</param>
    virtual Piece SpawnPiece(GameObject piecePrefab, int x, int playerIndex)
    {
        var player = players[playerIndex]; //🧑🏻
        var pieceInstance =
        Instantiate(piecePrefab, tiles[x].transform); //🏗️
        var spriteRenderer = pieceInstance.GetComponent<SpriteRenderer>();
        var pieceScript = pieceInstance.GetComponent<Piece>(); //🔍

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

        pieceInstance.name = $"{pieceInstance.name} {player.name} {x + 1}";//📛

        pieceScript.teamOne = player.teamOne;//⚖️
        pieceScript.playerIndex = playerIndex;

        if (player.isAi)//🤖
        {
            aiManager.aiPieces.Add(pieceScript);
        }

        return pieceScript;
    }
}
