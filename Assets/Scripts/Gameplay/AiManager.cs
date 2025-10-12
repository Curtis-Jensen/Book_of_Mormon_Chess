using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AiChoice
{
    public Piece chosenPiece;
    public Vector2 moveTo;
}

public class AiManager : MonoBehaviour
{
    [Tooltip("How many times it will check a random piece to see if it's valid")]
    public int maxCycles = 100;

    Player[] players;

    void Awake()
    {
        players = FindObjectOfType<PieceSpawner>().players;
    }

    public AiChoice ChooseMove(int playerIndex)
    {
        var killingMove = ChooseKillingMove(playerIndex);

        if (killingMove == null)
        {
            return ChooseRandomMove(playerIndex);
        }
        else
        {
            return killingMove;
        }
    }

    public AiChoice ChooseKillingMove(int playerIndex)
    {
        var killingMoves = new List<AiChoice>();

        foreach (var piece in players[playerIndex].pieces)
        {
            var moves = piece.GetMoves();
            foreach (var move in moves)
            {
                if (TurnManager.Instance.IsEnemyPiece(new Vector2Int((int)move.x, (int)move.y), piece.teamOne))
                {
                    killingMoves.Add(new AiChoice
                    {
                        chosenPiece = piece,
                        moveTo = move
                    });
                }
            }
        }

        if (killingMoves.Count > 0)
        {
            return killingMoves[Random.Range(0, killingMoves.Count)];
        }
        return null;
    }

    public AiChoice ChooseRandomMove(int playerIndex)
    {
        AiChoice aiChoice = new();

        if (players[playerIndex].pieces.Count == 0) return null;

        //Checks through each piece to see if one has a valid move
        for (int i = 0; i < maxCycles; i++)
        {
            var numberOfPieces = players[playerIndex].pieces.Count;
            //Picks a random piece
            aiChoice.chosenPiece = players[playerIndex].pieces[Random.Range(0, numberOfPieces)];
            //If it selects a piece that does not exist; try again.
            if (aiChoice.chosenPiece == null) continue;

            //Try to get the moves for the piece selected
            var validMoves = aiChoice.chosenPiece.GetMoves();

            //If a valid move has been found, stop searching! :D
            if (validMoves.Count > 0) break;
        }

        var possibleMoves = aiChoice.chosenPiece.GetMoves();

        if (possibleMoves.Count == 0) return null;

        aiChoice.moveTo = possibleMoves[Random.Range(0, possibleMoves.Count - 1)];

        return aiChoice;
    }
}

