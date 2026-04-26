using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class HoardBoardSetup : BoardSetup
{
    protected override void OrderPieces(int[] pieceChoices)
    {
        for(int i = 0; i < pieceSpawner.players.Length; i++)
        {
            if (i == 1) continue; // We skip player 2 because that's reserved for the endless Lamanites

            OrderBackRows(pieceChoices, i);

            if (boardSize > 3)
            {
                OrderPawns(i);
            }
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
