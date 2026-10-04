using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class PawnTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void NephitePawn_MovesForwardTwoSquaresOnFirstMove()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 1, faction: Faction.Nephite);
        var moves = pawn.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(3, 2));
        CollectionAssert.Contains(moves, new Vector2Int(3, 3));
    }

    [Test]
    public void NephitePawn_MovesOnlyOneSquareAfterFirstMove()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 1, faction: Faction.Nephite, firstTurnTaken: true);
        var moves = pawn.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(3, 2));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(3, 3));
    }

    [Test]
    public void LamanitePawn_MovesDownTheBoard()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 6, faction: Faction.Lamanite);
        var moves = pawn.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(3, 5));
        CollectionAssert.Contains(moves, new Vector2Int(3, 4));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(3, 7));
    }

    [Test]
    public void Pawn_CannotMoveStraightIntoAnOccupiedTile()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 1, faction: Faction.Nephite);
        fixture.PlacePiece(3, 2, playerIndex: 1, faction: Faction.Lamanite);

        var moves = pawn.GetMoves();

        Assert.IsFalse(moves.Any(m => m.x == 3));
    }

    [Test]
    public void Pawn_CannotDoubleMoveIfForwardTileIsBlocked()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 1, faction: Faction.Nephite);
        fixture.PlacePiece(3, 2, playerIndex: 1, faction: Faction.Lamanite);

        var moves = pawn.GetMoves();

        CollectionAssert.DoesNotContain(moves, new Vector2Int(3, 3));
    }

    [Test]
    public void Pawn_CapturesDiagonallyOnlyWhenEnemyPresent()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 1, faction: Faction.Nephite, firstTurnTaken: true);
        fixture.PlacePiece(4, 2, playerIndex: 1, faction: Faction.Lamanite);

        var moves = pawn.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(4, 2));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(2, 2));
    }

    [Test]
    public void Pawn_CannotCaptureDiagonallyWithoutAnEnemyThere()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 1, faction: Faction.Nephite, firstTurnTaken: true);
        var moves = pawn.GetMoves();

        CollectionAssert.DoesNotContain(moves, new Vector2Int(4, 2));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(2, 2));
    }

    [Test]
    public void Pawn_CannotCaptureDiagonallyOntoAFriendlyPiece()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 1, faction: Faction.Nephite, firstTurnTaken: true);
        fixture.PlacePiece(4, 2, playerIndex: 0, faction: Faction.Nephite);

        var moves = pawn.GetMoves();

        CollectionAssert.DoesNotContain(moves, new Vector2Int(4, 2));
    }
}
