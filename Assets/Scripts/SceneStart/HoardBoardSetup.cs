using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class HoardTurnManagerSetup : BoardSetup
{
    protected override void StartPieces()
    {
        var pieceChoices = RandomizePieces();
        OrderPieces(pieceChoices);
    }

    protected override void OrderPieces(int[] pieceChoices)
    {
        OrderBackRows(pieceChoices, 0, 0);

        if (boardSize > 3)
        {
            OrderPawns(0, 1);
        }
    }
}
