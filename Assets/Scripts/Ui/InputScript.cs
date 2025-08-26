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
        var inputValue = GetInputText(sizeInput);
        slider.value = inputValue;
    }

    int GetInputText(TextMeshProUGUI input)
    {
        //Clean for spaces I think
        var cleanedText = sizeInput.text.ToString().Remove(input.text.Length - 1, 1);

        var inputValue = int.Parse(cleanedText);

        if(inputValue < 1)
        {
            inputValue = 1;
        }
        else if (inputValue > slider.maxValue)
        {
            slider.maxValue = inputValue;
            slider.value    = inputValue;
        }
        else if (inputValue < slider.minValue)
        {
            slider.minValue = inputValue;
            slider.value    = inputValue;
        }

        return inputValue;
    }
}
