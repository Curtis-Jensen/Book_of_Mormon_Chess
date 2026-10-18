using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -------------------------------------------------------
// LastStandDramatizer
// Makes Nephites' Last Stand grow bleaker as the Nephites (player 0) lose troops:
//   - the screen darkens in proportion to losses
//   - rain begins after a set number of deaths and intensifies
//   - lightning flashes at the halfway point, the second-to-last death and the last death
//   - the music slows (and deepens) after every death
// Everything is built at runtime (overlay sprites + rain particles) so the scene only needs
// this one component. Every number is exposed in the Inspector.
// Counts deaths via Piece.OnPieceKilled; Nephite starting strength is inferred at the first death.
// -------------------------------------------------------
public class LastStandDramatizer : MonoBehaviour
{
    // ========================
    // DARKNESS
    // ========================
    [Header("Darkness")]
    [Tooltip("Overlay color. Alpha is ignored; it's driven by the curve below")]
    public Color darknessColor = new Color(0.02f, 0.03f, 0.09f, 1f);
    [Tooltip("X = fraction of Nephites lost (0-1), Y = how dark (0 = none, 1 = pitch black) before the max cap")]
    public AnimationCurve darknessByLossFraction = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Range(0f, 1f)] [Tooltip("Hard cap on overlay opacity so the board stays readable")]
    public float maxDarkness = 0.65f;
    [Range(0f, 1f)] [Tooltip("Extra darkness added once the rain has started (storm clouds)")]
    public float extraDarknessWhenRaining = 0.08f;
    [Tooltip("Seconds for the screen to fade to a new darkness level after a death")]
    public float darknessFadeSeconds = 1.5f;

    // ========================
    // RAIN
    // ========================
    [Header("Rain")]
    [Tooltip("Rain starts once this many Nephites have died (1 = after the first death)")]
    public int rainStartsAfterDeaths = 1;
    [Tooltip("Drops per second right when the rain starts")]
    public float minRainRate = 60f;
    [Tooltip("Drops per second at the final death (rain scales up with losses)")]
    public float maxRainRate = 260f;
    [Tooltip("How fast drops fall, in world units per second")]
    public float rainFallSpeed = 14f;
    [Tooltip("Sideways drift in world units per second (negative = blowing left)")]
    public float rainWindSpeed = -2.5f;
    [Tooltip("Thickness of each streak in world units")]
    public float rainDropSize = 0.025f;
    [Tooltip("How long each streak is stretched, relative to its speed")]
    public float rainStreakLength = 0.06f;
    public Color rainColor = new Color(0f, 0f, 0f, 0.6f);
    [Tooltip("Optional looping rain sound")]
    public AudioClip rainLoopClip;
    [Range(0f, 1f)] public float rainVolume = 0.4f;
    [Tooltip("Seconds for the rain sound to fade in")]
    public float rainFadeInSeconds = 3f;

    // ========================
    // LIGHTNING
    // ========================
    [Header("Lightning")]
    [Tooltip("Flash when the Nephites have lost half their troops")]
    public bool flashAtHalfDead = true;
    [Tooltip("Flash on the second-to-last Nephite death")]
    public bool flashAtSecondToLastDeath = true;
    [Tooltip("Flash on the last Nephite death")]
    public bool flashAtLastDeath = true;
    public Color lightningColor = new Color(0.85f, 0.9f, 1f, 1f);
    [Range(0f, 1f)] [Tooltip("Screen opacity at the brightest point of a flash")]
    public float lightningPeakAlpha = 0.85f;
    [Tooltip("How many quick strobes make up one lightning strike")]
    public int strobesPerStrike = 2;
    [Tooltip("Seconds to snap up to full brightness")]
    public float flashRiseSeconds = 0.05f;
    [Tooltip("Seconds to fade back out after each strobe")]
    public float flashFadeSeconds = 0.25f;
    [Tooltip("Seconds of dark between strobes within a strike")]
    public float timeBetweenStrobes = 0.12f;
    [Tooltip("Crack-and-rumble sound played the instant lightning flashes (defaults to Thunder.wav)")]
    public AudioClip lightningClip;
    [Range(0f, 1f)] public float lightningVolume = 1f;
    [Tooltip("Optional extra rumbles played after a delay; one is picked at random per strike")]
    public AudioClip[] thunderClips;
    [Range(0f, 1f)] public float thunderVolume = 0.8f;
    [Tooltip("Seconds between the flash and the thunder (min, max)")]
    public Vector2 thunderDelayRange = new Vector2(0.3f, 1.2f);

    // ========================
    // MUSIC
    // ========================
    [Header("Music slowdown")]
    [Tooltip("Speed once only one Nephite is left. Speed slides evenly from 1 down to this, so the slowdown per death depends on army size (2 troops: one death hits it; 16 troops: 15 deaths)")]
    [Range(0.2f, 1f)] public float minMusicSpeed = 0.6f;
    [Tooltip("Seconds for the music to glide down to the new speed")]
    public float musicSlowSeconds = 1.5f;

    // ========================
    // RUNTIME STATE
    // ========================
    const int OverlaySortingOrder = 500; // Above pieces/tiles; UI canvases are unaffected

    int _deaths;
    int _startingTroops;
    bool _isRaining;
    readonly HashSet<int> _flashedAtDeath = new HashSet<int>();

    Camera _cam;
    MusicPlayer _music;
    SpriteRenderer _darkness;
    SpriteRenderer _flash;
    ParticleSystem _rain;
    AudioSource _sfxSource;
    AudioSource _rainSource;

    float _currentDarkness;
    float _darknessVelocity;
    float _targetMusicSpeed = 1f;
    float _currentMusicSpeed = 1f;
    float _musicSpeedVelocity;
    float _flashAlpha;

    // ========================
    // SETUP
    // ========================

    // -------------------------------------------------------
    // Awake()
    // Finds the camera/music and builds the overlay sprites, rain particles and audio sources.
    // -------------------------------------------------------
    void Awake()
    {
        _cam = Camera.main;
        _music = FindAnyObjectByType<MusicPlayer>();
        if (_cam == null) { enabled = false; return; }

        Sprite square = MakeWhiteSquareSprite();
        _darkness = MakeOverlay("Darkness Overlay", square, OverlaySortingOrder);
        _flash = MakeOverlay("Lightning Overlay", square, OverlaySortingOrder + 2);
        _rain = MakeRain();

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _rainSource = gameObject.AddComponent<AudioSource>();
        _rainSource.playOnAwake = false;
        _rainSource.loop = true;
        _rainSource.clip = rainLoopClip;
        _rainSource.volume = 0f;
    }

    void OnEnable() { Piece.OnPieceKilled += OnPieceKilled; }

    void OnDisable() { Piece.OnPieceKilled -= OnPieceKilled; }

    // Put the music back to normal if this object goes away (e.g. returning to the menu).
    void OnDestroy()
    {
        if (_music != null) _music.SetPitch(1f);
    }

    // ========================
    // DEATH HANDLING
    // ========================

    // -------------------------------------------------------
    // OnPieceKilled(playerIndex)
    // Called for every real piece death. Ignores Lamanite deaths; for a Nephite death, updates
    // the targets for darkness/music and triggers rain and lightning when thresholds are hit.
    // -------------------------------------------------------
    void OnPieceKilled(int playerIndex)
    {
        if (playerIndex != 0) return;

        var spawner = FindAnyObjectByType<PieceSpawner>();
        // The dying piece was already removed from the list, so add it back to learn the starting count
        if (_deaths == 0 && spawner != null) _startingTroops = spawner.players[0].pieces.Count + 1;

        _deaths++;
        if (_startingTroops < 1) _startingTroops = _deaths;

        if (!_isRaining && _deaths >= rainStartsAfterDeaths) StartRain();

        TryStrikeLightning();

        // Reaches the minimum when one troop remains (startingTroops - 1 deaths), whatever the army size
        float deathsToMinimum = Mathf.Max(1, _startingTroops - 1);
        _targetMusicSpeed = Mathf.Lerp(1f, minMusicSpeed, Mathf.Clamp01(_deaths / deathsToMinimum));
    }

    // -------------------------------------------------------
    // TryStrikeLightning()
    // Strikes at half dead, second-to-last, and last, each at most once even when they coincide
    // (tiny armies can make "half" and "second-to-last" the same death).
    // -------------------------------------------------------
    void TryStrikeLightning()
    {
        bool isHalf = flashAtHalfDead && _deaths == Mathf.CeilToInt(_startingTroops / 2f);
        bool isSecondToLast = flashAtSecondToLastDeath && _deaths == _startingTroops - 1;
        bool isLast = flashAtLastDeath && _deaths == _startingTroops;

        if (!(isHalf || isSecondToLast || isLast)) return;
        if (!_flashedAtDeath.Add(_deaths)) return;

        StartCoroutine(StrikeLightning());
    }

    // ========================
    // PER-FRAME VISUALS
    // ========================

    // -------------------------------------------------------
    // LateUpdate()
    // Runs after CameraController so overlays always match the camera's current pan/zoom.
    // Eases darkness and music speed toward their targets and updates the rain strength.
    // -------------------------------------------------------
    void LateUpdate()
    {
        FitOverlaysToCamera();

        // Darkness
        float lossFraction = _startingTroops > 0 ? (float)_deaths / _startingTroops : 0f;
        float targetDarkness = darknessByLossFraction.Evaluate(Mathf.Clamp01(lossFraction)) * maxDarkness;
        if (_isRaining) targetDarkness += extraDarknessWhenRaining;
        targetDarkness = Mathf.Clamp01(targetDarkness);
        _currentDarkness = Mathf.SmoothDamp(_currentDarkness, targetDarkness, ref _darknessVelocity,
            Mathf.Max(0.01f, darknessFadeSeconds));
        _darkness.color = new Color(darknessColor.r, darknessColor.g, darknessColor.b, _currentDarkness);

        // Lightning overlay
        _flash.color = new Color(lightningColor.r, lightningColor.g, lightningColor.b, _flashAlpha);

        // Rain strength grows with losses
        if (_isRaining)
        {
            var emission = _rain.emission;
            emission.rateOverTime = Mathf.Lerp(minRainRate, maxRainRate, lossFraction);
            if (_rainSource.clip != null)
                _rainSource.volume = Mathf.MoveTowards(_rainSource.volume, rainVolume,
                    rainVolume / Mathf.Max(0.01f, rainFadeInSeconds) * Time.deltaTime);
        }

        // Music
        if (_music != null)
        {
            _currentMusicSpeed = Mathf.SmoothDamp(_currentMusicSpeed, _targetMusicSpeed,
                ref _musicSpeedVelocity, Mathf.Max(0.01f, musicSlowSeconds));
            _music.SetPitch(_currentMusicSpeed);
        }
    }

    // -------------------------------------------------------
    // FitOverlaysToCamera()
    // Sizes the full-screen overlays and the rain emitter to the camera's current view.
    // The CameraController zooms/pans, so this must happen every frame.
    // -------------------------------------------------------
    void FitOverlaysToCamera()
    {
        float height = _cam.orthographicSize * 2f + 1f;
        float width = _cam.orthographicSize * 2f * _cam.aspect + 1f;
        _darkness.transform.localScale = new Vector3(width, height, 1f);
        _flash.transform.localScale = new Vector3(width, height, 1f);

        float fallTime = height / Mathf.Max(0.1f, rainFallSpeed);
        var main = _rain.main;
        main.startLifetime = fallTime * 1.1f;
        main.startSize = rainDropSize;
        main.startColor = rainColor;
        var shape = _rain.shape;
        shape.scale = new Vector3(width + Mathf.Abs(rainWindSpeed) * fallTime, 1f, 1f);
        var velocity = _rain.velocityOverLifetime;
        velocity.x = rainWindSpeed;
        velocity.y = -rainFallSpeed;
        _rain.transform.localPosition = new Vector3(0f, _cam.orthographicSize + 0.5f, 5f);
        var rainRenderer = _rain.GetComponent<ParticleSystemRenderer>();
        rainRenderer.lengthScale = rainStreakLength * rainFallSpeed;
    }

    // ========================
    // EFFECTS
    // ========================

    void StartRain()
    {
        _isRaining = true;
        _rain.Play();
        if (_rainSource.clip != null) _rainSource.Play();
    }

    // -------------------------------------------------------
    // StrikeLightning()
    // One strike = several quick white flashes (snap up, fade down), then delayed thunder.
    // -------------------------------------------------------
    IEnumerator StrikeLightning()
    {
        if (lightningClip != null) _sfxSource.PlayOneShot(lightningClip, lightningVolume);
        for (int i = 0; i < Mathf.Max(1, strobesPerStrike); i++)
        {
            // Later strobes are dimmer than the first
            float peak = i == 0 ? lightningPeakAlpha : lightningPeakAlpha * 0.7f;
            for (float t = 0f; t < flashRiseSeconds; t += Time.deltaTime)
            {
                _flashAlpha = Mathf.Lerp(0f, peak, t / Mathf.Max(0.001f, flashRiseSeconds));
                yield return null;
            }
            _flashAlpha = peak;
            for (float t = 0f; t < flashFadeSeconds; t += Time.deltaTime)
            {
                _flashAlpha = Mathf.Lerp(peak, 0f, t / Mathf.Max(0.001f, flashFadeSeconds));
                yield return null;
            }
            _flashAlpha = 0f;
            if (i < strobesPerStrike - 1) yield return new WaitForSeconds(timeBetweenStrobes);
        }

        if (thunderClips != null && thunderClips.Length > 0)
        {
            yield return new WaitForSeconds(Random.Range(thunderDelayRange.x, thunderDelayRange.y));
            var clip = thunderClips[Random.Range(0, thunderClips.Length)];
            if (clip != null) _sfxSource.PlayOneShot(clip, thunderVolume);
        }
    }

    // ========================
    // RUNTIME OBJECT BUILDERS
    // ========================

    static Sprite MakeWhiteSquareSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        // 1 pixel per unit -> a 1x1 world-unit sprite we can scale to cover the screen
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    SpriteRenderer MakeOverlay(string objectName, Sprite sprite, int sortingOrder)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(_cam.transform, false);
        // In front of the board (camera looks down +Z locally)
        go.transform.localPosition = new Vector3(0f, 0f, 5f);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        renderer.color = new Color(0, 0, 0, 0);
        return renderer;
    }

    ParticleSystem MakeRain()
    {
        var go = new GameObject("Rain");
        go.transform.SetParent(_cam.transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startSpeed = 0f; // motion comes from velocityOverLifetime
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 3000;

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = 0f; // all three axes must be set in the same curve mode
        velocity.y = 0f;
        velocity.z = 0f;

        var rainRenderer = go.GetComponent<ParticleSystemRenderer>();
        rainRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        rainRenderer.sortingOrder = OverlaySortingOrder + 1; // above darkness so rain stays visible
        rainRenderer.material = new Material(Shader.Find("Sprites/Default"));
        return ps;
    }
}
