using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Tile[,] tiles = new Tile[8, 8];

    void Start()
    {
        InitializeBoard();
    }

    void InitializeBoard()
    {
        // Iterate through each child in the hierarchy
        for (int y = 0; y < 8; y++)
        {
            GameObject row = transform.GetChild(y).gameObject; // Get the row GameObject
            for (int x = 0; x < 8; x++)
            {
                Tile tile = row.transform.GetChild(x).GetComponent<Tile>(); // Get the Tile component
                if (tile != null)
                {
                    tiles[x, y] = tile;

                    // If there is a pawn on this tile, initialize it
                    if (tile.transform.childCount > 0)
                    {
                        Piece piece = tile.transform.GetChild(0).GetComponent<Piece>();
                        if (piece != null)
                        {
                            piece.isWhite = y < 2; // Assuming white pawns are on the first two rows
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Tile component not found on GameObject at position ({x}, {y}).");
                }
            }
        }
    }


    public void HilightPossibleTiles(List<Vector2Int> attemptedMoves)
    {
        // 🟩 Get the tile at the attempted move position
        var possibleTile = tiles[attemptedMoves[0].x, attemptedMoves[0].y];

        possibleTile.selected = true;

        // 🎨 Enable the SpriteRenderer to make it visible
        var spriteRenderer = possibleTile.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;
    }
}
