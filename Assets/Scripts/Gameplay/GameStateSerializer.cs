using System.Collections.Generic;
using UnityEngine;

public class PieceStateDto
{
    public string prefabName;
    public int playerIndex;
    public int x;
    public int y;
    public bool firstTurnTaken;
}

public class GameStateDto
{
    public int boardSize;
    public int currentTurnIndex;
    public List<PieceStateDto> pieces = new();
}

// Reconstructs/captures a board from the live TurnProgresser tile grid so it can be
// pushed to and pulled from a remote store (see CorrespondenceTurnProgresser).
public static class GameStateSerializer
{
    public static GameStateDto Serialize(TurnProgresser turnManager, int currentTurnIndex)
    {
        var dto = new GameStateDto
        {
            boardSize = turnManager.boardSize,
            currentTurnIndex = currentTurnIndex
        };

        for (int x = 0; x < turnManager.boardSize; x++)
        {
            for (int y = 0; y < turnManager.boardSize; y++)
            {
                var piece = turnManager.tiles[x, y].piece;
                if (piece == null) continue;

                dto.pieces.Add(new PieceStateDto
                {
                    prefabName = piece.prefabName,
                    playerIndex = piece.playerIndex,
                    x = x,
                    y = y,
                    firstTurnTaken = piece.firstTurnTaken
                });
            }
        }

        return dto;
    }

    // Destroys every piece currently on the board and respawns from the saved state.
    public static void Apply(GameStateDto state, TurnProgresser turnManager, PieceSpawner pieceSpawner)
    {
        ClearBoard(turnManager, pieceSpawner);

        foreach (var pieceState in state.pieces)
        {
            var prefab = Resources.Load<GameObject>($"Prefabs/{pieceState.prefabName}");
            if (prefab == null)
            {
                Debug.LogError($"GameStateSerializer: failed to load prefab '{pieceState.prefabName}'");
                continue;
            }

            if (pieceState.playerIndex < 0 || pieceState.playerIndex >= pieceSpawner.players.Length)
            {
                Debug.LogError($"GameStateSerializer: piece '{pieceState.prefabName}' at ({pieceState.x},{pieceState.y}) has playerIndex {pieceState.playerIndex}, but pieceSpawner.players only has {pieceSpawner.players.Length} entries. Skipping.");
                continue;
            }

            var position = new Vector2(pieceState.x, pieceState.y);
            var pieceScript = pieceSpawner.SpawnPiece(prefab, position, pieceState.playerIndex);
            pieceScript.firstTurnTaken = pieceState.firstTurnTaken;
        }
    }

    // Destroys pieces directly rather than calling Piece.Die() -- Die() is a gameplay
    // event (capture effects, StriplingWarrior's wounding instead of dying, etc.) and
    // assumes a move is actually in progress. This is a hard reset, not a capture.
    static void ClearBoard(TurnProgresser turnManager, PieceSpawner pieceSpawner)
    {
        for (int x = 0; x < turnManager.boardSize; x++)
        {
            for (int y = 0; y < turnManager.boardSize; y++)
            {
                var tile = turnManager.tiles[x, y];
                if (tile.piece == null) continue;

                pieceSpawner.players[tile.piece.playerIndex].pieces.Remove(tile.piece);
                Object.Destroy(tile.piece.gameObject);
                tile.piece = null;
            }
        }
    }
}
