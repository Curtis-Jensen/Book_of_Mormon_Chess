using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool selected;
    public Piece piece;

    [HideInInspector]
    public SpriteRenderer highlight;

    void Start()
    {
        FindPiece();
    }

    void FindPiece()
    {
        piece = gameObject.GetComponentInChildren<Piece>();
        
        highlight = gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
    }

    void OnMouseDown()
    {
        Board.Instance.OnTileClicked(this); // Notify the Board when a tile is clicked
    }
}

