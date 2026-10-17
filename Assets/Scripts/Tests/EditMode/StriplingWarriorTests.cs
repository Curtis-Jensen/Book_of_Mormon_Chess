using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class StriplingWarriorTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void GetMoves_MovesLikeAKing_OneStepInAnyOfEightDirections()
    {
        var warrior = fixture.PlaceStriplingWarrior(3, 3, faction: Faction.Nephite);
        var moves = warrior.GetMoves();

        Assert.AreEqual(8, moves.Count);
        CollectionAssert.Contains(moves, new Vector2Int(4, 4));
        CollectionAssert.DoesNotContain(moves, new Vector2Int(5, 5));
    }

    [Test]
    public void IsEnemyPiece_FalseAgainstAnotherStriplingWarrior_EvenOfTheOppositeFaction()
    {
        var warrior = fixture.PlaceStriplingWarrior(3, 3, faction: Faction.Nephite);
        fixture.PlaceStriplingWarrior(4, 4, playerIndex: 1, faction: Faction.Lamanite);

        Assert.IsFalse(warrior.IsEnemyPiece(new Vector2Int(4, 4)));
    }

    [Test]
    public void GetMoves_CannotStepOntoAnotherStriplingWarriorsSquare()
    {
        var warrior = fixture.PlaceStriplingWarrior(3, 3, faction: Faction.Nephite);
        fixture.PlaceStriplingWarrior(4, 4, playerIndex: 1, faction: Faction.Lamanite);

        var moves = warrior.GetMoves();

        CollectionAssert.DoesNotContain(moves, new Vector2Int(4, 4));
    }

    [Test]
    public void GetMoves_CanCaptureAnOrdinaryEnemyPiece()
    {
        var warrior = fixture.PlaceStriplingWarrior(3, 3, faction: Faction.Nephite);
        fixture.PlacePiece(4, 4, playerIndex: 1, faction: Faction.Lamanite);

        var moves = warrior.GetMoves();

        CollectionAssert.Contains(moves, new Vector2Int(4, 4));
    }

    [Test]
    public void Die_WhenKilledByAnOrdinaryPiece_BecomesAGhostAndTheAttackerSurvives()
    {
        var defender = fixture.PlaceStriplingWarrior(3, 3, playerIndex: 1, faction: Faction.Lamanite);
        var attacker = fixture.PlacePiece<Rook>(4, 4, playerIndex: 0, faction: Faction.Nephite);
        fixture.TurnProgresser.selectedPiece = attacker;
        BoardTestFixture.SetWoundedTurns(defender, currentWoundedTurns: 0, maxWoundedTurns: 14);

        defender.Die();

        Assert.IsTrue(defender != null, "defender should survive as a ghost");
        Assert.IsTrue(attacker != null, "attacker should not be killed");
        Assert.AreEqual(Faction.Inanimate, defender.faction);
        Assert.AreSame(attacker, fixture.TurnProgresser.selectedPiece);
        Assert.Less(defender.GetComponent<SpriteRenderer>().color.a, 1f);
        Assert.AreEqual(0, BoardTestFixture.GetMoveDirections(defender).Length);
    }

    [Test]
    public void DecrementWoundedCounter_GhostStaysGhostWhileAnotherPieceOccupiesItsSquare()
    {
        var warrior = fixture.PlaceStriplingWarrior(3, 3, playerIndex: 1, faction: Faction.Lamanite);
        warrior.faction = Faction.Inanimate;
        BoardTestFixture.SetWoundedTurns(warrior, currentWoundedTurns: 1, maxWoundedTurns: 4);
        var squatter = fixture.PlacePiece<Rook>(5, 5, playerIndex: 0, faction: Faction.Nephite);
        fixture.TurnProgresser.tiles[3, 3].piece = squatter;

        warrior.DecrementWoundedCounter();
        Assert.AreEqual(Faction.Inanimate, warrior.faction);

        fixture.TurnProgresser.tiles[3, 3].piece = null;
        warrior.DecrementWoundedCounter();

        Assert.AreEqual(Faction.Lamanite, warrior.faction);
        Assert.AreSame(warrior, fixture.TurnProgresser.tiles[3, 3].piece);
    }

    [Test]
    public void Die_WhenKilledByAnotherStriplingWarrior_DiesNormally()
    {
        var defender = fixture.PlaceStriplingWarrior(3, 3, playerIndex: 1, faction: Faction.Lamanite);
        var attacker = fixture.PlaceStriplingWarrior(4, 4, playerIndex: 0, faction: Faction.Nephite);
        fixture.EquipDeathEffects(defender);
        fixture.TurnProgresser.selectedPiece = attacker;

        LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));
        defender.Die();

        // See the note in the test above re: FindAnyObjectByType<PieceSpawner>() --
        // and Object.Destroy() is a same-frame no-op outside Play Mode (just the
        // logged error we expected above), so there's no surviving-object state to
        // assert either. The LogAssert.Expect above is what actually proves this
        // branch reached base.Die() instead of the wounding branch.
        CollectionAssert.Contains(fixture.PieceSpawner.players[0].pieces, attacker);
    }

    [Test]
    public void DecrementWoundedCounter_BecomesActiveAgainOnceTheCounterReachesZero()
    {
        var warrior = fixture.PlaceStriplingWarrior(3, 3, playerIndex: 1, faction: Faction.Lamanite);
        warrior.faction = Faction.Inanimate;
        BoardTestFixture.SetWoundedTurns(warrior, currentWoundedTurns: 1, maxWoundedTurns: 4);

        warrior.DecrementWoundedCounter();

        Assert.AreEqual(Faction.Lamanite, warrior.faction);
        Assert.AreEqual(8, BoardTestFixture.GetMoveDirections(warrior).Length);
    }

    [Test]
    public void DecrementWoundedCounter_StaysWoundedUntilCounterReachesZero()
    {
        var warrior = fixture.PlaceStriplingWarrior(3, 3, playerIndex: 1, faction: Faction.Lamanite);
        warrior.faction = Faction.Inanimate;
        BoardTestFixture.SetWoundedTurns(warrior, currentWoundedTurns: 2, maxWoundedTurns: 4);

        warrior.DecrementWoundedCounter();

        Assert.AreEqual(Faction.Inanimate, warrior.faction);
    }
}
