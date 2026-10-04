using NUnit.Framework;

public class ThreatCoordinatorTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void RebuildSilently_RecordsEveryPiecesDeclaredThreats()
    {
        fixture.PlacePiece<Rook>(0, 0, playerIndex: 0, faction: Faction.Nephite);

        fixture.RebuildThreats();

        var threatenedTile = fixture.TurnProgresser.tiles[0, 5].GetComponent<TileThreats>();
        Assert.IsTrue(threatenedTile.IsThreatenedBy(Faction.Nephite));
    }

    [Test]
    public void RebuildSilently_ClearsStaleThreatsFromBeforeAPieceMoved()
    {
        var rook = fixture.PlacePiece<Rook>(0, 0, playerIndex: 0, faction: Faction.Nephite);
        fixture.RebuildThreats();

        // Move the rook away from the file it was just threatening
        fixture.TurnProgresser.tiles[0, 0].piece = null;
        rook.transform.position = new UnityEngine.Vector3(7, 7, 0);
        fixture.TurnProgresser.tiles[7, 7].piece = rook;

        fixture.RebuildThreats();

        var formerlyThreatenedTile = fixture.TurnProgresser.tiles[0, 5].GetComponent<TileThreats>();
        Assert.IsFalse(formerlyThreatenedTile.IsThreatenedBy(Faction.Nephite));
    }

    [Test]
    public void RebuildSilently_DoesNotRecordThreatsFromInanimatePieces()
    {
        // A wounded StriplingWarrior goes Inanimate and stops threatening anything,
        // even though it's still sitting on the board (see Piece.DeclareThreats()).
        var warrior = fixture.PlaceStriplingWarrior(3, 3, faction: Faction.Nephite);
        warrior.faction = Faction.Inanimate;

        fixture.RebuildThreats();

        var adjacentTile = fixture.TurnProgresser.tiles[4, 4].GetComponent<TileThreats>();
        Assert.AreEqual(0, adjacentTile.threatenedBy.Count);
    }
}
