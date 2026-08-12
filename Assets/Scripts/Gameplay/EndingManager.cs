using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : HoardEndingManager
{
    [SerializeField] PieceSets pieceSets;
    [SerializeField] Slider[] materialSliders = new Slider[2];
    [SerializeField] TextMeshProUGUI materialCountText;
    [SerializeField] Color tieColor;
    [SerializeField] Color nephiteWinColor;
    [SerializeField] Color lamaniteWinColor;

    int[] factionCounts;

    void Awake()
    {
        pieceSpawner = FindAnyObjectByType<PieceSpawner>();

        factionCounts = new int[2];
        for (int i = 0; i < factionCounts.Length; i++)
        {
            factionCounts[i] = 0;
        }
    }

    public void UpdateMaterial(int playerIndex, int materialValue)
    {
        var factionIndex = playerIndex % 2;
        factionCounts[factionIndex] += materialValue;

        //Color changes based on who is winning
        if(factionCounts[0] == factionCounts[1])
        {
            materialCountText.color = tieColor;
        }
        else if(factionCounts[0] > factionCounts[1])
        {
            materialCountText.color = nephiteWinColor;
        }
        else
        {
            materialCountText.color = lamaniteWinColor;
        }

        //Update sliders
        UpdateSliders(factionIndex);

        //Update text
        materialCountText.text = (factionCounts[0] - factionCounts[1]).ToString();
    }

        void UpdateSliders(int factionIndex)
    {
        if (factionCounts[factionIndex] > materialSliders[factionIndex].maxValue)
        {
            materialSliders[factionIndex].maxValue = factionCounts[factionIndex];
        }

        materialSliders[factionIndex].value = factionCounts[factionIndex];
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
        var colorSelection = PlayerPrefs.GetInt(winningPlayerName + "color", pieceSpawner.DefaultColorIndex(winningPlayerName));//🎨

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
