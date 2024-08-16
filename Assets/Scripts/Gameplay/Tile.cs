using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool selected;
    public Piece piece;
    public bool AiOpponent = false;

    [HideInInspector]
    public GameObject highlight;

    void Start()
    {
        FindPiece();
    }

    void FindPiece()
    {
        piece = gameObject.GetComponentInChildren<Piece>();
    }

    void OnMouseDown()
    {
        if (selected)
        {
            Board.Instance.MovePiece(transform.position);
            if (AiOpponent)
            {
                PreviewPieceMoves();
            }
        }
        else
        {
            PreviewPieceMoves();
        }
    }

    void PreviewPieceMoves()
    {
        //If no piece is on the tile
        if (piece == null) return;
        //If no piece is on the tile, or it's not that piece's turn
        if (Board.Instance.whiteTurn != piece.isWhite) return;

        // Access the move data or any other data from the piece script
        Board.Instance.HilightPossibleTiles(piece.GetMoves(), piece);
    }
}

