using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EndingManager : HoardEndingManager
{
    public override void CheckEnd()
    {
        Debug.Log($"Called the appropriate CheckEnd method in {this.GetType()}");
        for (int i = 0; i < pieceSpawner.players.Length; i++)
        {
            CheckStalemate(i);
            CheckExtinction(i);
        }
    }
}
