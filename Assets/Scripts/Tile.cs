using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool selected;
    public Piece piece;

    Board board;

    void Start()
    {
        FindBoardAndPiece();
    }

    void FindBoardAndPiece()
    {
        board = transform.parent.GetComponentInParent<Board>();

        // Check if the tile has a piece
        if (transform.childCount <= 0) return;

        // Get the child object
        Transform child = transform.GetChild(0);
        piece = child.GetComponent<Piece>();
    }

    void OnMouseDown()
    {
        if (selected)
        {
            board.MovePiece(transform.position);
        }
        else
        {
            PreviewPieceMoves();
        }
    }

    void PreviewPieceMoves()
    {
        //If no piece is on the tile, do not attempt this logic
        if (piece == null) return;

        // Access the move data or any other data from the piece script
        board.HilightPossibleTiles(piece.GetMoves(), piece);
    }
}

