using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    public static PieceSpawner Instance { get; set; }
    public PieceSets pieceSets;

    int pieceNumber = 0; // Counter for piece names

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Spawns a piece at the given position, assigns it to the tile, and registers with AI if needed.
    /// </summary>
    public Piece SpawnPiece(GameObject piecePrefab, Vector2 position, int playerIndex, bool isAi)
    {
        var tile = TileHolder.Instance.tiles[(int)position.x, (int)position.y];
        var pieceObj = Instantiate(piecePrefab, tile.transform.position, Quaternion.identity);
        var piece = pieceObj.GetComponent<Piece>();
        var spriteRenderer = piece.GetComponent<SpriteRenderer>();

        piece.playerIndex = playerIndex;
        piece.teamOne = playerIndex == 0; // Adjust as needed

        pieceObj.name = $"{pieceObj.name} player{playerIndex} {pieceNumber++}";//📛

        var spriteNum = PlayerPrefs.GetInt(TileHolder.Instance.players[playerIndex].name + "skin");
        var spriteSet = pieceSets.spriteSets[spriteNum];
        Debug.Log($"Using sprite set: {spriteSet.name} for player {playerIndex}");
        spriteRenderer.sprite =
            spriteSet.GetType().GetField(piecePrefab.name).GetValue(spriteSet) as Sprite;
        piece.transform.localScale
            = new Vector3(spriteSet.transformScale, spriteSet.transformScale, 1);

        // Get color selection index for this player
        var colorSelection = PlayerPrefs.GetInt(TileHolder.Instance.players[playerIndex].name + "color");

        // Get the correct ColorSet from the PieceColors ScriptableObject
        var colorSet = pieceSets.colorSets[colorSelection];

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
            AiManager.Instance.aiPieces.Add(piece);
        }

        Debug.Log($"Spawning piece: {piecePrefab.name} at position: {position} for player: {playerIndex}, AI: {isAi}");

        return piece;
    }
}