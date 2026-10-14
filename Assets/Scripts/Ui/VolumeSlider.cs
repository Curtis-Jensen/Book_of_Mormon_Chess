using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public VolumeChannel channel;
    public Slider slider;

    private void Awake()
    {
        slider.value = VolumeSettings.Get(channel, slider.value);
        slider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume(float value)
    {
        VolumeSettings.Set(channel, value);
    }

    // Called by SettingsDefaultsSeeder so this control's own configured slider.value
    // becomes the saved value even if this GameObject is still inactive.
    public void SeedIfMissing()
    {
        if (!VolumeSettings.IsSaved(channel))
        {
            VolumeSettings.Set(channel, slider.value);
        }
    }
}
