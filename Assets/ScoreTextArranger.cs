using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreTextArranger : MonoBehaviour
{
    HoardTurnManager hoardTurnManager;
    TMP_Text scoreText;
    TMP_Text highScoreText;


    void Start()
    {
        hoardTurnManager = FindAnyObjectByType<HoardTurnManager>();
        scoreText = GetComponent<TMP_Text>();

        SetScore();
    }

    void SetScore()
    {
        int currentScore = hoardTurnManager.WaveNumber;
        scoreText.text = currentScore.ToString();

        var boardWidth = PlayerPrefs.GetInt("boardSize");
        string highScoreKey = $"{boardWidth}x{boardWidth}highScore";
        
        int highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt(highScoreKey, currentScore);
        }

        highScoreText.text = PlayerPrefs.GetInt(highScoreKey).ToString();
    }
}
