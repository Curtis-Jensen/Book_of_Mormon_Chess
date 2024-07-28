using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; } // Static instance

    public Tile[,] tiles = new Tile[8, 8];

    List<Tile> selectedTiles = new List<Tile>();
    Piece selectedPiece;

    void Awake()
    {
        // Ensure that there's only one instance of the Board
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

    public void HilightPossibleTiles(List<Vector2Int> attemptedMoves, Piece selectedPiece)
    {
        DeselectTiles();

        foreach(Vector2Int move in attemptedMoves)
        {
            this.selectedPiece = selectedPiece;

            // 🟩 Get the tile at the attempted move position
            var possibleTile = tiles[move.x, move.y];

            possibleTile.selected = true;

            // 🎨 Enable the SpriteRenderer to make it visible
            var spriteRenderer = possibleTile.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = true;

            selectedTiles.Add(possibleTile);
        }
    }

    public void MovePiece(Vector2 destinationLocation)
    {
        //Remove the piece from the previous tile's memory
        selectedPiece.transform.parent.GetComponent<Tile>().piece = null;

        // 🚀 Physically move the selected piece to the new position
        selectedPiece.transform.position = destinationLocation;

        // 🟩 Get the destination tile
        Tile destinationTile = tiles[(int)destinationLocation.x, (int)destinationLocation.y];

        // 💥 Destroy the piece on the destination tile if there is one
        if (destinationTile.piece != null)
        {
            Destroy(destinationTile.piece.gameObject);
        }

        // 👪 Set the piece's new parent to the destination tile
        selectedPiece.transform.SetParent(destinationTile.transform);

        // 📌 Assign the selected piece to the destination tile
        destinationTile.piece = selectedPiece;

        DeselectTiles();
    }



    /// <summary>
    /// Check if a tile is empty
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsTileEmpty(Vector2Int position)
    {
        if (position.x < 0 || position.x >= 8 || position.y < 0 || position.y >= 8) return false;

        Tile tile = tiles[position.x, position.y];
        return tile.piece == null;
    }

    /// <summary>
    /// Check if a tile contains an enemy piece
    /// </summary>
    /// <param name="position"></param>
    /// <param name="isWhite"></param>
    /// <returns></returns>
    public bool IsEnemyPiece(Vector2Int position, bool isWhite)
    {
        if (position.x < 0 || position.x >= 8 || position.y < 0 || position.y >= 8) return false;

        Tile tile = tiles[position.x, position.y];

        Debug.Log(tile.piece );
        return tile.piece != null && tile.piece.isWhite != isWhite;
    }

    /// <summary>
    /// Deselects all currently selected tiles.  
    /// Called when another piece is selected or a piecce moves
    /// </summary>
    void DeselectTiles()
    {
        foreach (var tile in selectedTiles)
        {
            tile.selected = false;
            tile.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
