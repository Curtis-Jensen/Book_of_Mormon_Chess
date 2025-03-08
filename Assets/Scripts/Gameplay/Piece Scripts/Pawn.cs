using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Pawn : Piece
{
    public GameObject queenPrefab;
    Board board;

    void Start()
    {
        board = GameObject.Find("Gameplay Board").GetComponent<Board>();
    }

    void Update()
    {
        //If the end has been reached
        if (transform.position.y == 0 || transform.position.y == board.boardSize - 1)
            QueenPromotion();
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

    //Changing things here?  Check BoardSetup.SpawnPiece() too.
    public void QueenPromotion()
    {
        GameObject queen = Instantiate(queenPrefab, transform.parent);
        queen.GetComponent<SpriteRenderer>().color = gameObject.GetComponent<SpriteRenderer>().color;
        queen.GetComponent<Queen>().isLight = isLight;

        //pieces.Add(pieceInstance);
        //var pieceComponent = pieceInstance.GetComponent<Piece>();

        //if (pieceComponent is King)
        //{
        //    pieceInstance.GetComponent<SpriteRenderer>().color = player.kingColor;
        //}
        //else
        //{
        //    pieceInstance.GetComponent<SpriteRenderer>().color = player.color;
        //}

        //pieceInstance.name = $"{pieceInstance.name} {player.name} {x + 1}";

        //pieceComponent.isLight = player.teamOne;
        //pieceComponent.playerIndex = playerIndex;

        //if (player.isAi)
        //{
        //    aiManager.aiPieces.Add(pieceComponent);
        //}
        Destroy(gameObject);
    }
}
