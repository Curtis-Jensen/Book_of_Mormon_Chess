using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class RookTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void Rook_OnEmptyBoard_CanSlideToEveryTileOnItsRankAndFile()
    {
        var rook = fixture.PlacePiece<Rook>(3, 3);
        var moves = rook.GetMoves();

        Assert.AreEqual((fixture.BoardSize - 1) * 2, moves.Count);
        CollectionAssert.Contains(moves, new Vector2Int(0, 3));
        CollectionAssert.Contains(moves, new Vector2Int(7, 3));
        CollectionAssert.Contains(moves, new Vector2Int(3, 0));
        CollectionAssert.Contains(moves, new Vector2Int(3, 7));
    }

    [Test]
    public void Rook_CannotMoveDiagonally()
    {
        var rook = fixture.PlacePiece<Rook>(3, 3);
        var moves = rook.GetMoves();

        Assert.IsFalse(moves.Any(m => m.x != 3 && m.y != 3));
    }

    [Test]
    public void Rook_IsBlockedByAFriendlyPieceAndCannotCaptureIt()
    {
        var rook = fixture.PlacePiece<Rook>(3, 3);
        fixture.PlacePiece(3, 5, playerIndex: 0);

        var moves = rook.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(3, 4));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(3, 5));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(3, 6));
    }

    [Test]
    public void Rook_CanCaptureAnEnemyPieceButNotSlidePastIt()
    {
        var rook = fixture.PlacePiece<Rook>(3, 3);
        fixture.PlacePiece(3, 5, playerIndex: 1);

        var moves = rook.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(3, 5));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(3, 6));
    }
}
