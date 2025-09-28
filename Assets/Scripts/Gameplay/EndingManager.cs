using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingManager : HoardEndingManager
{
    public void CheckEnd()
    {
        foreach (var player in pieceSpawner.players)
        {
            CheckStalemate(player.playerIndex);
            CheckExtinction(player.playerIndex);
        }
    }
}
