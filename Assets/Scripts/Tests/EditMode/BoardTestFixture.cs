using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Builds a minimal real TurnProgresser + tile grid in EditMode so static helpers that
// read TurnProgresser.Instance.tiles (BoardSimulator, GameStateSerializer) can be tested
// without loading a full scene.
public class BoardTestFixture
{
    public readonly TurnProgresser TurnProgresser;
    public readonly int BoardSize;
    private readonly List<GameObject> spawnedObjects = new();

    public BoardTestFixture(int boardSize = 8)
    {
        BoardSize = boardSize;

        var turnProgresserObject = new GameObject("TestTurnProgresser");
        spawnedObjects.Add(turnProgresserObject);
        TurnProgresser = turnProgresserObject.AddComponent<TurnProgresser>();
        TurnProgresser.boardSize = boardSize;
        TurnProgresser.tiles = new TileSelector[boardSize, boardSize];

        // AddComponent doesn't run Awake synchronously in EditMode tests, but
        // BoardSimulator/GameStateSerializer read TurnProgresser.Instance immediately --
        // set the private-setter singleton directly so it's wired up before Awake fires.
        typeof(TurnProgresser)
            .GetProperty(nameof(TurnProgresser.Instance), BindingFlags.Public | BindingFlags.Static)
            .SetValue(null, TurnProgresser);

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                var tileObject = new GameObject($"Tile_{x}_{y}");
                spawnedObjects.Add(tileObject);
                TurnProgresser.tiles[x, y] = tileObject.AddComponent<TileSelector>();
            }
        }
    }

    public TestPiece PlacePiece(int x, int y, int playerIndex = 0, string prefabName = "TestPiece", bool firstTurnTaken = false)
    {
        var pieceObject = new GameObject($"TestPiece_{x}_{y}");
        spawnedObjects.Add(pieceObject);

        var piece = pieceObject.AddComponent<TestPiece>();
        piece.playerIndex = playerIndex;
        piece.prefabName = prefabName;
        piece.firstTurnTaken = firstTurnTaken;
        piece.boardSize = BoardSize;

        TurnProgresser.tiles[x, y].piece = piece;
        return piece;
    }

    public void Teardown()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Object.DestroyImmediate(obj);
        }
        spawnedObjects.Clear();
    }
}

// Bare-bones concrete Piece so tests can place real tile-grid occupants without
// pulling in any specific piece's movement rules.
public class TestPiece : Piece
{
    public override List<Vector2Int> GetMoves() => new();
}
