using System.Linq;
using NUnit.Framework;

public class GameStateSerializerTests
{
    private BoardTestFixture fixture;

    [SetUp]
    public void SetUp()
    {
        fixture = new BoardTestFixture();
    }

    [TearDown]
    public void TearDown()
    {
        fixture.Teardown();
    }

    [Test]
    public void Serialize_CapturesBoardSizeAndTurnIndex()
    {
        var dto = GameStateSerializer.Serialize(fixture.TurnProgresser, currentTurnIndex: 3);

        Assert.AreEqual(fixture.BoardSize, dto.boardSize);
        Assert.AreEqual(3, dto.currentTurnIndex);
    }

    [Test]
    public void Serialize_SkipsEmptyTilesAndIncludesOnlyOccupiedOnes()
    {
        fixture.PlacePiece(0, 0, prefabName: "Rook");
        fixture.PlacePiece(2, 5, prefabName: "Knight");

        var dto = GameStateSerializer.Serialize(fixture.TurnProgresser, currentTurnIndex: 0);

        Assert.AreEqual(2, dto.pieces.Count);
    }

    [Test]
    public void Serialize_RecordsPiecePositionPlayerAndPrefabName()
    {
        fixture.PlacePiece(4, 6, playerIndex: 1, prefabName: "StriplingWarrior");

        var dto = GameStateSerializer.Serialize(fixture.TurnProgresser, currentTurnIndex: 0);
        var piece = dto.pieces.Single();

        Assert.AreEqual(4, piece.x);
        Assert.AreEqual(6, piece.y);
        Assert.AreEqual(1, piece.playerIndex);
        Assert.AreEqual("StriplingWarrior", piece.prefabName);
    }

    [Test]
    public void Serialize_PreservesFirstTurnTakenFlag()
    {
        fixture.PlacePiece(0, 1, firstTurnTaken: true);
        fixture.PlacePiece(0, 2, firstTurnTaken: false);

        var dto = GameStateSerializer.Serialize(fixture.TurnProgresser, currentTurnIndex: 0);

        Assert.IsTrue(dto.pieces.Single(p => p.y == 1).firstTurnTaken);
        Assert.IsFalse(dto.pieces.Single(p => p.y == 2).firstTurnTaken);
    }
}
