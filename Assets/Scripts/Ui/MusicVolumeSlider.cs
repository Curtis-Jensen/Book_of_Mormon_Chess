using UnityEngine;
using UnityEngine.UI;

// Put on the Settings music Slider: persists its value and applies it to the MusicPlayer live.
[RequireComponent(typeof(Slider))]
public class MusicVolumeSlider : MonoBehaviour
{
    Slider slider;
    MusicPlayer player;

    void Awake()
    {
        slider = GetComponent<Slider>();
        player = FindFirstObjectByType<MusicPlayer>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicPlayer.VolumeKey, MusicPlayer.DefaultVolume));
        slider.onValueChanged.AddListener(OnChanged);
    }

    void OnChanged(float value)
    {
        PlayerPrefs.SetFloat(MusicPlayer.VolumeKey, value);
        PlayerPrefs.Save();
        if (player != null) player.SetVolume(value);
    }
}
