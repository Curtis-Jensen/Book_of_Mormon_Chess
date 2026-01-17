using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 🚨TECH DEBT TODO🚨: Rename EndingManager to ClassicEndingManager and HoardEndingManager to EndingManager!🚨
public class EndingManager : HoardEndingManager
{
    [SerializeField] PieceSets pieceSets;

    public override void CheckEnd()
    {
        for (int i = 0; i < pieceSpawner.players.Length; i++)
        {
            if (CheckNoMoves(i))
            {
                EndGame(i);
            }
        }
    }

    public override void EndGame(int playerIndex)
    {
        base.EndGame(playerIndex);

        int winningPlayerIndex;
        if (playerIndex == 0)
        {
            winningPlayerIndex = 1;
        }
        else
        {
            winningPlayerIndex = 0;
        }

        SetFlagColor(winningPlayerIndex);
        CreateParty(winningPlayerIndex);
    }

    public override void UpdateMaterial(int playerIndex, int materialValue)
    {
        base.UpdateMaterial(playerIndex, materialValue);
        
                //Color changes based on who is winning
        if(teamCounts[0] == teamCounts[1])
        {
            materialCountText.color = tieColor;
        }
        else if(teamCounts[0] > teamCounts[1])
        {
            materialCountText.color = nephiteWinColor;
        }
        else
        {
            materialCountText.color = lamaniteWinColor;
        }

        //Update sliders
        UpdateSliders(0);
        UpdateSliders(1);

        //Update text
        materialCountText.text = (teamCounts[0] - teamCounts[1]).ToString();

        UpdateSliders(playerIndex);
    }

    void UpdateSliders(int playerIndex)
    {
        if (teamCounts[playerIndex] > materialSliders[playerIndex].maxValue)
        {
            materialSliders[playerIndex].maxValue = teamCounts[playerIndex];
        }

        materialSliders[playerIndex].value = teamCounts[playerIndex];
    }

    void SetFlagColor(int winningPlayerIndex)
    {
        var winningPlayerName = pieceSpawner.players[winningPlayerIndex].name;
        var colorSelection = PlayerPrefs.GetInt(winningPlayerName + "color");//🎨

        winScreen.GetComponent<Image>().color = pieceSets.colorSets[colorSelection].baseColor;
    }

    void CreateParty(int winningPlayerIndex)
    {
        foreach (Piece piece in pieceSpawner.players[winningPlayerIndex].pieces)
        {
            piece.GetComponent<Piece>().Dance();
        }
    }
}
