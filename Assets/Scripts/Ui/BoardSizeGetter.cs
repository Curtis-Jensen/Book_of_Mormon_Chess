using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardSizeGetter : MonoBehaviour
{
    TMP_Text sizeText;

    void Start()
    {
        sizeText = GetComponent<TMP_Text>();

        int boardWidth = PlayerPrefs.GetInt("boardSize");

        sizeText.text = $"{boardWidth}x{boardWidth}";
    }
}
