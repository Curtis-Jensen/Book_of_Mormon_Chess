using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;


[RequireComponent(typeof(AiManager))]
[RequireComponent(typeof(TileHolder))]
public class ResizableSetup : BoardSetup
{
    public Player[] players;
    public float cameraPadding;
    public Vector3 backGroundOffset;
    public GameObject background;
    public GameObject rowPrefab;
    public GameObject lightTilePrefab;
    public GameObject darkTilePrefab;
    public GameObject pawn;
    public GameObject[] backPiecePrefabs;
    public SpriteSets spriteSets;
    public ColorSet[] colorSets;

    [HideInInspector] public int boardSize = 8;

    SpriteSet spriteSet;
    List<GameObject> rows = new();
    List<GameObject> tiles = new();
    TileHolder tileHolder;
    AiManager aiManager;

    void Awake()
    {
        InitializeVariables();
        CenterCamera();
        SpawnRows();
        SpawnTiles();
        var pieceChoices = RandomizePieces();
        ArrangePieces(pieceChoices);
        InitializeBoardReferences();
        InitializeBoard();
    }

    void InitializeVariables()
    {
        boardSize = PlayerPrefs.GetInt("boardSize");
        tileHolder = GetComponent<TileHolder>();
        tileHolder.boardSize = boardSize;
        //If the int comes in as 1 that means true
        aiManager = GetComponent<AiManager>();
        tileHolder.aiManager = aiManager;
        tileHolder.players = players;
        //Hardcoded to make the red / dark player AI, even though parts of the code support 2 AI
        tileHolder.players[1].isAi = PlayerPrefs.GetInt("isAi") == 1 ? true : false;
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

        for (int i = 0; i < boardSize; i++)
        {
            // Refill and reshuffle the bag if it's empty
            if (bag.Count == 0)
            {
                // Fill the bag with indices of backPiecePrefabs
                //We use 1 indexing here because the 0 spot must be the king
                for (int j = 1; j < backPiecePrefabs.Length; j++)
                {
                    bag.Add(j);
                }

                // Shuffle the bag
                for (int j = 1; j < bag.Count; j++)
                {
                    int randomIndex = Random.Range(1, bag.Count);
                    int temp = bag[j];
                    bag[j] = bag[randomIndex];
                    bag[randomIndex] = temp;
                }
            }

            // Assign the next piece from the bag to the pieceChoices array
            pieceChoices[i] = bag[0];
            bag.RemoveAt(0); // Remove the used piece from the bag
        }

        //We set a random spot to be 0 so 1 king spawns
        pieceChoices[Random.Range(0, pieceChoices.Length)] = 0;

        return pieceChoices;
    }

    void ArrangePieces(int[] pieceChoices)
    {
        var topRightTile = tiles.Count - 1;

        ArrangeBackRows(topRightTile, pieceChoices);
        if (boardSize > 3)
        {
            ArrangePawns(topRightTile);
        }
    }

    void ArrangeBackRows(int topRightTile, int[] pieceChoices)
    {
        var playerIndex = 1;
        spriteSet = spriteSets.spriteSets[PlayerPrefs.GetInt(players[playerIndex].name + "skin")];
        for (int x = topRightTile; x > topRightTile - boardSize; x--)
        {
            int i = topRightTile - x;
            SpawnPiece(backPiecePrefabs[pieceChoices[i]], x, playerIndex);
        }

        playerIndex = 0;
        spriteSet = spriteSets.spriteSets[PlayerPrefs.GetInt(players[playerIndex].name + "skin")];
        for (int x = 0; x < boardSize; x++)
        {
            SpawnPiece(backPiecePrefabs[pieceChoices[x]], x, playerIndex);
        }
    }

    void ArrangePawns(int topRightTile)
    {
        var playerIndex = 1;
        spriteSet = spriteSets.spriteSets[PlayerPrefs.GetInt(players[playerIndex].name + "skin")];
        for (int x = topRightTile - boardSize; x > topRightTile - boardSize - boardSize; x--)
        {
            Pawn pawnInstance = (Pawn)SpawnPiece(pawn, x, playerIndex);
            pawnInstance.queenSprite = spriteSet.GetType().GetField("Queen").GetValue(spriteSet) as Sprite;
        }

        playerIndex = 0;
        spriteSet = spriteSets.spriteSets[PlayerPrefs.GetInt(players[playerIndex].name + "skin")];
        for (int x = boardSize; x < boardSize + boardSize; x++)
        {
            Pawn pawnInstance = (Pawn)SpawnPiece(pawn, x, playerIndex);
            pawnInstance.queenSprite = spriteSet.GetType().GetField("Queen").GetValue(spriteSet) as Sprite;
        }
    }

    /// <summary>
    /// Changing things here?  Check Pawn.QueenPromotion() too.
    /// 
    /// 🧑🏻Designate the player for later
    /// 
    /// 🎨 Color the piece.  If it's a king, use the special king color
    /// 
    /// 🏗️ Instantiate the piece prefab at the specified tile 
    /// 
    /// 🔍 Retrieve the Piece component for configuration
    /// 
    /// 📛 Assign a descriptive name to the piece GameObject 
    /// 
    /// ⚖️ Set piece properties for team and player ownership  
    /// 
    /// 🤖 Register the piece with AI manager if player is AI 
    /// </summary>
    /// <param name="piecePrefab">The prefab of the chess piece to spawn</param>
    /// <param name="x">The board position (x-coordinate) to spawn the piece </param>
    /// <param name="playerIndex">Index of the player owning the piece</param>
    Piece SpawnPiece(GameObject piecePrefab, int x, int playerIndex)
    {
        var player = players[playerIndex]; //🧑🏻
        var pieceInstance =
        Instantiate(piecePrefab, tiles[x].transform); //🏗️
        var spriteRenderer = pieceInstance.GetComponent<SpriteRenderer>();
        var pieceScript = pieceInstance.GetComponent<Piece>(); //🔍

        spriteRenderer.sprite =
            spriteSet.GetType().GetField(piecePrefab.name).GetValue(spriteSet) as Sprite;
        pieceInstance.transform.localScale
            = new Vector3(spriteSet.transformScale, spriteSet.transformScale, 1);

        var colorSelection = PlayerPrefs.GetInt(player.name + "color");//🎨
        if (pieceScript is King)
        {
            spriteRenderer.color = colorSets[colorSelection].kingColor;
        }
        else
        {
            spriteRenderer.color = colorSets[colorSelection].baseColor;
        }

        pieceInstance.name = $"{pieceInstance.name} {player.name} {x + 1}";//📛

        pieceScript.teamOne = player.teamOne;//⚖️
        pieceScript.playerIndex = playerIndex;

        if (player.isAi)//🤖
        {
            aiManager.aiPieces.Add(pieceScript);
        }

        return pieceScript;
    }

    void CenterCamera()
    {
        var cam = FindAnyObjectByType<Camera>();

        cam.orthographicSize = boardSize / 2 + cameraPadding;

        var camTransform = cam.gameObject;

        float centerLength = boardSize / 2;

        bool evenBoard = boardSize % 2 == 0;
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
        tileHolder.tiles = new Tile[boardSize, boardSize];

        TileHolder.Instance = tileHolder;

        tileHolder.audioSource = GetComponent<AudioSource>();
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

                tileHolder.tiles[x, y] = tile;

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
