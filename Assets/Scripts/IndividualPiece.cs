using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndividualPiece : MonoBehaviour
{
    [Header("References")]
    public GameObject controller;
    public GameObject movePlate;

    [Header("Positions")]
    int xBoard = -1;
    int yBoard = -1;

    [Tooltip("Variable to keep track of \"black\" player or \"white\" player.")]
    string player;

    [Tooltip("References for all the sprites that the black chess pieces can be.")]
    public GameObject blackQueen, blackKnight, blackBishop, blackKing, blackRook, blackPawn;
    [Tooltip("References for all the sprites that the white chess pieces can be.")]
    public GameObject whiteQueen, whiteKnight, whiteBishop, whiteKing, whiteRook, whitePawn;

    void Start()
    {
        
    }
}
