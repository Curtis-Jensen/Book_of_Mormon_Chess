using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool selected;
    public Piece piece;
    public Color hilightColor;
    public Color enemyHilightColor;

    [HideInInspector]
    public SpriteRenderer highlight;

    void Start()
    {
        highlight = gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
    }

    public void Highlight(bool highlighted)
    {
        highlight.enabled = highlighted;
        highlight.color = hilightColor;
    }

    void OnMouseDown()
    {
        TileHoler.Instance.OnTileClicked(this); // Notify the Board when a tile is clicked
    }
}

