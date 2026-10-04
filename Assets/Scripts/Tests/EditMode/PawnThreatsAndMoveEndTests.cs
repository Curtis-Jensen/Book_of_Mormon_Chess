using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PawnThreatsAndMoveEndTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void DeclareThreats_MarksBothDiagonalAttackSquares_EvenWhenEmpty()
    {
        // Pawns threaten their diagonals whether or not an enemy is actually
        // standing there yet -- unlike GetMoves(), which only offers a diagonal as
        // a *move* once there's something to capture.
        var pawn = fixture.PlacePiece<Pawn>(3, 3, faction: Faction.Nephite);

        pawn.DeclareThreats();

        Assert.IsTrue(fixture.TurnProgresser.tiles[2, 4].GetComponent<TileThreats>().IsThreatenedBy(Faction.Nephite));
        Assert.IsTrue(fixture.TurnProgresser.tiles[4, 4].GetComponent<TileThreats>().IsThreatenedBy(Faction.Nephite));
    }

    [Test]
    public void DeclareThreats_DoesNotMarkTheForwardSquare()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 3, faction: Faction.Nephite);

        pawn.DeclareThreats();

        Assert.IsFalse(fixture.TurnProgresser.tiles[3, 4].GetComponent<TileThreats>().IsThreatenedBy(Faction.Nephite));
    }

    [Test]
    public void DeclareThreats_LamaniteAttacksDownward()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 3, faction: Faction.Lamanite);

        pawn.DeclareThreats();

        Assert.IsTrue(fixture.TurnProgresser.tiles[2, 2].GetComponent<TileThreats>().IsThreatenedBy(Faction.Lamanite));
        Assert.IsTrue(fixture.TurnProgresser.tiles[4, 2].GetComponent<TileThreats>().IsThreatenedBy(Faction.Lamanite));
    }

    [Test]
    public void MoveEnd_SetsFirstTurnTaken()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 1, faction: Faction.Nephite);

        // endRow defaults to 0 (Start() never ran to set it from PlayerPrefs), and
        // this pawn is at y=1, so MoveEnd's promotion check is a safe no-op here.
        pawn.MoveEnd();

        Assert.IsTrue(pawn.firstTurnTaken);
    }

    [Test]
    public void MoveEnd_OnAnOrdinaryPiece_SetsFirstTurnTaken()
    {
        var piece = fixture.PlacePiece(3, 1);
        piece.MoveEnd();
        Assert.IsTrue(piece.firstTurnTaken);
    }

    [Test]
    public void MoveEnd_DoesNotPromoteBeforeReachingTheEndRow()
    {
        var pawn = fixture.PlacePiece<Pawn>(3, 1, faction: Faction.Nephite);
        typeof(Pawn).GetField("endRow", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(pawn, 7);

        // If this incorrectly tried to promote, QueenPromotion() would throw looking
        // for a PieceSpawner/queenPrefab -- reaching this line at all is the assertion.
        pawn.MoveEnd();

        Assert.IsTrue(pawn.firstTurnTaken);
    }
}
