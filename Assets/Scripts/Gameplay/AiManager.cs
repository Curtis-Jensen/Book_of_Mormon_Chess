using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Formatters;
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

    private King FindKingInCheck(int playerIndex)
    {
        // Find the king among the player's pieces
        King king = null;
        foreach (var piece in players[playerIndex].pieces)
        {
            if (piece is King)
            {
                king = (King)piece;
                break;
            }
        }

        if (king == null || !king.inCheck) return null;
        else
        return king;
    }

    private AiChoice GetKingEscapeMove(int playerIndex)
    {
        // Find the king
        King king = null;
        foreach (var piece in players[playerIndex].pieces)
        {
            if (piece is King)
            {
                king = (King)piece;
                break;
            }
        }

        if (king == null) return null;

        // Get all possible moves for the king
        var moves = king.GetMoves();
        if (moves.Count == 0) return null;

        // Choose a random move for the king
        var randomMove = moves[Random.Range(0, moves.Count)];
        return new AiChoice
        {
            chosenPiece = king,
            moveTo = randomMove
        };
    }

    public AiChoice ChooseMove(int playerIndex)
    {

        // First priority: If king is in check, move it
        if (FindKingInCheck(playerIndex))
        {
            var kingEscapeMove = GetKingEscapeMove(playerIndex);
            if (kingEscapeMove != null)
            {
                return kingEscapeMove;
            }
        }

        // Second priority: Look for killing moves
        var killingMove = ChooseKillingMove(playerIndex);
        if (killingMove != null)
        {
            return killingMove;
        }

        // Last resort: Make a random move
        return ChooseRandomMove(playerIndex);
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

