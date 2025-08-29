using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSetup : MonoBehaviour
{
    public float cameraPadding;
    public Vector3 backGroundOffset;
    public GameObject background;

    void Start()
    {
        var boardWidth = PlayerPrefs.GetInt("boardSize");
        var cam = GetComponent<Camera>();

        cam.orthographicSize = boardWidth / 2 + cameraPadding;

        CenterBoard(boardWidth);
    }

    void CenterBoard(int boardWidth)
    {
        float centerLength = boardWidth / 2;

        bool evenBoard = boardWidth % 2 == 0;
        if (evenBoard)
        {
            centerLength -= 0.5f;
        }
        var centeredPosition = new Vector3(centerLength, centerLength, -10);
        transform.position = centeredPosition;

        background.transform.position = centeredPosition + (backGroundOffset * boardWidth);
        background.transform.localScale = new Vector3(
            background.transform.localScale.x * boardWidth,  // Width  (x-axis)
            background.transform.localScale.y * boardWidth,  // Height (y-axis)
            background.transform.localScale.z);
    }
}
