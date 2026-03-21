using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreTextArranger : MonoBehaviour
{
    [SerializeField, TextArea(3, 8)] private string displayText;

    HoardTurnManager hoardTurnManager;
    TMP_Text scoreText;

    void Start()
    {
        hoardTurnManager = FindAnyObjectByType<HoardTurnManager>();
        scoreText = GetComponent<TMP_Text>();

        scoreText.text = hoardTurnManager.WaveNumber.ToString();
    }
}
