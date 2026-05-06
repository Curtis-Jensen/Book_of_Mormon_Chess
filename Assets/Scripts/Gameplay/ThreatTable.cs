using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which squares are threatened by which pieces.
/// Rebuilt after every move. Read by IsInCheck(), king move filtering, and (eventually) hover highlighting.
/// </summary>
public class ThreatTable
{
    // 🗺️ Maps each board position to the list of pieces that can attack it
    Dictionary<Vector2Int, List<Piece>> threatenedSquares = new();

    public void Rebuild(PieceSpawner pieceSpawner)
    {
        threatenedSquares.Clear();

        // 🧩 Get all pieces from all players
        foreach (var player in pieceSpawner.players)
        {
            foreach (var piece in player.pieces)
            {
                // ⏭️ Skip inanimate pieces — they don't threaten squares
                if (piece.faction == Faction.Inanimate) continue;

                // 🔍 Mark every square this piece can move to as threatened
                foreach (var square in piece.GetMoves())
                {
                    if (!threatenedSquares.ContainsKey(square))
                        threatenedSquares[square] = new List<Piece>();

                    threatenedSquares[square].Add(piece);
                }
            }
        }
    }

    public bool IsThreatenedBy(Vector2Int square, Faction byFaction)
    {
        if (!threatenedSquares.TryGetValue(square, out var threats)) return false;

        foreach (var piece in threats)
        {
            if (piece.faction == byFaction) return true;
        }

        return false;
    }

    public List<Piece> GetThreats(Vector2Int square)
    {
        if (threatenedSquares.TryGetValue(square, out var threats))
            return threats;

        return new List<Piece>();
    }
}
