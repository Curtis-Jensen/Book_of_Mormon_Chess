using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Pawn : Piece
{
    public Queen queen;

    Board board;

    void Start()
    {
        board = GameObject.Find("Gameplay Board").GetComponent<Board>();
    }

    void Update()
    {
        Debug.Log(transform.position.y);
        //If the end has been reached
        if (transform.position.y == 0 || transform.position.y == board.boardSize  - 1)
            QueenUpgrade();
    }

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        //If it's light, go up, if it's dark, go down
        int forward = isLight ? 1 : -1;

        validMoves = GetForwardMoves (validMoves, forward);

        if(!firstTurnTaken && validMoves.Count != 0)
        {
            validMoves = GetForwardMoves(validMoves, forward * 2);
        }

        validMoves = GetDiagonalMoves(validMoves, forward);

        return validMoves;
    }

    List<Vector2Int> GetForwardMoves(List<Vector2Int> validMoves, int forward)
    {
        // Check one square forward
        Vector2Int forwardMove =
            new((int)transform.position.x, (int)transform.position.y + forward);
        if (Board.Instance.IsTileEmpty(forwardMove))
        {
            validMoves.Add(forwardMove);
        }

        return validMoves;
    }

    List<Vector2Int> GetDiagonalMoves(List<Vector2Int> validMoves, int forward)
    {
        // Check diagonal capture moves
        Vector2Int[] diagonalMoves = new Vector2Int[]
        {
            new ((int)transform.position.x - 1, (int)transform.position.y + forward),
            new ((int)transform.position.x + 1, (int)transform.position.y + forward)
        };

        foreach (var move in diagonalMoves)
        {
            if (Board.Instance.IsEnemyPiece(move, isLight))
            {
                validMoves.Add(move);
            }
        }

        return validMoves;
    }

    public void QueenUpgrade()
    {
        Instantiate(queen, transform.parent);
        Destroy(gameObject);
    }
}
