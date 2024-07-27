using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    Tile[,] tiles = new Tile[8, 8];

    public void Moving(List<Vector2Int> attemptedMoves)
    {
        Debug.Log(attemptedMoves[0]);
    }
}
