using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class HoardBoardSetup : BoardSetup
{
    protected override void OrderPieces(int[] pieceChoices)
    {
        OrderBackRows(pieceChoices, 0);

        if (boardSize > 3)
        {
            OrderPawns(0);
        }
    }

    protected override void GetPlayerLaneBounds(int playerIndex, out int startX, out int endX) 
    { 
        int lanesPerSide = Mathf.Max(1, pieceSpawner.players.Length / 2); 
        int laneWidth = Mathf.Max(1, boardSize / lanesPerSide); 
        int laneIndex = playerIndex / 2; 

        startX = laneIndex * laneWidth; 

        if (startX >= boardSize) 
        { 
            startX = boardSize; 
            endX = boardSize; 
            return; 
        } 

        endX = Mathf.Min(startX + laneWidth, boardSize); 
    } 
}
