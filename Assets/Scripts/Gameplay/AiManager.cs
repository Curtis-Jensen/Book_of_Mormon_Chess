using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    [Tooltip("How many times it will check a random piece to see if it's valid")]
    public int maxCycles = 100;
    public List<Piece> aiPieces;

    public void ChoosePiece()
    {
        for (int i = 0; i < maxCycles; i++)
        {
            var selectedPiece = aiPieces[Random.Range(0, aiPieces.Count)];
            if (selectedPiece == null)
            {
                //TODO Remove the piece if the performance starts chugging with AI
                continue;
            }

            var validMoveFound = selectedPiece.GetMovesNew(0);

            if (validMoveFound)
            {
                break;
            }
        }
    }
}
