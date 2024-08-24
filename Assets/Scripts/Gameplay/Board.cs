using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; } // Static instance
    public Tile[,] tiles;
    public bool whiteTurn = true;
    public float moveTime = 0.5f;

    [HideInInspector]
    public int boardSize = 8;
    List<Tile> selectedTiles = new List<Tile>();
    Piece selectedPiece;
    GameObject capturedPiece;
    AudioSource audioSource;

    public  void Initialize()
    {
        boardSize = GetComponent<BoardSetup>().boardSize;
        InstantiateObjects();
        InitializeBoard();
    }

    void InstantiateObjects()
    {
        tiles = new Tile[boardSize, boardSize];

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    void InitializeBoard()
    {
        // Iterate through each child in the hierarchy
        for (int y = 0; y < boardSize; y++)
        {
            GameObject row = transform.GetChild(y).gameObject; // Get the row GameObject
            for (int x = 0; x < boardSize; x++)
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
        var tileUnderPiece = 
            tiles[(int)selectedPiece.transform.position.x, (int)selectedPiece.transform.position.y];

        // 🎨 Enable the SpriteRenderer to make it visible
        var highlight = tileUnderPiece.highlight;
        highlight.enabled = true;

        selectedTiles.Add(tileUnderPiece);

        foreach (Vector2Int move in attemptedMoves)
        {
            this.selectedPiece = selectedPiece;

            // 🟩 Get the tile at the attempted move position
            var possibleTile = tiles[move.x, move.y];

            possibleTile.selected = true;

            // 🎨 Enable the SpriteRenderer to make it visible
            highlight = possibleTile.highlight;
            highlight.enabled = true;

            selectedTiles.Add(possibleTile);
        }
    }

    public void MovePiece(Vector2 destination)
    {
        Tile destinationTile = tiles[(int)destination.x, (int)destination.y];

        DeselectPrevious(destination, destinationTile);
        DeselectTiles();

        // ➡️ Start the coroutine to physically move the piece
        StartCoroutine(PhysicallyMovePiece(selectedPiece.gameObject, destination));

        // 👪 Set the piece's new parent to the destination tile both in transform and in script
        selectedPiece.transform.SetParent(destinationTile.transform);
        destinationTile.piece = selectedPiece;

        selectedPiece.firstTurnTaken = true;

        // Change which player's turn it is
        whiteTurn = !whiteTurn;
    }

    IEnumerator PhysicallyMovePiece(GameObject piece, Vector2 destination)
    {
        float time = 0;
        Vector3 startPosition = piece.transform.position;
        Vector3 endPosition = new Vector3(destination.x, destination.y, startPosition.z); // Preserve z-axis position

        while (time < moveTime)
        {
            piece.transform.position = Vector3.Lerp(startPosition, endPosition, time / moveTime);
            time += Time.deltaTime;
            yield return null;
        }
        piece.transform.position = endPosition;

        audioSource.Play();

        DestroyEnemyPiece();
    }

    /// <summary>
    /// Handles the transition of the piece to the new tile by clearing old state and preparing the destination tile
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="destinationTile"></param>
    void DeselectPrevious(Vector2 destination, Tile destinationTile)
    {
        // 🚫👪 Orphan the piece from the tile script so en passants aren't eternal
        var piecePosition = selectedPiece.transform.position;
        Tile startingTile = tiles[(int)piecePosition.x, (int)piecePosition.y];
        startingTile.piece = null;

        // 💥 Destroy the piece on the destination tile if there is one
        if (destinationTile.piece != null)
        {
            capturedPiece = destinationTile.piece.gameObject;
        }

    }

    void DestroyEnemyPiece()
    {
        Destroy(capturedPiece);
    }

    /// <summary>
    /// Check if a tile is empty
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsTileEmpty(Vector2Int position)
    {
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSize) return false;

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
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSize) return false;

        Tile tile = tiles[position.x, position.y];
        return tile.piece != null && tile.piece.isWhite != isWhite;
    }

    /// <summary>
    /// Deselects all currently selected tiles.  
    /// Called when another piece is selected or a piecce moves
    /// </summary>
    public void DeselectTiles()
    {
        foreach (var tile in selectedTiles)
        {
            tile.selected = false;
            tile.highlight.enabled = false;
        }
    }
}
