using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreTextArranger : MonoBehaviour
{
    [SerializeField, TextArea(3, 8)] private string displayText;

    private TMP_Text scoreText;

    void Start()
    {
        scoreText = GetComponent<TMP_Text>();

        scoreText.text = displayText;
    }
}
