using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SizeInputScript : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI sizeInput;

    public void UpdateSlider()
    {
        if (!TryGetInputValue(out int inputValue)) return;

        slider.value = inputValue;
    }

    bool TryGetInputValue(out int inputValue)
    {
        // TMP input text carries a trailing zero-width space; strip anything non-numeric
        var cleanedText = new string(System.Array.FindAll(sizeInput.text.ToCharArray(), char.IsDigit));
        if (!int.TryParse(cleanedText, out inputValue)) return false;

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

        return true;
    }
}
