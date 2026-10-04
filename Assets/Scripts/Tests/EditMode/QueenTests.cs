using NUnit.Framework;
using UnityEngine;

public class QueenTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void Queen_OnEmptyBoard_CombinesRookAndBishopMoves()
    {
        var queen = fixture.PlacePiece<Queen>(3, 3);
        var moves = queen.GetMoves();

        // Orthogonal, like a rook
        CollectionAssert.Contains(moves, new Vector2Int(0, 3));
        CollectionAssert.Contains(moves, new Vector2Int(3, 7));
        // Diagonal, like a bishop
        CollectionAssert.Contains(moves, new Vector2Int(0, 0));
        CollectionAssert.Contains(moves, new Vector2Int(6, 6));

        // Rook component: all 7 other squares on the rank + all 7 on the file = 14.
        // Bishop component from (3,3) on an 8x8 board: diagonal run lengths to each
        // edge are 4 (up-right), 3 (down-left), 3 (up-left), 3 (down-right) = 13.
        Assert.AreEqual(27, moves.Count);
    }

    [Test]
    public void Queen_IsBlockedByAFriendlyPieceAndCannotCaptureIt()
    {
        var queen = fixture.PlacePiece<Queen>(3, 3);
        fixture.PlacePiece(3, 5, playerIndex: 0);

        var moves = queen.GetMoves();

        CollectionAssert.DoesNotContain(moves, new Vector2Int(3, 5));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(3, 6));
    }

    [Test]
    public void Queen_CanCaptureAnEnemyPieceButNotSlidePastIt()
    {
        var queen = fixture.PlacePiece<Queen>(3, 3);
        fixture.PlacePiece(5, 5, playerIndex: 1);

        var moves = queen.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(5, 5));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(6, 6));
    }
}
