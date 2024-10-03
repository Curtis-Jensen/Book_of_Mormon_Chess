using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BoardSetup : MonoBehaviour
{
    public int boardSize = 8;
    public float cameraPadding;
    public GameObject rowPrefab;
    public GameObject lightTilePrefab;
    public GameObject  darkTilePrefab;
    public GameObject[] lightPiecePrefabs;
    public GameObject[]  darkPiecePrefabs;

    bool evenBoard;
    List<GameObject> rows =   new();
    List<GameObject> tiles =  new();
    List<GameObject> pieces = new();
    Board board;
    AiManager aiManager;


    void Awake()
    {
        boardSize = PlayerPrefs.GetInt("boardSize");
        evenBoard = IsBoardEven();
        board = GetComponent<Board>();
        board.boardSize = boardSize;
        aiManager = GetComponent<AiManager>();
        board.aiManager = aiManager;
        CenterCamera();
        SpawnRows();
        SpawnTiles();
        SpawnPieces();
        InitializeBoardReferences();
        InitializeBoard();
    }

    bool IsBoardEven()
    {
        return boardSize % 2 == 0;
    }

    void SpawnRows()
    {
        for (int y = 0; y < boardSize; y++) 
        {
            var newRow = Instantiate(rowPrefab, transform);

            newRow.name = "Row " + (y + 1);
            rows.Add(newRow);
        }
    }
    
    void SpawnTiles()
    {
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                // Alternate between dark and light tiles
                GameObject prefabToInstantiate = (x + y) % 2 == 0 ? darkTilePrefab : lightTilePrefab;

                var tilePosition = new Vector3Int(x, y, 0);

                var newTile = 
                    Instantiate(prefabToInstantiate, tilePosition, Quaternion.identity, rows[y].transform);

                newTile.name = "Tile " + (x + 1);
                tiles.Add(newTile);
            }
        }
    }
    
    void SpawnPieces()
    {
        var topRightTile = tiles.Count - 1;
        int[] pieceChoice = new int[boardSize];

        for (int i = 0; i < boardSize; i++)
        {
             pieceChoice[i] = Random.Range(0, lightPiecePrefabs.Length);
        }

        SpawnBackRows (topRightTile, pieceChoice);
        if(boardSize > 3)
        {
            SpawnFrontRows(topRightTile, pieceChoice);
        }
    }

    void SpawnBackRows(int topRightTile, int[] pieceChoice)
    {
        for (int x = topRightTile; x > topRightTile - boardSize; x--)
        {
            int i = topRightTile - x;
            var newPiece =
                    Instantiate(darkPiecePrefabs[pieceChoice[i]], tiles[x].transform);

            newPiece.name = "Piece " + (x + 1);
            pieces.Add(newPiece);
            aiManager.aiPieces.Add(newPiece.GetComponent<Piece>());
        }

        for (int x = 0; x < boardSize; x++)
        {
            var newPiece =
                    Instantiate(lightPiecePrefabs[pieceChoice[x]], tiles[x].transform);

            newPiece.name = "Piece " + (x + 1);
            pieces.Add(newPiece);
        }
    }

    void SpawnFrontRows(int topRightTile, int[] pieceChoice)
    {
        for (int x = topRightTile - boardSize; x > topRightTile - boardSize - boardSize; x--)
        {
            int i = topRightTile - x;
            var newPiece =
                    Instantiate(darkPiecePrefabs[0], tiles[x].transform);

            newPiece.name = "Piece " + (x + 1);
            pieces.Add(newPiece);
            aiManager.aiPieces.Add(newPiece.GetComponent<Piece>());
        }

        for (int x = boardSize; x < boardSize + boardSize; x++)
        {
            var newPiece =
                    Instantiate(lightPiecePrefabs[0], tiles[x].transform);

            newPiece.name = "Piece " + (x + 1);
            pieces.Add(newPiece);
        }
    }

    void CenterCamera()
    {
        var cam = FindAnyObjectByType<Camera>();

        cam.orthographicSize = boardSize / 2 + cameraPadding;

        var camTransform = cam.gameObject;

        float camPosition = boardSize / 2;
        if (evenBoard)
        {
            camPosition -= 0.5f;
        }
        camTransform.transform.position = new Vector3 (camPosition, camPosition, -10);
    }

    public void InitializeBoardReferences()
    {
        board.tiles = new Tile[boardSize, boardSize];

        Board.Instance = board;

        board.audioSource = GetComponent<AudioSource>();
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
                if (tile == null)
                {
                    Debug.LogError($"Tile component not found on GameObject at position ({x}, {y}).");
                }

                board.tiles[x, y] = tile;

                // If there is a pawn on this tile, initialize it
                if (tile.transform.childCount > 0)
                {
                    Piece piece = tile.transform.GetChild(0).GetComponent<Piece>();
                    if (piece != null)
                    {
                        piece.isLight = y < 2; // Assuming white pawns are on the first two rows
                    }
                }
            }
        }
    }
}
