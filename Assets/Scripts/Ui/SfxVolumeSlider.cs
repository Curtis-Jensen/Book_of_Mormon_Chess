using UnityEngine;
using UnityEngine.UI;

// Put on the Settings sound-effects Slider: persists its value. ButtonClickSounds reads it on every click.
[RequireComponent(typeof(Slider))]
public class SfxVolumeSlider : MonoBehaviour
{
    void Awake()
    {
        var slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(ButtonClickSounds.VolumeKey, ButtonClickSounds.DefaultVolume));
        slider.onValueChanged.AddListener(value =>
        {
            PlayerPrefs.SetFloat(ButtonClickSounds.VolumeKey, value);
            PlayerPrefs.Save();
        });
    }
}
