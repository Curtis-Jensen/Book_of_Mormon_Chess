using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

[System.Serializable]
public class Player
{
    public string name;
    public Color color;
    public bool isAi;
    public bool teamOne;
}

public class BoardSetup : MonoBehaviour
{
    public Player[] players;
    public float cameraPadding;
    public Vector3 backGroundOffset;
    public GameObject background;
    public GameObject rowPrefab;
    public GameObject lightTilePrefab;
    public GameObject  darkTilePrefab;
    public GameObject pawn;
    public GameObject[] backPiecePrefabs;

    [HideInInspector] public int boardSize = 8;

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
        //If the int comes in as 1 that means true
        aiManager = GetComponent<AiManager>();
        board.aiManager = aiManager;
        board.players = players;
        //Hardcoded to make the red / dark player AI, even though parts of the code support 2 AI
        board.players[1].isAi = PlayerPrefs.GetInt("isAi") == 1 ? true : false;
        CenterCamera();
        SpawnRows();
        SpawnTiles();
        var pieceChoices = RandomizePieces();
        SpawnPieces(pieceChoices);
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
    
    int[] RandomizePieces()
    {
        int[] pieceChoices = new int[boardSize];
        List<int> bag = new();

        int pieceIndex = 0;
        for (int i = 0; i < boardSize; i++)
        {
            // Refill and reshuffle the bag if it's empty
            if (bag.Count == 0)
            {
                // Fill the bag with indices of backPiecePrefabs
                for (int j = 0; j < backPiecePrefabs.Length; j++)
                {
                    bag.Add(j);
                }

                // Shuffle the bag
                for (int j = 0; j < bag.Count; j++)
                {
                    int randomIndex = Random.Range(0, bag.Count);
                    int temp = bag[j];
                    bag[j] = bag[randomIndex];
                    bag[randomIndex] = temp;
                }
            }

            // Assign the next piece from the bag to the pieceChoices array
            pieceChoices[i] = bag[0];
            bag.RemoveAt(0); // Remove the used piece from the bag
        }

        return pieceChoices;
    }

    void SpawnPieces(int[] pieceChoices)
    {
        var topRightTile = tiles.Count - 1;

        SpawnBackRows (topRightTile, pieceChoices);
        if(boardSize > 3)
        {
            SpawnPawns(topRightTile);
        }
    }

    void SpawnBackRows(int topRightTile, int[] pieceChoices)
    {
        var playerIndex = 1;
        for (int x = topRightTile; x > topRightTile - boardSize; x--)
        {
            int i = topRightTile - x;
            SpawnPiece(backPiecePrefabs[pieceChoices[i]], x, playerIndex);
        }

        playerIndex = 0;
        for (int x = 0; x < boardSize; x++)
        {
            SpawnPiece(backPiecePrefabs[pieceChoices[x]], x, playerIndex);
        }
    }

    void SpawnPawns(int topRightTile)
    {
        var playerIndex = 1;
        for (int x = topRightTile - boardSize; x > topRightTile - boardSize - boardSize; x--)
        {
            SpawnPiece(pawn, x, playerIndex);
        }

        playerIndex = 0;
        for (int x = boardSize; x < boardSize + boardSize; x++)
        {
            SpawnPiece(pawn, x, playerIndex);
        }
    }

    void SpawnPiece(GameObject piecePrefab, int x, int playerIndex)
    {
        var player = players[playerIndex];
        var pieceInstance =
        Instantiate(piecePrefab, tiles[x].transform);

        pieces.Add(pieceInstance);

        pieceInstance.GetComponent<SpriteRenderer>().color = player.color;
        pieceInstance.name = $"{pieceInstance.name} {player.name} {x + 1}";

        var pieceComponent = pieceInstance.GetComponent<Piece>();
        pieceComponent.isLight = player.teamOne;
        pieceComponent.playerIndex = playerIndex;

        if (player.isAi)
        {
            aiManager.aiPieces.Add(pieceComponent);
        }
    }

    void CenterCamera()
    {
        var cam = FindAnyObjectByType<Camera>();

        cam.orthographicSize = boardSize / 2 + cameraPadding;

        var camTransform = cam.gameObject;

        float centerLength = boardSize / 2;
        if (evenBoard)
        {
            centerLength -= 0.5f;
        }
        var centeredPosition = new Vector3(centerLength, centerLength, -10);
        camTransform.transform.position = centeredPosition;

        background.transform.position = centeredPosition + (backGroundOffset * boardSize);
        background.transform.localScale = new Vector3(
            background.transform.localScale.x * boardSize,  // Width  (x-axis)
            background.transform.localScale.y * boardSize,  // Height (y-axis)
            background.transform.localScale.z);
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
