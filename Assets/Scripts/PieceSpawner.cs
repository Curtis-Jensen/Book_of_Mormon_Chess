using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteSet
{
    public string name;
    public float transformScale;
    public Sprite King, Queen, Rook, Bishop, Knight, Pawn;
}

[System.Serializable]
public class ColorSet
{
    public Color baseColor;
    public Color kingColor;
}

public class PieceSpawner : MonoBehaviour
{
    public SpriteSet[] spriteSets;
    public ColorSet[] colorSets;

    TileHolder tileHolder; // Reference to TileHolder instance
    int pieceNumber = 0; // Counter for piece names

    void Awake()
    {
        tileHolder = FindAnyObjectByType<TileHolder>(); // Get the TileHolder instance
    }

    /// <summary>
    /// Spawns a piece at the given position, assigns it to the tile, and registers with AI if needed.
    /// </summary>
    public Piece SpawnPiece(GameObject piecePrefab, Vector2 position, int playerIndex, bool isAi)
    {
        tileHolder = FindAnyObjectByType<TileHolder>();

        if (tileHolder == null)  Debug.LogError("TileHolder instance not found!");
        if (tileHolder.tiles == null)  Debug.LogError("The tile object is null!");

        var tile = tileHolder.tiles[(int)position.x, (int)position.y];
        var pieceObj = Instantiate(piecePrefab, tile.transform.position, Quaternion.identity);
        var piece = pieceObj.GetComponent<Piece>();
        var spriteRenderer = piece.GetComponent<SpriteRenderer>();

        piece.playerIndex = playerIndex;
        piece.teamOne = playerIndex == 0; // Adjust as needed

        pieceObj.name = $"{pieceObj.name} player{playerIndex} {pieceNumber++}";//📛

        var spriteNum = PlayerPrefs.GetInt(tileHolder.players[playerIndex].name + "skin");

        if(spriteSets.Length == 0) Debug.LogError("No sprite sets available!?");

        var spriteSet = spriteSets[spriteNum];
        Debug.Log($"Using sprite set: {spriteSet.name} for player {playerIndex}");
        spriteRenderer.sprite =
            spriteSet.GetType().GetField(piecePrefab.name).GetValue(spriteSet) as Sprite;
        piece.transform.localScale
            = new Vector3(spriteSet.transformScale, spriteSet.transformScale, 1);

        // Get color selection index for this player
        var colorSelection = PlayerPrefs.GetInt(tileHolder.players[playerIndex].name + "color");

        // Get the correct ColorSet from the PieceColors ScriptableObject
        var colorSet = colorSets[colorSelection];

        // Assign color based on piece type
        if (piece is King)
        {
            spriteRenderer.color = colorSet.kingColor;
        }
        else
        {
            spriteRenderer.color = colorSet.baseColor;
        }

        tile.piece = piece;
        piece.transform.parent = tile.transform;

        if (isAi)
        {
            var aiManager = FindAnyObjectByType<AiManager>();
            aiManager.aiPieces.Add(piece);
        }

        Debug.Log($"Spawning piece: {piecePrefab.name} at position: {position} for player: {playerIndex}, AI: {isAi}");

        return piece;
    }
}