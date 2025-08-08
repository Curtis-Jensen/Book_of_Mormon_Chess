using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Pawn : Piece
{
    public GameObject queenPrefab;
    public Sprite queenSprite;
    TileHoler board;

    void Start()
    {
        board = GameObject.Find("Gameplay Board").GetComponent<TileHoler>();
    }

    void Update()
    {
        var teamOneEnd = transform.position.y == board.boardSize - 1 && teamOne;
        var teamTwoEnd = transform.position.y == 0 && !teamOne;
        //If the end has been reached
        if (teamTwoEnd || teamOneEnd)
        {
            QueenPromotion();
            Destroy(gameObject);
        }
    }

    public override List<Vector2Int> GetMoves()
    {
        List<Vector2Int> validMoves = new();

        //If it's light, go up, if it's dark, go down
        int forward = teamOne ? 1 : -1;

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
        if (TileHoler.Instance.IsTileEmpty(forwardMove))
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
            if (TileHoler.Instance.IsEnemyPiece(move, teamOne))
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
        var queenScript = queen.GetComponent<Queen>();

        queen.GetComponent<SpriteRenderer>().color = gameObject.GetComponent<SpriteRenderer>().color;
        queen.GetComponent<SpriteRenderer>().sprite = queenSprite;
        queen.transform.localScale = gameObject.transform.localScale;

        queen.name = "New Queen";

        queenScript.teamOne = teamOne;
        queenScript.playerIndex = playerIndex;

        //Ai code has started here.  Might need more information to be passed to the pawn

        //if (player.isAi)
        //{
        //    FindObjectOfType<AiManager>().aiPieces.Add(queenScript);
        //}
        transform.parent.GetComponent<Tile>().piece = queenScript;
    }
}
