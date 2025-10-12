using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class StriplingWarrior : Piece
{
    [SerializeField] int maxWoundedTurns = 2;

    //The four cardinal directions, and diagonal moves
    Vector2Int[] moveDirections;

    Vector2Int[] kingMovementPattern =
        { new( 1,  0),
          new(-1,  0),
          new(0,   1),
          new(0,  -1),
          new( 1,  1),
          new(-1, -1),
          new(-1,  1),
          new( 1, -1) };

    int currentWoundedTurns = 0;

    protected override void Start()
    {
        SetupWounding();
        base.Start();
    }

    void SetupWounding()
    {
        moveDirections = kingMovementPattern;

        maxWoundedTurns *= 2; //Convert from player turns to full turns

        TurnManager.Instance.OnMoveEnd += DecrementWoundedCounter;
    }

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        foreach (var moveDirection in moveDirections)
        {
            Vector2Int newMove =
                new((int)transform.position.x + moveDirection.x, (int)transform.position.y + moveDirection.y);

            bool emptyOrEnemy =
                TurnManager.Instance.IsTileEmpty(newMove) || TurnManager.Instance.IsEnemyPiece(newMove, teamOne);

            if (emptyOrEnemy)
            {
                validMoves.Add(newMove);
            }
        }

        return validMoves;
    }

    public override void Die()
    {
        if (TurnManager.Instance.selectedPiece.GetType() == typeof(StriplingWarrior))
        {
            base.Die();
            return;
        }
        TurnManager.Instance.selectedPiece.Die();
        TurnManager.Instance.selectedPiece = this;

        moveDirections = new Vector2Int[0];
        currentWoundedTurns = maxWoundedTurns;
    }
    
    public void DecrementWoundedCounter()
    {
        currentWoundedTurns--;
        if (currentWoundedTurns <= 0)
        {
            moveDirections = kingMovementPattern;
        }

        Debug.Log($"Stripling Warrior has {currentWoundedTurns} turns left wounded.");
    }
}
