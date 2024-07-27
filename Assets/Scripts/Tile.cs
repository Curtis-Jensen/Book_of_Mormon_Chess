using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    void OnMouseDown()
    {
        // Check if the tile has a piece
        if (transform.childCount <= 0) return;

        // Get the child object (assuming there is only one child)
        Transform child = transform.GetChild(0);
        Piece piece = child.GetComponent<Piece>();

        if (piece != null)
        {
            // Access the move data or any other data from the piece script
            Debug.Log("Tile clicked. Piece is white: " + piece.isWhite);
        }
    }
}

