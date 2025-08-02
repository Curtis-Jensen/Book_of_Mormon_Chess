using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTester : MonoBehaviour
{
    public GameObject piece;
    PieceSpawner pieceSpawner;

    //void Awake()
    //{
    //    TestSpawn();
    //    Debug.Log("Start worked!");
    //}

    void Start()
    {
        pieceSpawner = GetComponent<PieceSpawner>();
        //TestSpawn();
        //Debug.Log("Awake worked!");
    }

    public void TestSpawn()
    {
        Debug.Log("Testing piece spawn...");

        pieceSpawner.SpawnPiece(
            piece, 
            new Vector2(0, 0), 
            0, 
            false
        );
    }
}
