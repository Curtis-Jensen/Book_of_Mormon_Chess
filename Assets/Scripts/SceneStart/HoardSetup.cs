using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class HoardSetup : ResizableSetup
{
    List<GameObject> tiles =  new();

    protected override void OrderPieces(int[] pieceChoices)
    {
        OrderBackRows(pieceChoices, 0, 0);

        if (boardSize > 3)
        {
            OrderPawns(0, 1);
        }
    }
}
