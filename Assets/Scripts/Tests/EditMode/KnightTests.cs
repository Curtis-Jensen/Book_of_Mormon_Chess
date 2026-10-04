using NUnit.Framework;
using UnityEngine;

public class KnightTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void Knight_OnEmptyBoard_HasAllEightLMoves()
    {
        var knight = fixture.PlacePiece<Knight>(3, 3);
        var moves = knight.GetMoves();

        Assert.AreEqual(8, moves.Count);
        CollectionAssert.Contains(moves, new Vector2Int(5, 4));
        CollectionAssert.Contains(moves, new Vector2Int(1, 2));
    }

    [Test]
    public void Knight_JumpsOverPiecesBetweenItAndItsDestination()
    {
        var knight = fixture.PlacePiece<Knight>(3, 3);
        // Directly adjacent to the knight, in the path an L-move would "pass over"
        fixture.PlacePiece(3, 4, playerIndex: 1);
        fixture.PlacePiece(4, 3, playerIndex: 1);

        var moves = knight.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(5, 4));
    }

    [Test]
    public void Knight_CannotLandOnAFriendlyPiece()
    {
        var knight = fixture.PlacePiece<Knight>(3, 3);
        fixture.PlacePiece(5, 4, playerIndex: 0);

        var moves = knight.GetMoves();

        CollectionAssert.DoesNotContain(moves, new Vector2Int(5, 4));
    }

    [Test]
    public void Knight_CanCaptureAnEnemyPieceAtItsDestination()
    {
        var knight = fixture.PlacePiece<Knight>(3, 3);
        fixture.PlacePiece(5, 4, playerIndex: 1);

        var moves = knight.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(5, 4));
    }

    [Test]
    public void Knight_InACornerHasFewerMoves()
    {
        var knight = fixture.PlacePiece<Knight>(0, 0);
        var moves = knight.GetMoves();

        Assert.AreEqual(2, moves.Count);
    }
}
