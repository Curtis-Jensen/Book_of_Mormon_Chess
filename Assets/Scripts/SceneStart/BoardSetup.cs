using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

[System.Serializable]
public class Player
{
    public string name;
    public bool isAi;
    public bool teamOne;
}

[RequireComponent(typeof(AiManager))]
[RequireComponent(typeof(TurnManager))]
public class BoardSetup : MonoBehaviour
{
    public Player[] players;
    public GameObject lightTilePrefab;
    public GameObject darkTilePrefab;
    public GameObject pawn;
    public GameObject[] backPiecePrefabs;

    [HideInInspector] public int boardSize = 8;

    protected GameObject[,] tiles;
    protected TurnManager TurnManager;
    protected AiManager aiManager;
    protected PieceSpawner pieceSpawner;

    void Start()
    {
        StartBoard();
        StartPieces();
    }

    void StartBoard()
    {
        InitializeVariables();
        SpawnTiles();
        InitializeTurnManagerReferences();
        InitializeTurnManager();
    }

    protected virtual void StartPieces()
    {
        var pieceChoices = RandomizePieces();
        pieceChoices = PlaceKing(pieceChoices);    
        OrderPieces(pieceChoices);
    }

    void InitializeVariables()
    {
        boardSize = PlayerPrefs.GetInt("boardSize");
        TurnManager = GetComponent<TurnManager>();
        TurnManager.boardSize = boardSize;
        pieceSpawner = GetComponent<PieceSpawner>();
        //If the int comes in as 1 that means true
        aiManager = GetComponent<AiManager>();
        TurnManager.aiManager = aiManager;
        TurnManager.players = players;
        TurnManager.players[0].isAi = PlayerPrefs.GetInt("1isAI") == 1;
        TurnManager.players[1].isAi = PlayerPrefs.GetInt("2isAI") == 1;
    }

    void SpawnTiles()
    {
        tiles = new GameObject[boardSize, boardSize];

        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                // Alternate between dark and light tiles
                GameObject prefabToInstantiate = (x + y) % 2 == 0 ? darkTilePrefab : lightTilePrefab;

                var tilePosition = new Vector3Int(x, y, 0);

                var newTile =
                    Instantiate(prefabToInstantiate, tilePosition, Quaternion.identity, transform);

                var tileSpriteRenderer = newTile.GetComponent<SpriteRenderer>();
                tileSpriteRenderer.flipX = Random.value > 0.5f;
                tileSpriteRenderer.flipY = Random.value > 0.5f;

                newTile.name = $"Tile ({x + 1}, {y + 1})";
                tiles[x,y] = newTile;
            }
        }
    }

    protected int[] RandomizePieces()
    {
        int[] pieceChoices = new int[boardSize];
        List<int> bag = new();

        for (int i = 0; i < boardSize; i++)
        {
            // Refill and reshuffle the bag if it's empty
            if (bag.Count == 0)
            {
                bag.Clear();
                // Fill the bag with indices of backPiecePrefabs
                //We use 1 indexing here because the 0 spot must be the king
                for (int j = 1; j < backPiecePrefabs.Length; j++)
                {
                    bag.Add(j);
                }

                // Modified Fisher-Yates shuffle starting from index 1
                for (int j = bag.Count - 1; j > 0; j--)
                {
                    int randomIndex = Random.Range(1, j + 1); // Start from 1 to preserve piece type indexing
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

    int[] PlaceKing(int[] pieceChoices)
    {
        //We set a random spot to be 0 so 1 king spawns
        pieceChoices[Random.Range(0, pieceChoices.Length)] = 0;

        return pieceChoices;
    }

    virtual protected void OrderPieces(int[] pieceChoices)
    {
        OrderBackRows(pieceChoices, 0, 0);
        OrderBackRows(pieceChoices, 1, boardSize - 1);

        if (boardSize > 3)
        {
            OrderPawns(0, 1);
            OrderPawns(1, boardSize - 2);
        }
    }

    protected void OrderBackRows(int[] pieceChoices, int playerIndex, int pieceRow)
    {
        for (int x = 0; x < boardSize; x++)
        {
            pieceSpawner.SpawnPiece(backPiecePrefabs[pieceChoices[x]], new Vector2(x, pieceRow), playerIndex);
        }
    }

    protected void OrderPawns(int playerIndex, int pawnRow)
    {
        for (int x = 0; x < boardSize; x++)
        {
            pieceSpawner.SpawnPiece(pawn, new Vector2(x, pawnRow), playerIndex);
        }
    }

    void InitializeTurnManagerReferences()
    {
        TurnManager.tiles = new Tile[boardSize, boardSize];

        TurnManager.Instance = TurnManager;

        TurnManager.audioSource = GetComponent<AudioSource>();
    }

    void InitializeTurnManager()
    {
        // Iterate through each child in the hierarchy
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                var tilePosition = boardSize * y + x;

                Tile tile = transform.GetChild(tilePosition).GetComponent<Tile>(); // Get the Tile component 
                if (tile == null)
                {
                    Debug.LogError($"Tile component not found on GameObject at position ({x}, {y}).");
                }

                TurnManager.tiles[x, y] = tile;

                // If there is a pawn on this tile, initialize it
                if (tile.transform.childCount > 0)
                {
                    Piece piece = tile.transform.GetChild(0).GetComponent<Piece>();
                    if (piece != null)
                    {
                        piece.teamOne = y < 2; // Assuming white pawns are on the first two rows
                    }
                }
            }
        }
    }
}
