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
        ThreatTable.Instance.OnThreatTableUpdated += IsInCheck;

        base.Start();
    }

    public void IsInCheck()
    {
        var kingPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        // 🗺️ Delegate to ThreatTable — it already knows what threatens every square
        inCheck = ThreatTable.Instance.IsThreatenedBy(kingPosition, EnemyFaction());

        if (inCheck) Debug.LogError("Check!");
    }

    // ⚔️ Returns the opposing faction so IsInCheck knows whose threats to check
    Faction EnemyFaction() => faction == Faction.Nephite ? Faction.Lamanite : Faction.Nephite;

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        foreach (var moveDirection in moveDirections)
        {
            Vector2Int newMove =
                new((int)transform.position.x + moveDirection.x, (int)transform.position.y + moveDirection.y);

            bool emptyOrEnemy =
                IsTileEmpty(newMove) || IsEnemyPiece(newMove);

            if (emptyOrEnemy)
            {
                validMoves.Add(newMove);
            }
        }

        return validMoves;
    }

    public override void Die(bool instantiateEffects = true)
    {
        var endingManager = FindAnyObjectByType<EndingManager>();
        endingManager.EndGame(playerIndex);

        base.Die();
    }
}
