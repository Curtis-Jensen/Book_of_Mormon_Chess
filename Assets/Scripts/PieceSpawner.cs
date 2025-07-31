using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    public static PieceSpawner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Spawns a piece at the given position, assigns it to the tile, and registers with AI if needed.
    /// </summary>
    public Piece SpawnPiece(GameObject piecePrefab, Vector2Int position, int playerIndex, bool isAi, Sprite sprite)
    {
        var tile = TileHolder.Instance.tiles[position.x, position.y];
        var pieceObj = Instantiate(piecePrefab, tile.transform.position, Quaternion.identity);
        var piece = pieceObj.GetComponent<Piece>();
        var spriteRenderer = piece.GetComponent<SpriteRenderer>();

        piece.playerIndex = playerIndex;
        piece.teamOne = playerIndex == 0; // Adjust as needed

        var colorSelection = PlayerPrefs.GetInt(TileHolder.Instance.players[playerIndex].name + "color");//🎨
        if (piece is King)
        {
            spriteRenderer.color = colorSets[colorSelection].kingColor;
        }
        else
        {
            spriteRenderer.color = colorSets[colorSelection].baseColor;
        }

        tile.piece = piece;
        piece.transform.parent = tile.transform;

        if (isAi)
        {
            AiManager.Instance.aiPieces.Add(piece);
        }

        return piece;
    }
}