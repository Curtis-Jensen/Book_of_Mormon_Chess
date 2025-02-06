using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    public TMP_InputField input;
    public 
    public Slider slider;

    public SliderScript()
    {
        input.text = slider.value.ToString();
    }
}
