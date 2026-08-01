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
        ClearBoard(turnManager);

        foreach (var pieceState in state.pieces)
        {
            var prefab = Resources.Load<GameObject>($"Prefabs/{pieceState.prefabName}");
            if (prefab == null)
            {
                Debug.LogError($"GameStateSerializer: failed to load prefab '{pieceState.prefabName}'");
                continue;
            }

            var position = new Vector2(pieceState.x, pieceState.y);
            var pieceScript = pieceSpawner.SpawnPiece(prefab, position, pieceState.playerIndex);
            pieceScript.firstTurnTaken = pieceState.firstTurnTaken;
        }
    }

    static void ClearBoard(TurnProgresser turnManager)
    {
        for (int x = 0; x < turnManager.boardSize; x++)
        {
            for (int y = 0; y < turnManager.boardSize; y++)
            {
                var tile = turnManager.tiles[x, y];
                if (tile.piece == null) continue;

                tile.piece.Die(instantiateEffects: false);
                tile.piece = null;
            }
        }
    }
}
