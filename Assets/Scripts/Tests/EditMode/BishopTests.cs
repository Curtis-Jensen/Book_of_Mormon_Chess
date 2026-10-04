using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class BishopTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void Bishop_OnEmptyBoard_CanSlideAlongAllFourDiagonals()
    {
        var bishop = fixture.PlacePiece<Bishop>(3, 3);
        var moves = bishop.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(0, 0));
        CollectionAssert.Contains(moves, new Vector2Int(6, 6));
        CollectionAssert.Contains(moves, new Vector2Int(0, 6));
        CollectionAssert.Contains(moves, new Vector2Int(6, 0));
    }

    [Test]
    public void Bishop_CannotMoveOrthogonally()
    {
        var bishop = fixture.PlacePiece<Bishop>(3, 3);
        var moves = bishop.GetMoves();

        Assert.IsFalse(moves.Any(m => m.x == 3 || m.y == 3));
    }

    [Test]
    public void Bishop_IsBlockedByAFriendlyPieceAndCannotCaptureIt()
    {
        var bishop = fixture.PlacePiece<Bishop>(3, 3);
        fixture.PlacePiece(5, 5, playerIndex: 0);

        var moves = bishop.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(4, 4));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(5, 5));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(6, 6));
    }

    [Test]
    public void Bishop_CanCaptureAnEnemyPieceButNotSlidePastIt()
    {
        var bishop = fixture.PlacePiece<Bishop>(3, 3);
        fixture.PlacePiece(5, 5, playerIndex: 1);

        var moves = bishop.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(5, 5));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(6, 6));
    }
}
