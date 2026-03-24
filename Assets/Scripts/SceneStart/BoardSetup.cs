using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

// 🚨TECH DEBT TODO🚨: The King should be its own prefab instead of relying on knowing that index 0 is the king.
[RequireComponent(typeof(AiManager))]
[RequireComponent(typeof(TurnManager))]
[RequireComponent(typeof(HoardEndingManager))]
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
        if (TurnManager.players[0].isAi)
        {
            StartCoroutine(DelayFirstTurn());
        }
    }

    IEnumerator DelayFirstTurn()
    {
        yield return new WaitForSeconds(1f); // 1 second delay
        TurnManager.AiTurn();
    }

    void StartBoard()
    {
        InitializeVariables();
        SpawnTiles();
        InitializeTurnManagerReferences();
        AssigningTiles();
    }

    protected virtual void StartPieces()
    {
        var pieceChoices = RandomizePieces();
        pieceChoices = PlaceKing(pieceChoices);    
        OrderPieces(pieceChoices);
    }

    void InitializeVariables()
    {
        boardSize = PlayerPrefs.GetInt("boardSize", 7);
        
        // Load back row prefabs from PlayerPrefs
        int backRowCount = PlayerPrefs.GetInt("backRowCount");
        backPiecePrefabs = new GameObject[backRowCount];
        for (int i = 0; i < backRowCount; i++)
        {
            string prefabName = PlayerPrefs.GetString($"backRowPrefab_{i}");
            backPiecePrefabs[i] = Resources.Load<GameObject>($"Prefabs/{prefabName}");
            if (backPiecePrefabs[i] == null)
            {
                Debug.LogError($"Failed to load prefab: {prefabName}");
            }
        }

        TurnManager = GetComponent<TurnManager>();
        TurnManager.boardSize = boardSize;
        pieceSpawner = GetComponent<PieceSpawner>();
        pieceSpawner.players = players;
        //If the int comes in as 1 that means true
        aiManager = GetComponent<AiManager>();
        TurnManager.endingManager = GetComponent<HoardEndingManager>();
        TurnManager.aiManager = aiManager;
        TurnManager.players = players;

        for(int i = 0;i < players.Length; i++)
        {
            TurnManager.players[i].isAi = PlayerPrefs.GetInt($"{i+1}isAI", 0) == 1;
        }
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

    //
    protected int[] RandomizePieces()
    {
        int lanesPerSide = Mathf.Max(1, players.Length / 2);
        int laneWidth = Mathf.Max(1, boardSize / lanesPerSide);
        int[] pieceChoices = new int[laneWidth];
        List<int> bag = new();

        if (backPiecePrefabs == null || backPiecePrefabs.Length == 0)
        {
            return pieceChoices;
        }

        for (int i = 0; i < laneWidth; i++)
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

            // If there are no non-king pieces configured, keep default fallback value (king index 0)
            if (bag.Count == 0)
            {
                pieceChoices[i] = 0;
                continue;
            }

            // Assign the next piece from the bag to the pieceChoices array
            pieceChoices[i] = bag[0];
            bag.RemoveAt(0); // Remove the used piece from the bag
        }

        return pieceChoices;
    }

    void GetPlayerLaneBounds(int playerIndex, out int startX, out int endX)
    {
        int lanesPerSide = Mathf.Max(1, players.Length / 2);
        int laneWidth = Mathf.Max(1, boardSize / lanesPerSide);
        int laneIndex = playerIndex / 2;

        startX = laneIndex * laneWidth;
        if (startX >= boardSize)
        {
            startX = boardSize;
            endX = boardSize;
            return;
        }

        endX = Mathf.Min(startX + laneWidth, boardSize);
    }

    void GetPlayerRows(int playerIndex, out int homeRow, out int pawnRow)
    {
        bool isBottomSide = playerIndex % 2 == 0;
        homeRow = isBottomSide ? 0 : boardSize - 1;
        pawnRow = isBottomSide ? 1 : boardSize - 2;
    }

    int[] PlaceKing(int[] pieceChoices)
    {
        if (pieceChoices.Length == 0)
        {
            return pieceChoices;
        }

        //We set a random spot to be 0 so 1 king spawns
        pieceChoices[Random.Range(0, pieceChoices.Length)] = 0;

        return pieceChoices;
    }

    virtual protected void OrderPieces(int[] pieceChoices)
    {
        for(int i = 0; i < players.Length; i++)
        {
            OrderBackRows(pieceChoices, i);

            if (boardSize > 3)
            {
                OrderPawns(i);
            }
        } 
    }

    protected void OrderBackRows(int[] pieceChoices, int playerIndex)
    {
        if (backPiecePrefabs == null || backPiecePrefabs.Length == 0)
        {
            return;
        }

        GetPlayerLaneBounds(playerIndex, out int startX, out int endX);
        GetPlayerRows(playerIndex, out int homeRow, out _);

        for (int x = startX; x < endX; x++)
        {
            int laneLocalIndex = x - startX;
            if (laneLocalIndex < 0 || laneLocalIndex >= pieceChoices.Length)
            {
                continue;
            }

            int piecePrefabIndex = pieceChoices[laneLocalIndex];
            if (piecePrefabIndex < 0 || piecePrefabIndex >= backPiecePrefabs.Length)
            {
                continue;
            }

            GameObject piecePrefab = backPiecePrefabs[piecePrefabIndex];
            if (piecePrefab == null)
            {
                continue;
            }

            pieceSpawner.SpawnPiece(piecePrefab, new Vector2(x, homeRow), playerIndex);
        }
    }

    protected void OrderPawns(int playerIndex)
    {
        GetPlayerLaneBounds(playerIndex, out int startX, out int endX);
        GetPlayerRows(playerIndex, out _, out int pawnRow);

        for (int x = startX; x < endX; x++)
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

    void AssigningTiles()
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
                        Debug.Log($"We actually reached this!");
                        //piece.teamOne = y < 2; // Assuming white pawns are on the first two rows
                    }
                }
            }
        }
    }
}
