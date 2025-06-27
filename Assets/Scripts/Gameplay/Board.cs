using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance { get; set; } // Static instance
    public Tile[,] tiles;
    public float moveTime = 0.5f;
    [Tooltip("Particle system to play when the piece is destroyed.")]
    public GameObject destroyParticlesPrefab;

    [HideInInspector] public Player[] players;
    [HideInInspector] public int boardSize = 8;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public AiManager aiManager;

    List<Tile> selectedTiles = new();
    Piece selectedPiece;
    int playerTurn = 0;

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
        else
        {
            MovePiece(clickedTile.transform.position);
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
            tile.selected= false;
            tile.Highlight(false);
        }
        selectedTiles.Clear();

        return selectedTiles;
    }

    void PreviewPieceMoves(Piece piece)
    {
        //If no piece is on the tile
        if (piece == null) return;
        //If no piece is on the tile, or it's not that piece's turn
        if (playerTurn != piece.playerIndex) return;

        // Access the move data or any other data from the piece script
        selectedTiles = HilightPossibleTiles(piece.GetMoves(), piece, selectedTiles);
    }

    public List<Tile> HilightPossibleTiles(List<Vector2Int> attemptedMoves, Piece selectedPiece, List<Tile> selectedTiles)
    {
        var tileUnderPiece =
            tiles[(int)selectedPiece.transform.position.x, (int)selectedPiece.transform.position.y];

        tileUnderPiece.Highlight(true);

        selectedTiles.Add(tileUnderPiece);
        this.selectedPiece = selectedPiece;

        foreach (Vector2Int move in attemptedMoves)
        {

            // 🟩 Get the tile at the attempted move position
            var possibleTile = tiles[move.x, move.y];

            possibleTile.selected = true;
            possibleTile.Highlight(true);

            selectedTiles.Add(possibleTile);
        }

        return selectedTiles;
    }
    #endregion
	
    public void MovePiece(Vector2 destination)
    {
        selectedTiles = DeselectTiles(selectedTiles);
        DeselectPreviousPiece(destination);
        StartCoroutine(PhysicallyMovePiece(selectedPiece.gameObject, destination, selectedPiece));
    }

    #region ➡Moving sub-methods
    /// <summary>
    /// Handles the transition of the piece to the new tile by clearing old state and preparing the destination tile
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="destinationTile"></param>
    void DeselectPreviousPiece(Vector2 destination)
    {
        Tile destinationTile = tiles[(int)destination.x, (int)destination.y];
        // 🚫👪 Orphan the piece from the tile script so en passants aren't eternal
        var piecePosition = selectedPiece.transform.position;
        Tile startingTile = tiles[(int)piecePosition.x, (int)piecePosition.y];

        if(selectedPiece == null)
        {
            Debug.LogError("selectedPiece is null");
        }

        if(selectedPiece.gameObject == null)
        {
            Debug.LogError("selectedPiece.gameObject is null");
        }

        if (startingTile == null)
        {
            Debug.LogError("startingTile is null");
        }

        if(startingTile.piece == null)
        {
            Debug.LogError("startingTile.piece is null");
        }

        if (startingTile.piece != selectedPiece)
        {
            Debug.LogError
                ($"Expected to deselect {selectedPiece.gameObject.name}" +
                $" but instead almost deselected {startingTile.piece.gameObject.name}");
        }

        startingTile.piece = null;
    }

    protected virtual IEnumerator PhysicallyMovePiece(GameObject piece, Vector2 destination, Piece selectedPiece)
    {
        float time = 0;
        Vector3 startPosition = piece.transform.position;
        Vector3 endPosition = new Vector3(destination.x, destination.y, startPosition.z); // Preserve z-axis position
        var destinationTile = tiles[(int)destination.x, (int)destination.y];

        while (time < moveTime)
        {
            piece.transform.position = Vector3.Lerp(startPosition, endPosition, time / moveTime);
            time += Time.deltaTime;
            yield return null;
        }
        DestroyEnemyPiece(destinationTile);
  
        piece.transform.position = endPosition;

        AssignNewParent(destinationTile, selectedPiece);

        audioSource.Play();

        FinishTurn();
        ChangeTurn();
    }

    protected virtual void FinishTurn() { }

    protected void DestroyEnemyPiece(Tile destinationTile)
    {
		//Figure out if there's a piece that needs to be destroyed
		var capturedPiece = destinationTile.piece;
  
        if (capturedPiece == null) return;

        //Set the position the particles need to spawn to be visible
        Vector3 position = 
            new Vector3(capturedPiece.transform.position.x, capturedPiece.transform.position.y, -5);
        // Spawn particles
        GameObject particles = Instantiate(destroyParticlesPrefab, position, Quaternion.identity);

        aiManager.aiPieces.Remove(capturedPiece);
        Destroy(capturedPiece.gameObject);
    }

    /// <summary>
    /// 👪 Set the piece's new parent to the destination tile both in transform and in script
    /// </summary>
    /// <param name="destinationTile"></param>
    protected void AssignNewParent(Tile destinationTile, Piece selectedPiece)
    {
        selectedPiece.transform.SetParent(destinationTile.transform);
        destinationTile.piece = selectedPiece;
    }


    protected void ChangeTurn()
    {
        selectedPiece.firstTurnTaken = true;

        // Move to next player
        playerTurn++;
        if (playerTurn >= players.Length) 
        {
            playerTurn = 0;
        }

        if (players[playerTurn].isAi)
        {
            //Select a new AI piece and a move for it
            selectedPiece = aiManager.ChoosePiece();

            var aiMoves = selectedPiece.GetMoves();

            var moveChoice = Random.Range(0, aiMoves.Count - 1);
            MovePiece(aiMoves[moveChoice]);
        }
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
        return tile.piece != null && tile.piece.teamOne != isLight;
    }
    #endregion
}
