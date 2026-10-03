using NUnit.Framework;
using UnityEngine;

public class BoardSimulatorTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp()
    {
        fixture = new BoardTestFixture();
    }

    [TearDown]
    public void TearDown()
    {
        fixture.Teardown();
    }

    [Test]
    public void SimulateMove_MovesPieceToDestinationTile()
    {
        var piece = fixture.PlacePiece(1, 1);
        var from = new Vector2Int(1, 1);
        var to = new Vector2Int(3, 3);

        BoardSimulator.SimulateMove(from, to);

        Assert.AreSame(piece, fixture.TurnProgresser.tiles[3, 3].piece);
        Assert.IsNull(fixture.TurnProgresser.tiles[1, 1].piece);
    }

    [Test]
    public void SimulateMove_ReturnsDisplacedPieceAndSetsIsSimulating()
    {
        fixture.PlacePiece(0, 0);
        var enemy = fixture.PlacePiece(0, 1, playerIndex: 1);

        var displaced = BoardSimulator.SimulateMove(new Vector2Int(0, 0), new Vector2Int(0, 1));

        Assert.AreSame(enemy, displaced);
        Assert.IsTrue(BoardSimulator.IsSimulating);
    }

    [Test]
    public void UndoSimulate_RestoresBothTilesAndClearsIsSimulating()
    {
        var mover = fixture.PlacePiece(2, 2);
        var defender = fixture.PlacePiece(2, 3, playerIndex: 1);
        var from = new Vector2Int(2, 2);
        var to = new Vector2Int(2, 3);

        var displaced = BoardSimulator.SimulateMove(from, to);
        BoardSimulator.UndoSimulate(from, to, displaced);

        Assert.AreSame(mover, fixture.TurnProgresser.tiles[2, 2].piece);
        Assert.AreSame(defender, fixture.TurnProgresser.tiles[2, 3].piece);
        Assert.IsFalse(BoardSimulator.IsSimulating);
    }

    [Test]
    public void UndoSimulate_WithNoDisplacedPiece_LeavesDestinationEmpty()
    {
        fixture.PlacePiece(4, 4);
        var from = new Vector2Int(4, 4);
        var to = new Vector2Int(4, 5);

        var displaced = BoardSimulator.SimulateMove(from, to);
        BoardSimulator.UndoSimulate(from, to, displaced);

        Assert.IsNull(fixture.TurnProgresser.tiles[4, 5].piece);
        Assert.IsNotNull(fixture.TurnProgresser.tiles[4, 4].piece);
    }
}
