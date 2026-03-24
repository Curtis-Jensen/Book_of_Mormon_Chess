using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    public PieceSets pieceSets;
    public Player[] players;

    [Header("Victory Dance Properties")]
    [Tooltip("How high the pieces bounce in units")]
    public float pieceBounceHeight = 0.5f;
    [Tooltip("How long each bounce cycle takes in seconds")]
    public float piecebounceDuration = 0.5f;
    [Tooltip("Maximum random delay added between bounces (in seconds)")]
    public float maxBounceDelay = 0.3f;

    TurnManager TurnManager;

    void Awake()
    {
        TurnManager = GetComponent<TurnManager>();

        if (players == null)
        {
            return;
        }

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
            {
                continue;
            }

            players[i].isAi = PlayerPrefs.GetInt($"{i + 1}isAI", 0) == 1;
        }
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
        if (players == null || playerIndex < 0 || playerIndex >= players.Length)
        {
            Debug.LogError($"Invalid player index {playerIndex} for piece spawn.");
            return null;
        }

        int x = (int)position.x;
        int y = (int)position.y;
        if (TurnManager == null || TurnManager.tiles == null || x < 0 || y < 0 ||
            x >= TurnManager.tiles.GetLength(0) || y >= TurnManager.tiles.GetLength(1))
        {
            Debug.LogError($"Invalid board position ({x}, {y}) for piece spawn.");
            return null;
        }

        if (piecePrefab == null)
        {
            Debug.LogError("Attempted to spawn a null piece prefab.");
            return null;
        }

        var player = players[playerIndex]; //🧑🏻
        if (player == null)
        {
            Debug.LogError($"Player at index {playerIndex} is null.");
            return null;
        }

        var pieceInstance =
        Instantiate(piecePrefab, TurnManager.tiles[x, y].transform); //🏗️
        var spriteRenderer = pieceInstance.GetComponent<SpriteRenderer>();
        var pieceScript = pieceInstance.GetComponent<Piece>(); //🔍

        SetSprite(pieceInstance, piecePrefab, player, spriteRenderer); //🎨

        spriteRenderer.color = SetColor(player, pieceScript); //🎨

        pieceInstance.name = $"{pieceInstance.name} {player.name} {position.x + 1}";//📛

        pieceScript.faction = player.faction;//⚖️
        pieceScript.playerIndex = playerIndex;

        pieceScript.boardSize = TurnManager.boardSize;

        SetDanceProperties(pieceScript);

        player.pieces.Add(pieceScript);//⚖️

        TurnManager.tiles[x, y].piece = pieceScript;

        return pieceScript;
    }

    Color SetColor(Player player, Piece pieceScript)
    {
        var colorSelection = PlayerPrefs.GetInt(player.name + "color");//🎨
        if (pieceScript is King)
        {
            return pieceSets.colorSets[colorSelection].kingColor;
        }
        else
        {
            return pieceSets.colorSets[colorSelection].baseColor;
        }
    }

    void SetSprite(GameObject pieceInstance, GameObject piecePrefab, Player player, SpriteRenderer spriteRenderer)
    {
        var styleChoice = PlayerPrefs.GetInt(player.name + "style"); //🎨
        var spriteSet = pieceSets.spriteSets[styleChoice]; //🎨

        spriteRenderer.sprite =
            spriteSet.GetType().GetField(piecePrefab.name).GetValue(spriteSet) as Sprite;

        pieceInstance.transform.localScale
            = new Vector3(spriteSet.transformScale, spriteSet.transformScale, 1);
    }

    void SetDanceProperties(Piece pieceScript)
    {
        pieceScript.bounceHeight = pieceBounceHeight;
        pieceScript.bounceDuration = piecebounceDuration;
        pieceScript.maxBounceDelay = maxBounceDelay;
    }
}
