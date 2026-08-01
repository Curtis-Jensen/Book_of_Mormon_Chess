using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class King : Piece
{
    public bool inCheck;

    //The four cardinal directions, and diagonal moves
    Vector2Int[] moveDirections =
        { new( 1,  0),
          new(-1,  0),
          new(0,   1),
          new(0,  -1),
          new( 1,  1),
          new(-1, -1),
          new(-1,  1),
          new( 1, -1)};

    protected override void Start()
    {
        ThreatCoordinator.Instance.OnThreatsReady += IsInCheck;

        base.Start();
    }

    public void IsInCheck()
    {
        var kingPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        // 🗺️ Read directly from this king's tile — ThreatCoordinator already populated it
        var myTile = TurnProgresser.Instance.tiles[kingPosition.x, kingPosition.y];
        inCheck = myTile.GetComponent<TileThreats>().IsThreatenedBy(EnemyFaction());

        if (inCheck) Debug.LogError("Check!");
    }

    // ⚔️ Returns the opposing faction so IsInCheck knows whose threats to check
    Faction EnemyFaction() => faction == Faction.Nephite ? Faction.Lamanite : Faction.Nephite;

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();
        var kingPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        foreach (var moveDirection in moveDirections)
        {
            Vector2Int candidate = new(kingPosition.x + moveDirection.x, kingPosition.y + moveDirection.y);

            if (!IsTileEmpty(candidate) && !IsEnemyPiece(candidate)) continue;

            // 🔬 Simulate the king moving there and check if it would be threatened
            if (IsSafeAfterSimulation(kingPosition, candidate))
                validMoves.Add(candidate);
        }

        return validMoves;
    }

    // 🔬 Temporarily moves the king, rebuilds threats, checks safety, then undoes
    bool IsSafeAfterSimulation(Vector2Int from, Vector2Int to)
    {
        // 🚦 If we're already inside a simulation, skip — prevents infinite recursion
        if (BoardSimulator.IsSimulating) return true;

        Piece displaced = BoardSimulator.SimulateMove(from, to);

        ThreatCoordinator.Instance.RebuildSilently();
        bool isSafe = !TurnProgresser.Instance.tiles[to.x, to.y].GetComponent<TileThreats>().IsThreatenedBy(EnemyFaction());

        BoardSimulator.UndoSimulate(from, to, displaced);

        return isSafe;
    }

    public override void Die(bool instantiateEffects = true)
    {
        var endingManager = FindAnyObjectByType<EndingManager>();
        endingManager.EndGame(playerIndex);

        base.Die();
    }
}
