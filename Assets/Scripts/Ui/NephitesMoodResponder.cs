using UnityEngine;

/// <summary>
/// 🌑 Responds to Nephite troop loss by making the world darker and the music slower and quieter.
///
/// Attach this MonoBehaviour to any persistent GameObject in the Hoard scene.
/// It listens to <see cref="NephitesMoodTracker.OnTroopsPercentageChanged"/> and
/// smoothly drives three parameters toward their "despair" values as the army shrinks:
///
///   • Music pitch  – slows down, making the track feel heavier
///   • Music volume – fades out, like hope draining away
///   • World brightness – dims the camera background and scene ambient light
///
/// All transitions are smooth (interpolated every Update frame) so changes feel
/// gradual rather than sudden.
/// </summary>
public class NephitesMoodResponder : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────
    //  Inspector fields
    // ──────────────────────────────────────────────────────────────

    [Header("Music – Pitch (Speed)")]
    [Tooltip("Pitch when 100% of troops are alive.  1.0 = normal playback speed.")]
    [Range(0.5f, 1f)]
    public float maxPitch = 1f;

    [Tooltip("Pitch when ALL troops are gone.  Values below 1 slow the music down.")]
    [Range(0.1f, 1f)]
    public float minPitch = 0.6f;

    [Header("Music – Volume")]
    [Tooltip("Volume when 100% of troops are alive.  1.0 = full volume.")]
    [Range(0f, 1f)]
    public float maxVolume = 1f;

    [Tooltip("Volume when ALL troops are gone.")]
    [Range(0f, 1f)]
    public float minVolume = 0.3f;

    [Header("Music – Transition Speed")]
    [Tooltip("How fast pitch and volume interpolate toward their targets (units per second).")]
    public float musicTransitionSpeed = 0.5f;

    [Header("World Darkening")]
    [Tooltip("Brightness multiplier applied to the camera background color when all troops are gone.  " +
             "0 = pitch black, 1 = no change.")]
    [Range(0f, 1f)]
    public float minBrightness = 0.15f;

    [Tooltip("Brightness multiplier when all troops are alive.")]
    [Range(0f, 1f)]
    public float maxBrightness = 1f;

    [Tooltip("How fast the world darkens / lightens (lerp speed).")]
    public float darkeningTransitionSpeed = 1f;

    // ──────────────────────────────────────────────────────────────
    //  Private state
    // ──────────────────────────────────────────────────────────────
    AudioSource musicSource;

    Color originalBackgroundColor;
    Color originalAmbientLight;

    // Targets set whenever OnTroopsPercentageChanged fires
    float targetPitch;
    float targetVolume;
    Color targetBackgroundColor;
    Color targetAmbientColor;

    bool initialized;

    // ──────────────────────────────────────────────────────────────
    //  Unity lifecycle
    // ──────────────────────────────────────────────────────────────
    void Start()
    {
        // 🎵 Find the music AudioSource via MusicManager
        var musicManager = FindAnyObjectByType<MusicManager>();
        if (musicManager != null)
        {
            musicSource = musicManager.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogWarning("🌑 NephitesMoodResponder: No MusicManager found – music mood will not change");
        }

        // 🌅 Record original colors so we can scale them down
        originalBackgroundColor = Camera.main != null ? Camera.main.backgroundColor : Color.black;
        originalAmbientLight = RenderSettings.ambientLight;

        // Start at full brightness / full music
        targetPitch = maxPitch;
        targetVolume = maxVolume;
        targetBackgroundColor = originalBackgroundColor;
        targetAmbientColor = originalAmbientLight;

        initialized = true;

        // 🔌 Subscribe to mood events
        NephitesMoodTracker.OnTroopsPercentageChanged += HandleTroopsPercentageChanged;
    }

    void OnDestroy()
    {
        NephitesMoodTracker.OnTroopsPercentageChanged -= HandleTroopsPercentageChanged;
    }

    // ──────────────────────────────────────────────────────────────
    //  Event handler – recalculates targets
    // ──────────────────────────────────────────────────────────────

    /// <param name="troopsRemainingPercent">1.0 = all alive, 0.0 = all dead</param>
    void HandleTroopsPercentageChanged(float troopsRemainingPercent)
    {
        // 🎵 Music targets
        targetPitch  = Mathf.Lerp(minPitch,  maxPitch,  troopsRemainingPercent);
        targetVolume = Mathf.Lerp(minVolume, maxVolume, troopsRemainingPercent);

        // 🌑 World brightness target
        float brightness = Mathf.Lerp(minBrightness, maxBrightness, troopsRemainingPercent);
        targetBackgroundColor = originalBackgroundColor * brightness;
        targetAmbientColor    = originalAmbientLight    * brightness;
    }

    // ──────────────────────────────────────────────────────────────
    //  Update – smooth interpolation toward targets every frame
    // ──────────────────────────────────────────────────────────────
    void Update()
    {
        if (!initialized) return;

        // 🎵 Smoothly adjust pitch and volume
        if (musicSource != null)
        {
            musicSource.pitch  = Mathf.MoveTowards(musicSource.pitch,  targetPitch,  musicTransitionSpeed * Time.deltaTime);
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, targetVolume, musicTransitionSpeed * Time.deltaTime);
        }

        // 🌑 Smoothly darken / brighten the camera background
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = Color.Lerp(
                Camera.main.backgroundColor,
                targetBackgroundColor,
                darkeningTransitionSpeed * Time.deltaTime
            );
        }

        // 🌑 Smoothly darken / brighten the scene ambient light
        RenderSettings.ambientLight = Color.Lerp(
            RenderSettings.ambientLight,
            targetAmbientColor,
            darkeningTransitionSpeed * Time.deltaTime
        );
    }
}
