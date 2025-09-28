using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : HoardEndingManager
{
    [SerializeField] PieceSets pieceSets;

    public override void CheckEnd()
    {
        for (int i = 0; i < pieceSpawner.players.Length; i++)
        {
            CheckStalemate(i);
            CheckExtinction(i);
        }
    }

    protected override void EndGame(int playerIndex)
    {
        base.EndGame(playerIndex);

        SetFlagColor(playerIndex);
    }

    void SetFlagColor(int playerIndex)
    {
        int winningPlayerIndex;
        if (playerIndex == 0)
        {
            winningPlayerIndex = 1;
        }
        else
        {
            winningPlayerIndex = 0;
        }

        var winningPlayerName = pieceSpawner.players[winningPlayerIndex].name;
        var colorSelection = PlayerPrefs.GetInt(winningPlayerName + "color");//🎨

        winScreen.GetComponent<Image>().color = pieceSets.colorSets[colorSelection].baseColor;
    }
}
