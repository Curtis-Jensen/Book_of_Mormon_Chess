using NUnit.Framework;

public class TileThreatsTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void IsThreatenedBy_TrueWhenAThreatenerOfThatFactionIsPresent()
    {
        var threats = fixture.TurnProgresser.tiles[0, 0].GetComponent<TileThreats>();
        var enemy = fixture.PlacePiece(5, 5, faction: Faction.Lamanite);
        threats.threatenedBy.Add(enemy);

        Assert.IsTrue(threats.IsThreatenedBy(Faction.Lamanite));
    }

    [Test]
    public void IsThreatenedBy_FalseForAFactionNotInTheList()
    {
        var threats = fixture.TurnProgresser.tiles[0, 0].GetComponent<TileThreats>();
        var enemy = fixture.PlacePiece(5, 5, faction: Faction.Lamanite);
        threats.threatenedBy.Add(enemy);

        Assert.IsFalse(threats.IsThreatenedBy(Faction.Nephite));
    }

    [Test]
    public void IsThreatenedBy_FalseWhenEmpty()
    {
        var threats = fixture.TurnProgresser.tiles[0, 0].GetComponent<TileThreats>();
        Assert.IsFalse(threats.IsThreatenedBy(Faction.Nephite));
        Assert.IsFalse(threats.IsThreatenedBy(Faction.Lamanite));
    }

    [Test]
    public void Clear_RemovesAllPreviouslyRecordedThreats()
    {
        var threats = fixture.TurnProgresser.tiles[0, 0].GetComponent<TileThreats>();
        var enemy = fixture.PlacePiece(5, 5, faction: Faction.Lamanite);
        threats.threatenedBy.Add(enemy);

        threats.Clear();

        Assert.IsFalse(threats.IsThreatenedBy(Faction.Lamanite));
        Assert.AreEqual(0, threats.threatenedBy.Count);
    }
}
