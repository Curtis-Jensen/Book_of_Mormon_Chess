using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance { get; set; } // Static instance
    public Tile[,] tiles;
    public bool lightTurn = true;
    public float moveTime = 0.5f;
    [Tooltip("Particle system to play when the piece is destroyed.")]
    public GameObject destroyParticlesPrefab;

    [HideInInspector]
    public bool ai;
    [HideInInspector]
    public int boardSize = 8;
    [HideInInspector]
    public AudioSource audioSource;
    [HideInInspector]
    public AiManager aiManager;
    List<Tile> selectedTiles = new();
    Piece selectedPiece;

    /// <summary>
    /// Makes decisions on what to do if the tile is clicked in different states
    /// </summary>
    /// <param name="clickedTile"></param>
    public void OnTileClicked(Tile clickedTile)
    {
        //If the tile is not already selected, deselect other tiles and attempt to select the underlying piece
        if (!clickedTile.selected)
        {
            selectedTiles = DeselectTiles(selectedTiles);
            PreviewPieceMoves(clickedTile.piece);
        }
        //If the tile is selected, that means a piece is selected, so move that piece
        //After the piece has moved, if AI is enabled, have the AI piece move
        else
        {
            MovePiece(clickedTile.transform.position);

            if (ai)
            {
                //Select a new green piece and a move for it
                selectedPiece = aiManager.ChoosePiece();

                var aiMoves = selectedPiece.GetMoves();

                MovePiece(aiMoves[Random.Range(0, aiMoves.Count - 1)]);
            }
        }
    }

    #region🖱️ Tile Interaction: Selection & Movement

    /// <summary>
    /// Deselects all currently selected tiles.  
    /// Called when another piece is selected or a piecce moves
    /// </summary>
    public List<Tile> DeselectTiles(List<Tile> selectedTiles)
    {
        foreach (var tile in selectedTiles)
        {
            tile.selected = false;
            tile.highlight.enabled = false;
        }
        selectedTiles.Clear();

        return selectedTiles;
    }

    void PreviewPieceMoves(Piece piece)
    {
        //If no piece is on the tile
        if (piece == null) return;
        //If no piece is on the tile, or it's not that piece's turn
        if (lightTurn != piece.isLight) return;

        // Access the move data or any other data from the piece script
        selectedTiles = HilightPossibleTiles(piece.GetMoves(), piece, selectedTiles);
    }

    public List<Tile> HilightPossibleTiles(List<Vector2Int> attemptedMoves, Piece selectedPiece, List<Tile> selectedTiles)
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

        return selectedTiles;
    }
    #endregion

    public void MovePiece(Vector2 destination)
    {
        Tile destinationTile = tiles[(int)destination.x, (int)destination.y];

        selectedTiles = DeselectTiles(selectedTiles);
        DeselectPreviousPiece(destination, destinationTile);
        var capturedPiece = MarkEnemyForDestruction(destinationTile);
        StartCoroutine(PhysicallyMovePiece(selectedPiece.gameObject, destination, capturedPiece));
        AssignNewParent(destinationTile);
        ChangeTurn();
        MoveTest();
    }

    #region ➡Moving sub-methods
    /// <summary>
    /// Handles the transition of the piece to the new tile by clearing old state and preparing the destination tile
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="destinationTile"></param>
    void DeselectPreviousPiece(Vector2 destination, Tile destinationTile)
    {
        // 🚫👪 Orphan the piece from the tile script so en passants aren't eternal
        var piecePosition = selectedPiece.transform.position;
        Tile startingTile = tiles[(int)piecePosition.x, (int)piecePosition.y];

        if (startingTile.piece != selectedPiece)
            Debug.LogError($"Expected to clear starting tile of {selectedPiece.gameObject.name} but instead it would clear it of {startingTile.piece.gameObject.name});

        startingTile.piece = null;
    }

    /// <summary>
    /// 💥 If a piece is already occupying the tile at the end, mark it for destruction
    /// </summary>
    GameObject MarkEnemyForDestruction(Tile destinationTile)
    {
        GameObject capturedPiece = null;
        if (destinationTile.piece != null)
        {
            capturedPiece = destinationTile.piece.gameObject;
        }
        return capturedPiece;
    }

    IEnumerator PhysicallyMovePiece(GameObject piece, Vector2 destination, GameObject capturedPiece)
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
        DestroyEnemyPiece(capturedPiece);
    }

    void DestroyEnemyPiece(GameObject capturedPiece)
    {
        if (capturedPiece == null) return;

        //Set the position the particles need to spawn to be visible
        Vector3 position = 
            new Vector3(capturedPiece.transform.position.x, capturedPiece.transform.position.y, -5);
        // Spawn particles
        GameObject particles = Instantiate(destroyParticlesPrefab, position, Quaternion.identity);

        Destroy(capturedPiece);
    }

    /// <summary>
    /// 👪 Set the piece's new parent to the destination tile both in transform and in script
    /// </summary>
    /// <param name="destinationTile"></param>
    void AssignNewParent(Tile destinationTile)
    {
        selectedPiece.transform.SetParent(destinationTile.transform);
        destinationTile.piece = selectedPiece;
    }

    void ChangeTurn()
    {
        selectedPiece.firstTurnTaken = true;
        lightTurn = !lightTurn;
    }

    void MoveTest()
    {
        if (selectedPiece == null)
        {
            Debug.LogError("SelectedPiece is null!");
        }
    }
    #endregion

    #region 🧩 Tile & Piece Status Checkers
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
    /// <param name="isLight"></param>
    /// <returns></returns>
    public bool IsEnemyPiece(Vector2Int position, bool isLight)
    {
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSize) return false;

        Tile tile = tiles[position.x, position.y];
        return tile.piece != null && tile.piece.isLight != isLight;
    }
    #endregion
}
