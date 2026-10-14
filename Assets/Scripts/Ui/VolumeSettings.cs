using System;
using UnityEngine;

public enum VolumeChannel { Master, Music, SoundEffects }

// Player-chosen volumes, saved in PlayerPrefs. Master goes through AudioListener so it
// covers every AudioSource; Music and SoundEffects are multipliers that whatever plays
// that kind of sound applies on top of its own configured volume.
public static class VolumeSettings
{
    public static event Action Changed;

    static string Key(VolumeChannel channel) => channel + "Volume";

    public static bool IsSaved(VolumeChannel channel) => PlayerPrefs.HasKey(Key(channel));

    public static float Get(VolumeChannel channel, float defaultValue = 1f)
    {
        return PlayerPrefs.GetFloat(Key(channel), defaultValue);
    }

    public static void Set(VolumeChannel channel, float value)
    {
        PlayerPrefs.SetFloat(Key(channel), value);
        ApplyMaster();
        Changed?.Invoke();
    }

    // Runs before the first scene loads, so the saved master volume applies in every
    // scene, not only the one that holds the sliders.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyMaster()
    {
        AudioListener.volume = Get(VolumeChannel.Master);
    }
}
