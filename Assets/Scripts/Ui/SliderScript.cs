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
        slider.value = Random.Range(slider.minValue, slider.maxValue);
        UpdateInput();
    }

    public void UpdateInput()
    {
        input.text = slider.value.ToString();
    }
}
