using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [Header("References")]
    public GameObject controller;
    public GameObject movePlate;

    [Header("Positions")]
    int xBoard = -1;
    int yBoard = -1;

    [Tooltip("Variable to keep track of \"black\" player or \"white\" player.")]
    bool player;

    void Start()
    {
        
    }
}
