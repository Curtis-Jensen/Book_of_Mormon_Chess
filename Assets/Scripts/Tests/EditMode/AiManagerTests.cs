using NUnit.Framework;
using UnityEngine;

public class AiManagerTests
{
    private BoardTestFixture fixture;
    private AiManager aiManager;

    [SetUp]
    public void SetUp()
    {
        fixture = new BoardTestFixture();
        aiManager = fixture.AddAiManager();
    }

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void ChooseKillingMove_PicksAMoveThatCapturesAnEnemyPiece()
    {
        fixture.PlacePiece<Rook>(0, 0, playerIndex: 0, faction: Faction.Nephite);
        fixture.PlacePiece(0, 5, playerIndex: 1, faction: Faction.Lamanite);

        var choice = aiManager.ChooseKillingMove(playerIndex: 0);

        Assert.IsNotNull(choice);
        Assert.AreEqual(new Vector2(0, 5), choice.moveTo);
        Assert.IsTrue(choice.chosenPiece.IsEnemyPiece(new Vector2Int(0, 5)));
    }

    [Test]
    public void ChooseKillingMove_ReturnsNullWhenNoCaptureIsAvailable()
    {
        fixture.PlacePiece<Rook>(0, 0, playerIndex: 0, faction: Faction.Nephite);

        var choice = aiManager.ChooseKillingMove(playerIndex: 0);

        Assert.IsNull(choice);
    }

    [Test]
    public void ChooseRandomMove_ReturnsNullWhenThePlayerHasNoPieces()
    {
        var choice = aiManager.ChooseRandomMove(playerIndex: 0);
        Assert.IsNull(choice);
    }

    [Test]
    public void ChooseRandomMove_PicksAPieceThatActuallyHasAMoveAvailable()
    {
        // A pawn boxed in on three sides has no moves; a rook in the corner always does.
        fixture.PlacePiece<Pawn>(0, 0, playerIndex: 0, faction: Faction.Nephite, firstTurnTaken: true);
        fixture.PlacePiece(0, 1, playerIndex: 0, faction: Faction.Nephite);
        fixture.PlacePiece<Rook>(7, 7, playerIndex: 0, faction: Faction.Nephite);

        var choice = aiManager.ChooseRandomMove(playerIndex: 0);

        Assert.IsNotNull(choice);
        Assert.AreSame(fixture.PieceSpawner.players[0].pieces[2], choice.chosenPiece);
    }

    [Test]
    public void ChooseMove_PrefersAKillingMoveOverARandomMove()
    {
        var rook = fixture.PlacePiece<Rook>(0, 0, playerIndex: 0, faction: Faction.Nephite);
        fixture.PlacePiece(0, 5, playerIndex: 1, faction: Faction.Lamanite);

        var choice = aiManager.ChooseMove(playerIndex: 0);

        Assert.AreSame(rook, choice.chosenPiece);
        Assert.AreEqual(new Vector2(0, 5), choice.moveTo);
    }
}
