using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class KingTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void King_InTheCornerWithNoThreats_HasExactlyThreeMoves()
    {
        var king = fixture.PlacePiece<King>(0, 0, faction: Faction.Nephite);
        var moves = king.GetMoves();

        Assert.AreEqual(3, moves.Count);
        CollectionAssert.Contains(moves, new Vector2Int(1, 0));
        CollectionAssert.Contains(moves, new Vector2Int(0, 1));
        CollectionAssert.Contains(moves, new Vector2Int(1, 1));
    }

    [Test]
    public void King_CannotCaptureItsOwnPiece()
    {
        var king = fixture.PlacePiece<King>(3, 3, faction: Faction.Nephite);
        fixture.PlacePiece(4, 3, playerIndex: 0, faction: Faction.Nephite);

        var moves = king.GetMoves();

        CollectionAssert.DoesNotContain(moves, new Vector2Int(4, 3));
    }

    [Test]
    public void King_CannotMoveIntoASquareThreatenedByAnEnemyRook()
    {
        var king = fixture.PlacePiece<King>(0, 0, faction: Faction.Nephite);
        // Same file as (0,1): an unobstructed rook threatens the whole file.
        fixture.PlacePiece<Rook>(0, 7, playerIndex: 1, faction: Faction.Lamanite);

        var moves = king.GetMoves();

        CollectionAssert.DoesNotContain(moves, new Vector2Int(0, 1));
        CollectionAssert.Contains(moves, new Vector2Int(1, 0));
        CollectionAssert.Contains(moves, new Vector2Int(1, 1));
    }

    [Test]
    public void IsInCheck_TrueWhenEnemyThreatensItsTile()
    {
        var king = fixture.PlacePiece<King>(3, 3, faction: Faction.Nephite);
        var enemy = fixture.PlacePiece(2, 2, playerIndex: 1, faction: Faction.Lamanite);
        fixture.TurnProgresser.tiles[3, 3].GetComponent<TileThreats>().threatenedBy.Add(enemy);

        // King.IsInCheck() logs an error as a (loud, intentional) debug signal when check happens.
        LogAssert.Expect(LogType.Error, "Check!");
        king.IsInCheck();

        Assert.IsTrue(king.inCheck);
    }

    [Test]
    public void IsInCheck_FalseWhenNoThreatsReachItsTile()
    {
        var king = fixture.PlacePiece<King>(3, 3, faction: Faction.Nephite);

        king.IsInCheck();

        Assert.IsFalse(king.inCheck);
    }

    [Test]
    public void IsInCheck_IgnoresThreatsFromItsOwnFaction()
    {
        var king = fixture.PlacePiece<King>(3, 3, faction: Faction.Nephite);
        var friendly = fixture.PlacePiece(2, 2, playerIndex: 0, faction: Faction.Nephite);
        fixture.TurnProgresser.tiles[3, 3].GetComponent<TileThreats>().threatenedBy.Add(friendly);

        king.IsInCheck();

        Assert.IsFalse(king.inCheck);
    }
}
