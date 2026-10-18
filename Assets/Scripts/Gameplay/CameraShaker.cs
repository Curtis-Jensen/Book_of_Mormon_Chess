using UnityEngine;

// Small screen shake for big moments: a pawn promoting or a queen dying.
// Installs itself at startup, so no scene setup is needed.
public class CameraShaker : MonoBehaviour
{
    const float Strength = 0.04f; // fraction of half the screen height
    const float Seconds = 0.3f;
    const float Frequency = 30f;

    Camera _cam;
    Vector3 _offset;
    float _secondsLeft;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        var go = new GameObject("CameraShaker");
        DontDestroyOnLoad(go); // the game scene is usually loaded after startup, so survive scene changes
        go.AddComponent<CameraShaker>();
    }

    void OnEnable()
    {
        Pawn.OnPromoted += Shake;
        Piece.OnQueenKilled += Shake;
    }

    void OnDisable()
    {
        Pawn.OnPromoted -= Shake;
        Piece.OnQueenKilled -= Shake;
        Restore();
    }

    void Shake() => _secondsLeft = Seconds;

    void Restore()
    {
        if (_cam != null) _cam.transform.position -= _offset;
        _offset = Vector3.zero;
    }

    // Runs after CameraController: undo last frame's offset, then apply a fresh one.
    void LateUpdate()
    {
        var main = Camera.main;
        if (main != _cam)
        {
            _offset = Vector3.zero; // a different camera: the old offset no longer applies
            _cam = main;
        }
        if (_cam == null) return;

        _cam.transform.position -= _offset;
        _offset = Vector3.zero;
        if (_secondsLeft <= 0f) return;

        _secondsLeft -= Time.deltaTime;
        float size = _cam.orthographicSize * Strength * Mathf.Clamp01(_secondsLeft / Seconds);
        float t = Time.time * Frequency;
        _offset = new Vector3((Mathf.PerlinNoise(t, 0.5f) - 0.5f) * 2f * size,
                              (Mathf.PerlinNoise(0.5f, t) - 0.5f) * 2f * size, 0f);
        _cam.transform.position += _offset;
    }
}
