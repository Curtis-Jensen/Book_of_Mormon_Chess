using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTester : MonoBehaviour
{
    public PieceSpawner pieceSpawner;
    public GameObject piece;

    //void Awake()
    //{
    //    TestSpawn();
    //    Debug.Log("Start worked!");
    //}

    //void Start()
    //{
    //    TestSpawn();
    //    Debug.Log("Awake worked!");
    //}

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
