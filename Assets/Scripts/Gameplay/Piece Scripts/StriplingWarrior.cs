using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StriplingWarrior : Piece
{
    [SerializeField] Sprite activeSprite;
    [SerializeField] Sprite woundedSprite;
    SpriteRenderer spriteRenderer;


    [SerializeField] int maxWoundedTurns = 2;
    int currentWoundedTurns = 0;
    TextMeshPro woundedCounterText;
    Faction originalFaction;

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


    protected override void Start()
    {
        SetupWounding();
        base.Start();
    }

    void SetupWounding()
    {
        moveDirections = kingMovementPattern;

        maxWoundedTurns *= 2; //Convert from player turns to full turns

        TurnProgresser.Instance.OnMoveEnd += DecrementWoundedCounter;

        woundedCounterText = GetComponentInChildren<TextMeshPro>();
        woundedCounterText.text = "";

        spriteRenderer = GetComponent<SpriteRenderer>();

        originalFaction = faction;
    }

    public override bool IsEnemyPiece(Vector2Int position)
    {
        // Check if position is valid
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSize) return false;

        TileSelector tile = TurnProgresser.Instance.tiles[position.x, position.y];
        
        // If there's no piece, it's not an enemy
        if (tile.piece == null) return false;
        
        // If it's another Stripling Warrior, they won't fight each other
        if (tile.piece.GetType() == typeof(StriplingWarrior)) return false;
        
        return base.IsEnemyPiece(position);
    }

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        foreach (var moveDirection in moveDirections)
        {
            Vector2Int newMove =
                new(Mathf.RoundToInt(transform.position.x) + moveDirection.x, Mathf.RoundToInt(transform.position.y) + moveDirection.y);

            bool emptyOrEnemy =
                IsTileEmpty(newMove) || IsEnemyPiece(newMove);

            if (emptyOrEnemy)
            {
                validMoves.Add(newMove);
            }
        }

        return validMoves;
    }

    /// <summary>
    /// Stripling Warriors don't die, they just get wounded.
    /// </summary>
    public override void Die(bool instantiateEffects = true)
    {
        if (TurnProgresser.Instance.selectedPiece.GetType() == typeof(StriplingWarrior))
        {
            base.Die();
            return;
        }
        TurnProgresser.Instance.selectedPiece.Die();
        TurnProgresser.Instance.selectedPiece = this;

        moveDirections = new Vector2Int[0];
        currentWoundedTurns = maxWoundedTurns;
        spriteRenderer.sprite = woundedSprite;

        faction = Faction.Inanimate;
    }

    public void DecrementWoundedCounter()
    {
        currentWoundedTurns--;
        if (currentWoundedTurns <= 0)
        {
            UnWounded();
        }
        else if (woundedCounterText != null)
        {
            woundedCounterText.text = (currentWoundedTurns / 2 + currentWoundedTurns % 2).ToString();
        }
    }

    void UnWounded()
    {
        moveDirections = kingMovementPattern;

        if (woundedCounterText != null) woundedCounterText.text = "";

        spriteRenderer.sprite = activeSprite;

        faction = originalFaction;
    }
}
