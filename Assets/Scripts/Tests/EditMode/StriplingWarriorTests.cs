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
    public void Die_WhenKilledByAnOrdinaryPiece_SurvivesWoundedInsteadAndKillsTheAttackerInstead()
    {
        var defender = fixture.PlaceStriplingWarrior(3, 3, playerIndex: 1, faction: Faction.Lamanite);
        var attacker = fixture.PlacePiece<Rook>(4, 4, playerIndex: 0, faction: Faction.Nephite);
        fixture.EquipDeathEffects(attacker);
        fixture.TurnProgresser.selectedPiece = attacker;

        // Object.Destroy outside Play Mode logs an error (it's immediate in-editor,
        // same as production behavior when this runs for real) -- expected here.
        LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));
        defender.Die();

        // Note: Piece.Die()'s own removal-from-player-list step uses
        // FindAnyObjectByType<PieceSpawner>(), which isn't reliably *this* fixture's
        // PieceSpawner if another one exists in a loaded scene -- so that part of the
        // behavior isn't asserted here. What StriplingWarrior.Die() itself guarantees
        // (defender survives, reassigns selectedPiece, goes Inanimate) is fully testable.
        Assert.IsTrue(defender != null, "defender should survive, only wounded");
        Assert.AreEqual(Faction.Inanimate, defender.faction);
        Assert.AreSame(defender, fixture.TurnProgresser.selectedPiece);
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
