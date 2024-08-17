using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BoardSetup : MonoBehaviour
{
    public int boardSize = 8;
    public GameObject rowPrefab;
    public GameObject lightTilePrefab;
    public GameObject  darkTilePrefab;
    public GameObject lightPiecePrefab;
    public GameObject  darkPiecePrefab;

    bool evenBoard;
    List<GameObject> rowList =   new();
    List<GameObject> tileList =  new();
    List<GameObject> pieceList = new();

    void Awake()
    {
        evenBoard = IsBoardEven();
        SpawnRows();
        SpawnTiles();
        SpawnPieces();
        CenterCamera();
    }

    bool IsBoardEven()
    {
        return boardSize % 2 == 0;
    }

    void SpawnRows()
    {
        for (int y = 0; y < boardSize; y++) 
        {
            var newRow = Instantiate(rowPrefab, transform);

            newRow.name = "Row " + (y + 1);
            rowList.Add(newRow);
        }
    }
    
    void SpawnTiles()
    {
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                // Alternate between dark and light tiles
                GameObject prefabToInstantiate = (x + y) % 2 == 0 ? darkTilePrefab : lightTilePrefab;

                var tilePosition = new Vector3Int(x, y, 0);

                var newTile = 
                    Instantiate(prefabToInstantiate, tilePosition, Quaternion.identity, rowList[y].transform);

                newTile.name = "Tile " + (x + 1);
                tileList.Add(newTile);
            }
        }
    }
    
    void SpawnPieces()
    {
        var topRightTile = tileList.Count - 1;

        for (int x = topRightTile; x > topRightTile - boardSize; x--)
        {
            var newTile =
                    Instantiate(darkPiecePrefab, tileList[x].transform);

            newTile.name = "Piece " + (x + 1);
            pieceList.Add(newTile);
        }

        for (int x = 0; x < boardSize; x++)
        {
            var newTile =
                    Instantiate(lightPiecePrefab, tileList[x].transform);

            newTile.name = "Piece " + (x + 1);
            pieceList.Add(newTile);
        }
    }

    void CenterCamera()
    {
        var cam = FindAnyObjectByType<Camera>().gameObject;

        float camPosition = boardSize / 2;
        if (evenBoard)
        {
            camPosition -= 0.5f;
        }
        cam.transform.position = new Vector3 (camPosition, camPosition, -10);
    }
}
