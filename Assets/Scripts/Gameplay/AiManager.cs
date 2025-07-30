using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    [Tooltip("How many times it will check a random piece to see if it's valid")]
    public int maxCycles = 100;
    [HideInInspector]
    public List<Piece> aiPieces;
  
    public bool PiecesExist() 
    { 
        if(aiPieces.Count == 0) return false;
        else return true;
    }



    public AiChoice ChooseMove()
    {
        AiChoice aiChoice = new();
        aiChoice.moveTo = new Vector2(-100, 100);

        if (aiPieces.Count == 0) return aiChoice;

        //Checks through each piece to see if one has a valid move
        for (int i = 0; i < maxCycles; i++)
        {
            //Picks a random piece
            aiChoice.chosenPiece = aiPieces[Random.Range(0, aiPieces.Count - 1)];
            //If it selects a piece that does not exist; try again.
            if (aiChoice.chosenPiece == null) continue;

            //Try to get the moves for the piece selected
            var validMoves = aiChoice.chosenPiece.GetMoves();

            //If a valid move has been found, stop searching! :D
            if (validMoves.Count > 0) break;
        }

        var possibleMoves = aiChoice.chosenPiece.GetMoves();

        if(possibleMoves.Count == 0) return aiChoice;

        aiChoice.moveTo = possibleMoves[Random.Range(0, possibleMoves.Count - 1)];

        return aiChoice;
    }

    public AiChoice ChooseKillingMove()
    {

    }

    public AiChoice ChooseRandomMove()
    {

    }
}

public class AiChoice
{
    public Piece chosenPiece;
    public Vector2 moveTo;
}
