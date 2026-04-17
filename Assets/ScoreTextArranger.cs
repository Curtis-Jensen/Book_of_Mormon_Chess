using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreTextArranger : MonoBehaviour
{
    TMP_Text scoreText;
    TMP_Text highScoreText;

    void Start()
    {
        var hoardTurnManager = FindAnyObjectByType<HoardTurnManager>();
        int currentScore = hoardTurnManager.WaveNumber;

        SetScore(currentScore);
        SetHighscore(currentScore);
    }

    void SetScore(int currentScore)
    {
        scoreText = GetComponent<TMP_Text>();
        scoreText.text = currentScore.ToString();
    }

    void SetHighscore(int currentScore)
    {
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
