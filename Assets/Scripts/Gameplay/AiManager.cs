using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    [Tooltip("How many times it will check a random piece to see if it's valid")]
    public int maxCycles = 100;
    public List<Piece> aiPieces;

    public static AiManager Instance { get; set; } // Static instance

    private void Start()
    {
        Instance = this;
    }

    public Vector2 ChoosePiece()
    {
        Vector2 validMove = new Vector2(99,99);

        //Checks through each piece to see if one has a valid move
        for (int i = 0; i < maxCycles; i++)
        {
            //Picks a random piece
            var selectedPiece = aiPieces[Random.Range(0, aiPieces.Count - 1)];
            //If it selects a piece that does not exist; try again.
            if (selectedPiece == null)
            {
                //TODO Remove the piece if the performance starts chugging with AI
                continue;
            }

            //Try to get the moves for the piece selected
            var allValidMoves = selectedPiece.GetMovesNew();
            validMove = allValidMoves[Random.Range(0, allValidMoves.Count)];

            //If a valid move has been found, stop searching! :D
            if (validMove != null)
            {
                break;
            }
        }

        return validMove;
    }
}
