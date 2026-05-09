using UnityEngine;

/// <summary>
/// Temporarily mutates board tile state for simulation purposes.
/// No animations, no events, no game logic — just tile data.
/// The backbone of chess AI: simulate a move, evaluate, undo.
/// </summary>
public static class BoardSimulator
{
    // 🚦 True while a simulation is in progress — prevents King.GetMoves() from recursing into another simulation
    public static bool IsSimulating { get; private set; }

    // ♟️ Temporarily move a piece on the tile grid. Returns the piece that was on the destination (if any).
    public static Piece SimulateMove(Vector2Int from, Vector2Int to)
    {
        var tiles = TurnManager.Instance.tiles;

        var fromTile = tiles[from.x, from.y];
        var toTile = tiles[to.x, to.y];

        Piece displaced = toTile.piece;

        toTile.piece = fromTile.piece;
        fromTile.piece = null;

        IsSimulating = true;
        return displaced;
    }

    // ↩️ Restore the tile grid to its state before SimulateMove was called
    public static void UndoSimulate(Vector2Int from, Vector2Int to, Piece displaced)
    {
        var tiles = TurnManager.Instance.tiles;

        tiles[from.x, from.y].piece = tiles[to.x, to.y].piece;
        tiles[to.x, to.y].piece = displaced;

        IsSimulating = false;
    }
}
