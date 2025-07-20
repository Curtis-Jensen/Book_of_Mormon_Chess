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

    public Piece ChoosePiece()
    {
        Piece selectedPiece = null;

        //Checks through each piece to see if one has a valid move
        for (int i = 0; i < maxCycles; i++)
        {
            //Picks a random piece
            selectedPiece = aiPieces[Random.Range(0, aiPieces.Count - 1)];
            //If it selects a piece that does not exist; try again.
            if (selectedPiece == null)
            {
                //TODO Remove the piece if the performance starts chugging with AI
                continue;
            }

            //Try to get the moves for the piece selected
            var validMoves = selectedPiece.GetMoves();

            //If a valid move has been found, stop searching! :D
            if (validMoves.Count > 0)
            {
                break;
            }
        }

        return selectedPiece;
    }
}
