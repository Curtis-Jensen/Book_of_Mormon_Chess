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
        int lanesPerSide = pieceSpawner.players.Length - 1;
        int laneWidth = boardSize / lanesPerSide; 
        int laneIndex = playerIndex / 2; 

        startX = laneIndex * laneWidth; 

        endX = Mathf.Min(startX + laneWidth, boardSize); 
    } 
}
