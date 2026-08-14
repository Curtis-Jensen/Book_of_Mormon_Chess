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

    TurnProgresser TurnProgresser;

    void Awake()
    {
        TurnProgresser = GetComponent<TurnProgresser>();

        players[0].isAi = PlayerPrefs.GetInt("1isAI") == 1;
        players[1].isAi = PlayerPrefs.GetInt("2isAI") == 1;
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
        Instantiate(piecePrefab, TurnProgresser.tiles[(int)position.x, (int)position.y].transform); //🏗️
        var spriteRenderer = pieceInstance.GetComponent<SpriteRenderer>();
        var pieceScript = pieceInstance.GetComponent<Piece>(); //🔍

        SetSprite(pieceInstance, piecePrefab, player, spriteRenderer); //🎨

        spriteRenderer.color = SetColor(player, pieceScript); //🎨

        pieceInstance.name = $"{pieceInstance.name} {player.name} {position.x + 1}";//📛

        pieceScript.faction = player.faction;//⚖️
        pieceScript.playerIndex = playerIndex;
        pieceScript.prefabName = piecePrefab.name;

        pieceScript.boardSize = TurnProgresser.boardSize;

        SetDanceProperties(pieceScript);

        player.pieces.Add(pieceScript);//⚖️

        TurnProgresser.tiles[(int)position.x, (int)position.y].piece = pieceScript;

        return pieceScript;
    }

    Color SetColor(Player player, Piece pieceScript)
    {
        // SettingsDefaultsSeeder guarantees this key exists once the menu scene has
        // loaded, seeded from the Settings dropdown's own configured defaultValue.
        var colorSelection = PlayerPrefs.GetInt(player.name + "color");//🎨
        Debug.Log($"[PieceSpawner] {player.name}color = {colorSelection} (HasKey: {PlayerPrefs.HasKey(player.name + "color")})");
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
