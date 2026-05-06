using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lives on each tile. Holds the list of pieces currently threatening this square.
/// Data is written by Piece.DeclareThreats() and cleared by ThreatCoordinator each move.
/// </summary>
public class TileThreats : MonoBehaviour
{
    public List<Piece> threatenedBy = new();

    public void Clear()
    {
        threatenedBy.Clear();
    }

    public bool IsThreatenedBy(Faction faction)
    {
        foreach (var piece in threatenedBy)
        {
            if (piece.faction == faction) return true;
        }
        return false;
    }
}
