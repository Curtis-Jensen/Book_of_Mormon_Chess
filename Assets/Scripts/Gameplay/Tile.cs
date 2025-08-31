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
    public List<Vector2Int> targetedBy = new();

    [HideInInspector]
    public SpriteRenderer highlight;

    void Start()
    {
        highlight = gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
    }

    void OnMouseDown()
    {
        TurnManager.Instance.OnTileClicked(this); // Notify the Board when a tile is clicked
    }

    public void Highlight(bool highlighted)
    {
        highlight.enabled = highlighted;
        highlight.color = hilightColor;
    }

    void OnMouseEnter()
    {
        foreach (var tile in targetedBy)
        {
            TurnManager.Instance.tiles[tile.x, tile.y].Highlight(true);
        }
    }

    void OnMouseExit()
    {
        //Highlight(false);
    }
}

