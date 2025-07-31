using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputScript : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI sizeInput;

    public void UpdateSlider()
    {
        var size = GetInputText(sizeInput);

        if (size > slider.maxValue)
        {
            slider.maxValue = size;
            slider.value = size;
        }
        else slider.value = size;
    }

    int GetInputText(TextMeshProUGUI input)
    {
        //Clean for spaces I think
        var cleanedText = sizeInput.text.ToString().Remove(input.text.Length - 1, 1);

        if (cleanedText.Length < 1) 
        {
            cleanedText = "1";
        }

        return int.Parse(cleanedText);
    }
}
