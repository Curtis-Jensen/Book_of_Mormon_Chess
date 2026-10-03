using UnityEngine;
using UnityEngine.UI;

// Put on a Canvas: every Button under it (including inactive panels) plays a click.
[RequireComponent(typeof(AudioSource))]
public class ButtonClickSounds : MonoBehaviour
{
    [SerializeField] AudioClip clickClip;
    [SerializeField, Range(0f, 1f)] float volume = 0.5f;

    AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;

        foreach (var button in GetComponentsInChildren<Button>(includeInactive: true))
            button.onClick.AddListener(PlayClick);
    }

    void PlayClick()
    {
        if (clickClip != null) source.PlayOneShot(clickClip, volume);
    }
}
