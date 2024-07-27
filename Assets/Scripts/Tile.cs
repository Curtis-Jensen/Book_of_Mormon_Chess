using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool selected;

    Board board;
    Piece piece;

    void Start()
    {
        FindBoardAndPiece();
    }

    void FindBoardAndPiece()
    {
        // Check if the tile has a piece
        if (transform.childCount <= 0) return;

        // Get the child object
        Transform child = transform.GetChild(0);
        piece = child.GetComponent<Piece>();
        board = gameObject.GetComponentInParent<Board>();
    }

    void OnMouseDown()
    {
        if (selected)
        {
            board.MovePiece(transform.position, piece);
        }
        else
        {
            PreviewPieceMoves();
        }
    }

    void PreviewPieceMoves()
    {
        // Access the move data or any other data from the piece script
        board.HilightPossibleTiles(piece.GetMoves());
    }
}

