using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    public TMP_InputField input;
    public Slider slider;

    private void Start()
    {
        // Restore the saved size instead of randomizing, so the slider respects the last choice
        slider.value = PlayerPrefs.GetInt("boardSize", (int)slider.value);
        UpdateInput();
    }

    public void UpdateInput()
    {
        input.text = slider.value.ToString();
        PlayerPrefs.SetInt("boardSize", (int)slider.value);

    }
}
