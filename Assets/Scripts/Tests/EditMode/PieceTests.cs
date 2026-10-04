using NUnit.Framework;
using UnityEngine;

// Covers the shared base-class checks every piece's GetMoves() relies on.
public class PieceTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp() => fixture = new BoardTestFixture();

    [TearDown]
    public void TearDown() => fixture.Teardown();

    [Test]
    public void IsTileEmpty_TrueForEmptyTile()
    {
        var piece = fixture.PlacePiece(0, 0);
        Assert.IsTrue(piece.IsTileEmpty(new Vector2Int(3, 3)));
    }

    [Test]
    public void IsTileEmpty_FalseWhenOccupied()
    {
        var piece = fixture.PlacePiece(0, 0);
        fixture.PlacePiece(3, 3);
        Assert.IsFalse(piece.IsTileEmpty(new Vector2Int(3, 3)));
    }

    [Test]
    public void IsTileEmpty_FalseOutOfBounds()
    {
        var piece = fixture.PlacePiece(0, 0);
        Assert.IsFalse(piece.IsTileEmpty(new Vector2Int(-1, 0)));
        Assert.IsFalse(piece.IsTileEmpty(new Vector2Int(0, fixture.BoardSize)));
    }

    [Test]
    public void IsEnemyPiece_TrueForDifferentFaction()
    {
        var piece = fixture.PlacePiece(0, 0, playerIndex: 0, faction: Faction.Nephite);
        fixture.PlacePiece(3, 3, playerIndex: 1, faction: Faction.Lamanite);

        Assert.IsTrue(piece.IsEnemyPiece(new Vector2Int(3, 3)));
    }

    [Test]
    public void IsEnemyPiece_FalseForSameFaction()
    {
        var piece = fixture.PlacePiece(0, 0, faction: Faction.Nephite);
        fixture.PlacePiece(3, 3, playerIndex: 1, faction: Faction.Nephite);

        Assert.IsFalse(piece.IsEnemyPiece(new Vector2Int(3, 3)));
    }

    [Test]
    public void IsEnemyPiece_FalseForInanimateFaction()
    {
        var piece = fixture.PlacePiece(0, 0, faction: Faction.Nephite);
        fixture.PlacePiece(3, 3, playerIndex: 1, faction: Faction.Inanimate);

        Assert.IsFalse(piece.IsEnemyPiece(new Vector2Int(3, 3)));
    }

    [Test]
    public void IsEnemyPiece_FalseForEmptyTile()
    {
        var piece = fixture.PlacePiece(0, 0);
        Assert.IsFalse(piece.IsEnemyPiece(new Vector2Int(5, 5)));
    }

    [Test]
    public void IsEnemyPiece_FalseOutOfBounds()
    {
        var piece = fixture.PlacePiece(0, 0);
        Assert.IsFalse(piece.IsEnemyPiece(new Vector2Int(-1, 0)));
    }
}
