using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Builds a minimal real board in EditMode -- TurnProgresser, a tile grid (each with
// TileThreats), a PieceSpawner with two Players, and a ThreatCoordinator -- so the real
// gameplay classes (BoardSimulator, GameStateSerializer, the Piece subclasses, King
// check detection, HoardEndingManager, AiManager) can be exercised without loading a
// scene or touching prefabs/sprites.
public class BoardTestFixture
{
    public readonly TurnProgresser TurnProgresser;
    public readonly PieceSpawner PieceSpawner;
    public readonly ThreatCoordinator ThreatCoordinator;
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

        PieceSpawner = turnProgresserObject.AddComponent<PieceSpawner>();
        PieceSpawner.players = new[]
        {
            new Player { name = "Player1", faction = Faction.Nephite, pieces = new List<Piece>() },
            new Player { name = "Player2", faction = Faction.Lamanite, pieces = new List<Piece>() },
        };
        TurnProgresser.pieceSpawner = PieceSpawner;

        var threatCoordinatorObject = new GameObject("TestThreatCoordinator");
        spawnedObjects.Add(threatCoordinatorObject);
        ThreatCoordinator = threatCoordinatorObject.AddComponent<ThreatCoordinator>();

        // AddComponent doesn't run Awake synchronously in EditMode tests, but the
        // gameplay code reads these singletons immediately -- set the private setters
        // directly so everything is wired up before Awake would otherwise fire.
        SetSingleton<TurnProgresser>(nameof(TurnProgresser.Instance), TurnProgresser);
        SetSingleton<ThreatCoordinator>(nameof(ThreatCoordinator.Instance), ThreatCoordinator);

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                var tileObject = new GameObject($"Tile_{x}_{y}");
                spawnedObjects.Add(tileObject);
                TurnProgresser.tiles[x, y] = tileObject.AddComponent<TileSelector>();
                tileObject.AddComponent<TileThreats>();
            }
        }
    }

    static void SetSingleton<T>(string propertyName, T value)
    {
        typeof(T)
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)
            .SetValue(null, value);
    }

    public TPiece PlacePiece<TPiece>(int x, int y, int playerIndex = 0, Faction? faction = null,
        string prefabName = null, bool firstTurnTaken = false) where TPiece : Piece
    {
        var pieceObject = new GameObject($"{typeof(TPiece).Name}_{x}_{y}");
        spawnedObjects.Add(pieceObject);

        var piece = pieceObject.AddComponent<TPiece>();
        piece.playerIndex = playerIndex;
        piece.prefabName = prefabName ?? typeof(TPiece).Name;
        piece.firstTurnTaken = firstTurnTaken;
        piece.boardSize = BoardSize;
        piece.faction = faction ?? PieceSpawner.players[playerIndex].faction;
        piece.transform.position = new Vector3(x, y, 0);

        TurnProgresser.tiles[x, y].piece = piece;
        PieceSpawner.players[playerIndex].pieces.Add(piece);

        return piece;
    }

    public TestPiece PlacePiece(int x, int y, int playerIndex = 0, Faction? faction = null,
        string prefabName = "TestPiece", bool firstTurnTaken = false)
        => PlacePiece<TestPiece>(x, y, playerIndex, faction, prefabName, firstTurnTaken);

    // Runs the real two-phase threat sweep (clear, then each piece declares) without
    // firing OnThreatsReady -- same entry point King.IsSafeAfterSimulation uses.
    public void RebuildThreats() => ThreatCoordinator.RebuildSilently();

    // StriplingWarrior normally wires itself up in Start() (move pattern, sprite
    // renderer cache, original faction, subscribing to TurnProgresser.OnMoveEnd) --
    // but Start() never fires a GameObject created via AddComponent in EditMode tests,
    // and calling SetupWounding() directly throws (it expects a TextMeshPro child for
    // the wounded-turns counter, which this fixture doesn't build). So set just the
    // private state GetMoves()/Die()/DecrementWoundedCounter() actually depend on.
    public StriplingWarrior PlaceStriplingWarrior(int x, int y, int playerIndex = 0, Faction? faction = null)
    {
        var warrior = PlacePiece<StriplingWarrior>(x, y, playerIndex, faction);
        var type = typeof(StriplingWarrior);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        var kingMovementPattern = (Vector2Int[])type
            .GetField("kingMovementPattern", flags)
            .GetValue(warrior);
        type.GetField("moveDirections", flags).SetValue(warrior, kingMovementPattern);
        type.GetField("originalFaction", flags).SetValue(warrior, warrior.faction);
        type.GetField("spriteRenderer", flags).SetValue(warrior, warrior.GetComponent<SpriteRenderer>());

        return warrior;
    }

    public static void SetWoundedTurns(StriplingWarrior warrior, int currentWoundedTurns, int maxWoundedTurns)
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        typeof(StriplingWarrior).GetField("currentWoundedTurns", flags).SetValue(warrior, currentWoundedTurns);
        typeof(StriplingWarrior).GetField("maxWoundedTurns", flags).SetValue(warrior, maxWoundedTurns);
    }

    public static Vector2Int[] GetMoveDirections(StriplingWarrior warrior) =>
        (Vector2Int[])typeof(StriplingWarrior)
            .GetField("moveDirections", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(warrior);

    // Piece.Die()'s default path instantiates death-particle and ghost prefabs; without
    // them it throws trying to Instantiate(null). Give it harmless stand-ins so Die()
    // tests can exercise the real capture/removal logic without crashing on VFX.
    public void EquipDeathEffects(Piece piece)
    {
        var particles = new GameObject("DummyDestroyParticles");
        particles.AddComponent<ParticleSystem>();
        spawnedObjects.Add(particles);
        piece.destroyParticlesPrefab = particles;

        var ghost = new GameObject("DummyGhost");
        ghost.AddComponent<SpriteRenderer>();
        spawnedObjects.Add(ghost);
        piece.ghost = ghost;
    }

    public AiManager AddAiManager(int maxCycles = 100)
    {
        var aiObject = new GameObject("TestAiManager");
        spawnedObjects.Add(aiObject);
        var aiManager = aiObject.AddComponent<AiManager>();
        aiManager.maxCycles = maxCycles;

        typeof(AiManager)
            .GetField("players", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(aiManager, PieceSpawner.players);

        return aiManager;
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
