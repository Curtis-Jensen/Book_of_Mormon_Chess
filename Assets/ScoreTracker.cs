using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    [SerializeField] string highScoreMessage;
    [SerializeField] TMP_Text highScorePrefix;
    [SerializeField] TMP_Text highScoreNumber;

    void Start()
    {
        var hoardTurnManager = FindAnyObjectByType<HoardTurnManager>();
        int currentScore = hoardTurnManager.WaveNumber;

        SetScore(currentScore);
        SetHighscore(currentScore);
    }

    void SetScore(int currentScore)
    {
        var scoreText = GetComponent<TMP_Text>();
        scoreText.text = currentScore.ToString();
    }

    void SetHighscore(int currentScore)
    {
        var boardWidth = PlayerPrefs.GetInt("boardSize");
        string highScoreKey = $"{boardWidth}x{boardWidth}highScore";
        
        int highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        if (currentScore > highScore)
        {
            highScorePrefix.text = highScoreMessage;

            PlayerPrefs.SetInt(highScoreKey, currentScore);
        }

        highScoreNumber.text = PlayerPrefs.GetInt(highScoreKey).ToString();
    }
}
