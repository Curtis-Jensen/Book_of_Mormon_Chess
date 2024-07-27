using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool selected;

    Board board;

    void OnMouseDown()
    {
        // Check if the tile has a piece
        if (transform.childCount <= 0) return;

        // Get the child object
        Transform child = transform.GetChild(0);
        Piece piece = child.GetComponent<Piece>();
        board = gameObject.GetComponentInParent<Board>();

        // Access the move data or any other data from the piece script
        board.HilightPossibleTiles(piece.GetMoves());
    }
}

