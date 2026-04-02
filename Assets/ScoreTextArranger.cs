using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreTextArranger : MonoBehaviour
{
    HoardTurnManager hoardTurnManager;
    TMP_Text scoreText;

    void Start()
    {
        hoardTurnManager = FindAnyObjectByType<HoardTurnManager>();
        scoreText = GetComponent<TMP_Text>();

        SetNumber();
    }

    void SetNumber()
    {
        scoreText.text = hoardTurnManager.WaveNumber.ToString();
    }
}
