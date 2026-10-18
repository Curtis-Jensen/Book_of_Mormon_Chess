using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -------------------------------------------------------
// LastStandDramatizer
// Makes Nephites' Last Stand grow bleaker as the Nephites (player 0) lose troops:
//   - the screen darkens in proportion to losses
//   - rain begins after a set number of deaths and intensifies
//   - lightning at half dead and on the last two deaths, plus a low random chance on other deaths
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
    [Tooltip("How long each streak is, as seconds of its own motion (0.025 at speed 14 = about 0.35 units)")]
    public float rainStreakLength = 0.025f;
    [Range(0f, 1f)] [Tooltip("How much drop thickness varies (0 = all identical, 1 = from tiny to double size)")]
    public float rainSizeVariation = 0.6f;
    [Range(0f, 0.8f)] [Tooltip("How much each drop's fall speed varies, so they don't move in lockstep")]
    public float rainSpeedVariation = 0.3f;
    [Range(0f, 1f)] [Tooltip("How much the rain swells and thins in waves (0 = steady, 1 = near-stops between gusts)")]
    public float rainGustStrength = 0.7f;
    [Tooltip("How fast the gusts come and go, in waves per second")]
    public float rainGustSpeed = 0.25f;
    [Tooltip("How much the wind direction sways during gusts, in world units per second")]
    public float rainWindSway = 2f;
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
    [Range(0f, 1f)] [Tooltip("Chance of a strike on any other Nephite death, once the rain has started (0 = never, 1 = every death)")]
    public float randomStrikeChance = 0.2f;
    [Tooltip("Always strike when the Nephites have lost half their troops")]
    public bool flashAtHalfDead = true;
    [Tooltip("Flash on the second-to-last Nephite death")]
    public bool flashAtSecondToLastDeath = true;
    [Tooltip("Flash on the last Nephite death")]
    public bool flashAtLastDeath = true;
    public Color lightningColor = new Color(0.85f, 0.9f, 1f, 1f);
    [Range(0f, 1f)] [Tooltip("Screen opacity at the brightest point of a flash")]
    public float lightningPeakAlpha = 0.85f;
    [Tooltip("Fewest quick flashes in one strike")]
    public int minStrobesPerStrike = 1;
    [Tooltip("Most quick flashes in one strike; each strike picks randomly between min and max")]
    public int maxStrobesPerStrike = 2;
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
    // VIGNETTE & COLOR DRAIN
    // ========================
    [Header("Vignette (closing-in edges)")]
    [Range(0f, 1f)] [Tooltip("Edge darkness once only one Nephite is left; grows with losses")]
    public float maxVignetteOpacity = 0.85f;
    [Range(0f, 0.95f)] [Tooltip("Where the dark starts, as a fraction of the way out from the center (smaller = more tunnel-like)")]
    public float vignetteInnerRadius = 0.35f;
    public Color vignetteColor = Color.black;

    [Header("Color drain")]
    [Tooltip("How far colors wash toward gray once only one Nephite is left (0 = none). Keep small for a subtle effect")]
    [Range(0f, 1f)] public float maxColorDrain = 0.25f;

    // ========================
    // CAMERA SHAKE
    // ========================
    [Header("Camera shake")]
    [Tooltip("Shake strength on each Nephite death, as a fraction of half the screen height")]
    public float deathShakeStrength = 0.03f;
    public float deathShakeSeconds = 0.25f;
    [Tooltip("Shake strength when lightning strikes (timed to the thunder sound)")]
    public float thunderShakeStrength = 0.09f;
    public float thunderShakeSeconds = 0.7f;
    [Tooltip("Seconds between a strike's first flash and the thunder shake, so the rumble lands after the light")]
    public float thunderShakeDelay = 0.1f;
    [Tooltip("How fast the shake jitters")]
    public float shakeFrequency = 28f;

    // ========================
    // HEARTBEAT (last troop only)
    // ========================
    [Header("Heartbeat (only while one Nephite remains)")]
    public AudioClip heartbeatClip;
    [Range(0f, 1f)] public float heartbeatVolume = 0.9f;
    [Tooltip("Beats per minute when the last troop is left")]
    public float heartbeatStartBpm = 70f;
    [Tooltip("Beats per minute it climbs to")]
    public float heartbeatMaxBpm = 120f;
    [Tooltip("Seconds it takes to climb from start to max BPM")]
    public float heartbeatRampSeconds = 25f;

    // ========================
    // MUSIC MUFFLING
    // ========================
    [Header("Music muffling")]
    [Tooltip("Music high-frequency cutoff (Hz) when no troops are lost; 22000 = untouched")]
    public float musicCutoffStart = 22000f;
    [Tooltip("Cutoff (Hz) once only one Nephite is left; lower = more muffled")]
    public float musicCutoffEnd = 1800f;

    // ========================
    // RUNTIME STATE
    // ========================
    const int OverlaySortingOrder = 500; // Above pieces/tiles; UI canvases are unaffected
    // Draw order: color drain 498, darkness 499, vignette 500, rain 501, lightning 502

    int _deaths;
    int _startingTroops;
    bool _isRaining;
    readonly HashSet<int> _flashedAtDeath = new HashSet<int>();

    Camera _cam;
    MusicPlayer _music;
    SpriteRenderer _darkness;
    SpriteRenderer _vignette;
    SpriteRenderer _drain;
    AudioLowPassFilter _musicFilter;
    AudioSource _heartbeatSource;
    Coroutine _heartbeatRoutine;
    Vector3 _shakeOffset;
    float _shakeAmplitude;
    float _shakeSecondsLeft;
    float _shakeTotalSeconds;
    float _currentVignette;
    float _vignetteVelocity;
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
        _drain = MakeOverlay("Color Drain Overlay", square, OverlaySortingOrder - 2);
        _darkness = MakeOverlay("Darkness Overlay", square, OverlaySortingOrder - 1);
        _vignette = MakeOverlay("Vignette Overlay", MakeVignetteSprite(), OverlaySortingOrder);
        _flash = MakeOverlay("Lightning Overlay", square, OverlaySortingOrder + 2);
        _rain = MakeRain();

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _rainSource = gameObject.AddComponent<AudioSource>();
        _rainSource.playOnAwake = false;
        _rainSource.loop = true;
        _rainSource.clip = rainLoopClip;
        _rainSource.volume = 0f;
        _heartbeatSource = gameObject.AddComponent<AudioSource>();
        _heartbeatSource.playOnAwake = false;

        // Muffles the music as troops fall; starts wide open so nothing changes until the first death
        if (_music != null)
        {
            _musicFilter = _music.gameObject.AddComponent<AudioLowPassFilter>();
            _musicFilter.cutoffFrequency = musicCutoffStart;
        }
    }

    void OnEnable()
    {
        Piece.OnPieceKilled += OnPieceKilled;
        HoardEndingManager.OnGameEnd += OnGameEnded;
    }

    void OnDisable()
    {
        Piece.OnPieceKilled -= OnPieceKilled;
        HoardEndingManager.OnGameEnd -= OnGameEnded;
    }

    // The game is over, so the heartbeat has nothing left to beat for.
    void OnGameEnded(int playerIndex)
    {
        if (_heartbeatRoutine != null) StopCoroutine(_heartbeatRoutine);
        _heartbeatRoutine = null;
    }

    // Put the music back to normal if this object goes away (e.g. returning to the menu).
    void OnDestroy()
    {
        if (_music != null) _music.SetPitch(1f);
        if (_musicFilter != null) Destroy(_musicFilter);
        if (_cam != null) _cam.transform.position -= _shakeOffset;
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
        AddShake(deathShakeStrength, deathShakeSeconds);

        // Only one troop left: start the heartbeat
        if (_deaths == _startingTroops - 1 && _startingTroops >= 2 && heartbeatClip != null && _heartbeatRoutine == null)
            _heartbeatRoutine = StartCoroutine(BeatHeart());

        // Reaches the minimum when one troop remains (startingTroops - 1 deaths), whatever the army size
        float deathsToMinimum = Mathf.Max(1, _startingTroops - 1);
        _targetMusicSpeed = Mathf.Lerp(1f, minMusicSpeed, Mathf.Clamp01(_deaths / deathsToMinimum));
    }

    // -------------------------------------------------------
    // TryStrikeLightning()
    // Always strikes at half dead and on the last two deaths, and randomly (randomStrikeChance) on others.
    // Each death strikes at most once.
    // -------------------------------------------------------
    void TryStrikeLightning()
    {
        bool isHalf = flashAtHalfDead && _deaths == Mathf.CeilToInt(_startingTroops / 2f);
        bool isSecondToLast = flashAtSecondToLastDeath && _deaths == _startingTroops - 1;
        bool isLast = flashAtLastDeath && _deaths == _startingTroops;
        // Other deaths: a low random chance, but only once the storm has actually begun
        bool isRandom = _isRaining && Random.value < randomStrikeChance;

        if (!(isHalf || isSecondToLast || isLast || isRandom)) return;
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
        ApplyShake();
        FitOverlaysToCamera();

        // Darkness
        float lossFraction = _startingTroops > 0 ? (float)_deaths / _startingTroops : 0f;
        float targetDarkness = darknessByLossFraction.Evaluate(Mathf.Clamp01(lossFraction)) * maxDarkness;
        if (_isRaining) targetDarkness += extraDarknessWhenRaining;
        targetDarkness = Mathf.Clamp01(targetDarkness);
        _currentDarkness = Mathf.SmoothDamp(_currentDarkness, targetDarkness, ref _darknessVelocity,
            Mathf.Max(0.01f, darknessFadeSeconds));
        _darkness.color = new Color(darknessColor.r, darknessColor.g, darknessColor.b, _currentDarkness);

        // Vignette tightens and color drains as troops fall (both reach their max with one troop left)
        float lastTroopProgress = _startingTroops > 1
            ? Mathf.Clamp01(_deaths / Mathf.Max(1f, _startingTroops - 1f)) : 0f;
        _currentVignette = Mathf.SmoothDamp(_currentVignette, lastTroopProgress * maxVignetteOpacity,
            ref _vignetteVelocity, Mathf.Max(0.01f, darknessFadeSeconds));
        _vignette.color = new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, _currentVignette);
        _drain.color = new Color(0.5f, 0.5f, 0.5f, lastTroopProgress * maxColorDrain);
        if (_musicFilter != null)
            _musicFilter.cutoffFrequency = Mathf.Lerp(musicCutoffStart, musicCutoffEnd, lastTroopProgress);

        // Lightning overlay
        _flash.color = new Color(lightningColor.r, lightningColor.g, lightningColor.b, _flashAlpha);

        // Rain strength grows with losses
        if (_isRaining)
        {
            var emission = _rain.emission;
            // Perlin noise makes the rain swell and thin unpredictably instead of a constant stream
            float gust = Mathf.PerlinNoise(Time.time * rainGustSpeed, 3.7f);
            float gustFactor = Mathf.Lerp(1f, Mathf.Lerp(0.15f, 1.4f, gust), rainGustStrength);
            emission.rateOverTime = Mathf.Lerp(minRainRate, maxRainRate, lossFraction) * gustFactor;
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
        _vignette.transform.localScale = new Vector3(width, height, 1f);
        _drain.transform.localScale = new Vector3(width, height, 1f);

        // Slowest drops set the lifetime so every drop can cross the whole screen
        float slowestFall = Mathf.Max(0.1f, rainFallSpeed * (1f - rainSpeedVariation));
        float fallTime = height / slowestFall;
        var main = _rain.main;
        main.startLifetime = fallTime * 1.05f;
        main.startSize = new ParticleSystem.MinMaxCurve(rainDropSize * (1f - rainSizeVariation * 0.5f),
            rainDropSize * (1f + rainSizeVariation));
        // Random alpha per drop so some read as nearer/fainter
        Color faint = new Color(rainColor.r, rainColor.g, rainColor.b, rainColor.a * 0.4f);
        main.startColor = new ParticleSystem.MinMaxGradient(faint, rainColor);
        var shape = _rain.shape;
        shape.scale = new Vector3(width + (Mathf.Abs(rainWindSpeed) + rainWindSway) * fallTime, 1f, 1f);
        var velocity = _rain.velocityOverLifetime;
        velocity.y = new ParticleSystem.MinMaxCurve(-rainFallSpeed * (1f + rainSpeedVariation),
            -rainFallSpeed * (1f - rainSpeedVariation));
        // Wind sways slowly with a second noise lane so it isn't the same as the gust rhythm
        float sway = (Mathf.PerlinNoise(Time.time * rainGustSpeed * 0.7f, 17.3f) - 0.5f) * 2f * rainWindSway;
        float wind = rainWindSpeed + sway;
        // All three axes must share a curve mode, so x and z are two-constant ranges too
        velocity.x = new ParticleSystem.MinMaxCurve(wind, wind);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        _rain.transform.localPosition = new Vector3(0f, _cam.orthographicSize + 0.5f, 5f);
        var rainRenderer = _rain.GetComponent<ParticleSystemRenderer>();
        rainRenderer.velocityScale = rainStreakLength;
        rainRenderer.lengthScale = 1f;
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
        StartCoroutine(ShakeAfterDelay(thunderShakeDelay));
        int strobes = Random.Range(Mathf.Max(1, minStrobesPerStrike), Mathf.Max(minStrobesPerStrike, maxStrobesPerStrike) + 1);
        for (int i = 0; i < strobes; i++)
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
            if (i < strobes - 1) yield return new WaitForSeconds(timeBetweenStrobes);
        }

        if (thunderClips != null && thunderClips.Length > 0)
        {
            yield return new WaitForSeconds(Random.Range(thunderDelayRange.x, thunderDelayRange.y));
            var clip = thunderClips[Random.Range(0, thunderClips.Length)];
            if (clip != null) _sfxSource.PlayOneShot(clip, thunderVolume);
        }
    }

    // -------------------------------------------------------
    // AddShake(strength, seconds)
    // Starts (or strengthens) a camera shake. Strength is a fraction of half the screen height.
    // A new shake only replaces the current one if it would be stronger than what's left of it.
    // -------------------------------------------------------
    void AddShake(float strength, float seconds)
    {
        float remaining = _shakeTotalSeconds > 0f ? _shakeAmplitude * (_shakeSecondsLeft / _shakeTotalSeconds) : 0f;
        if (strength < remaining) return;
        _shakeAmplitude = strength;
        _shakeSecondsLeft = _shakeTotalSeconds = Mathf.Max(0.01f, seconds);
    }

    IEnumerator ShakeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AddShake(thunderShakeStrength, thunderShakeSeconds);
    }

    // -------------------------------------------------------
    // ApplyShake()
    // CameraController moves the camera with += each frame, so we first undo last frame's shake
    // offset (restoring the true position) and then apply a fresh one. The overlays are children
    // of the camera, so they shake with it.
    // -------------------------------------------------------
    void ApplyShake()
    {
        _cam.transform.position -= _shakeOffset;
        _shakeOffset = Vector3.zero;
        if (_shakeSecondsLeft <= 0f) return;

        _shakeSecondsLeft -= Time.deltaTime;
        float fade = Mathf.Clamp01(_shakeSecondsLeft / _shakeTotalSeconds);
        float size = _cam.orthographicSize * _shakeAmplitude * fade;
        float t = Time.time * shakeFrequency;
        _shakeOffset = new Vector3(
            (Mathf.PerlinNoise(t, 0.5f) - 0.5f) * 2f * size,
            (Mathf.PerlinNoise(0.5f, t) - 0.5f) * 2f * size, 0f);
        _cam.transform.position += _shakeOffset;
    }

    // -------------------------------------------------------
    // BeatHeart()
    // Plays the lub-dub clip on a loop, gradually speeding up. Runs from the second-to-last
    // death until the game ends.
    // -------------------------------------------------------
    IEnumerator BeatHeart()
    {
        float elapsed = 0f;
        while (true)
        {
            _heartbeatSource.PlayOneShot(heartbeatClip, heartbeatVolume);
            float bpm = Mathf.Lerp(heartbeatStartBpm, heartbeatMaxBpm,
                Mathf.Clamp01(elapsed / Mathf.Max(0.01f, heartbeatRampSeconds)));
            float wait = 60f / Mathf.Max(1f, bpm);
            elapsed += wait;
            yield return new WaitForSeconds(wait);
        }
    }

    // ========================
    // RUNTIME OBJECT BUILDERS
    // ========================

    // Square sprite that's clear in the middle and dark at the edges (round fade, stretched to the screen).
    Sprite MakeVignetteSprite()
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                // 1.0 at the middle of each edge, ~1.41 at corners
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(vignetteInnerRadius, 1.2f, r));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    static Sprite MakeWhiteSquareSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        // 1 pixel per unit -> a 1x1 world-unit sprite we can scale to cover the screen
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    // A thin vertical streak with soft edges and a faded tail, so drops look like rain instead of squares.
    // Stretch-billboard particles orient the texture's long axis along their motion.
    static Texture2D MakeStreakTexture()
    {
        const int w = 8, h = 64;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < h; y++)
        {
            float v = y / (h - 1f);
            // Tail (top) fades in; head (bottom) ends in a quick round-off
            float along = Mathf.SmoothStep(0f, 1f, v / 0.7f) * Mathf.SmoothStep(0f, 1f, (1f - v) / 0.12f);
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w * 2f - 1f;
                float across = Mathf.Exp(-u * u * 5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, along * across));
            }
        }
        tex.Apply();
        return tex;
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
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f); // same curve mode on every axis
        velocity.y = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var rainRenderer = go.GetComponent<ParticleSystemRenderer>();
        rainRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        rainRenderer.sortingOrder = OverlaySortingOrder + 1; // above darkness so rain stays visible
        var material = new Material(Shader.Find("Sprites/Default"));
        material.mainTexture = MakeStreakTexture();
        rainRenderer.material = material;
        return ps;
    }
}
