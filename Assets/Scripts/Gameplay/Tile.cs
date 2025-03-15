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
        InitializePiece();
    }

    void InitializePiece()
    {
        piece = gameObject.GetComponentInChildren<Piece>();
        
        highlight = gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
    }

    public void Highlight(bool highlighted)
    {
        highlight.enabled = highlighted;
    }

    void OnMouseDown()
    {
        Board.Instance.OnTileClicked(this); // Notify the Board when a tile is clicked
    }
}

