using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : HoardEndingManager
{
    [SerializeField] PieceSets pieceSets;

    int[] teamCounts;

    void Awake()
    {
        pieceSpawner = FindAnyObjectByType<PieceSpawner>();

        teamCounts = new int[2];
        for (int i = 0; i < teamCounts.Length; i++)
        {
            teamCounts[i] = 0;
        }
    }

    public void UpdateMaterial(int playerIndex, int materialValue)
    {
        teamCounts[playerIndex] += materialValue;

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
    }

        void UpdateSliders(int playerIndex)
    {
        if (teamCounts[playerIndex] > materialSliders[playerIndex].maxValue)
        {
            materialSliders[playerIndex].maxValue = teamCounts[playerIndex];
        }

        materialSliders[playerIndex].value = teamCounts[playerIndex];
    }

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
