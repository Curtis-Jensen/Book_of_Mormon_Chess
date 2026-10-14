using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shows a 0-1 slider's value as a percentage next to it.
public class SliderValueLabel : MonoBehaviour
{
    public Slider slider;
    public TMP_Text label;

    void Awake()
    {
        slider.onValueChanged.AddListener(Show);
    }

    void OnEnable()
    {
        Show(slider.value);
    }

    public void Show(float value)
    {
        label.text = $"{Mathf.RoundToInt(value * 100f)}%";
    }
}
