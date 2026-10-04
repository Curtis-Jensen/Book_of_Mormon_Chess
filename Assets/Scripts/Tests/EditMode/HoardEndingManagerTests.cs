using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class HoardEndingManagerTests
{
    private BoardTestFixture fixture;
    private HoardEndingManager endingManager;
    private GameObject endingManagerObject;

    [SetUp]
    public void SetUp()
    {
        fixture = new BoardTestFixture();

        endingManagerObject = new GameObject("TestHoardEndingManager");
        endingManager = endingManagerObject.AddComponent<HoardEndingManager>();
        endingManager.winScreen = new GameObject("DummyWinScreen");

        // Awake() (which wires up pieceSpawner via FindAnyObjectByType) never runs
        // synchronously for AddComponent in EditMode tests -- set it directly.
        typeof(HoardEndingManager)
            .GetField("pieceSpawner", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(endingManager, fixture.PieceSpawner);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(endingManager.winScreen);
        Object.DestroyImmediate(endingManagerObject);
        fixture.Teardown();
    }

    [Test]
    public void CheckNoMoves_TrueWhenThePlayerHasNoPiecesLeft()
    {
        Assert.IsTrue(endingManager.CheckNoMoves(playerIndex: 0));
    }

    [Test]
    public void CheckNoMoves_FalseWhenAnyPieceHasALegalMove()
    {
        fixture.PlacePiece<Rook>(3, 3, playerIndex: 0, faction: Faction.Nephite);

        Assert.IsFalse(endingManager.CheckNoMoves(playerIndex: 0));
    }

    [Test]
    public void CheckNoMoves_TrueWhenEveryPieceIsCompletelyBlocked()
    {
        // A knight surrounded on all 8 of its L-move destinations by friendly pieces
        // has pieces on the board but literally nowhere to go.
        fixture.PlacePiece<Knight>(3, 3, playerIndex: 0, faction: Faction.Nephite);
        var blockedSquares = new[]
        {
            (5, 4), (1, 4), (5, 2), (1, 2), (4, 5), (2, 5), (4, 1), (2, 1)
        };
        foreach (var (x, y) in blockedSquares)
        {
            fixture.PlacePiece(x, y, playerIndex: 0, faction: Faction.Nephite);
        }

        Assert.IsTrue(endingManager.CheckNoMoves(playerIndex: 0));
    }

    [Test]
    public void CheckNoMoves_OnlyLooksAtTheRequestedPlayer()
    {
        fixture.PlacePiece<Rook>(3, 3, playerIndex: 1, faction: Faction.Lamanite);

        Assert.IsTrue(endingManager.CheckNoMoves(playerIndex: 0));
    }
}
