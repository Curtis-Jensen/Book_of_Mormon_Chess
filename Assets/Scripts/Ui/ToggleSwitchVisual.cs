using UnityEngine;
using UnityEngine.UI;

// Makes a Toggle look like a switch: the knob slides to the right and brightens when on.
public class ToggleSwitchVisual : MonoBehaviour
{
    public Toggle toggle;
    public RectTransform knob;
    public Graphic knobGraphic;
    public float knobInset = 2f;
    public Color onColor = Color.white;
    public Color offColor = new Color(0.5f, 0.42f, 0.25f);

    void Awake()
    {
        toggle.onValueChanged.AddListener(Apply);
    }

    void OnEnable()
    {
        Apply(toggle.isOn);
    }

    public void Apply(bool isOn)
    {
        float side = isOn ? 1f : 0f;
        knob.anchorMin = new Vector2(side, 0.5f);
        knob.anchorMax = new Vector2(side, 0.5f);
        knob.pivot = new Vector2(side, 0.5f);
        knob.anchoredPosition = new Vector2(isOn ? -knobInset : knobInset, 0f);
        knobGraphic.color = isOn ? onColor : offColor;
    }
}
