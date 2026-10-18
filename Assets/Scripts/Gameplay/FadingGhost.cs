using System.Collections;
using UnityEngine;

// -------------------------------------------------------
// FadingGhost
// A death ghost that fades away and disappears after a number of turns, so corpses don't
// pile up (e.g. the Lamanite pawns in Nephites' Last Stand). Put it on a variant of the Ghost
// prefab and assign that variant to a piece's Ghost field.
// Counts turns by subscribing to TurnProgresser.OnMoveEnd (one per move, so two moves = one round).
// -------------------------------------------------------
public class FadingGhost : MonoBehaviour
{
    [Tooltip("Turns (moves) before the ghost starts fading")]
    public int lifetimeTurns = 6;
    [Tooltip("Seconds the fade-out takes")]
    public float fadeSeconds = 1.5f;

    int _turnsElapsed;
    bool _fading;
    TurnProgresser _turns;

    void OnEnable()
    {
        _turns = TurnProgresser.Instance;
        if (_turns != null) _turns.OnMoveEnd += OnTurnPassed;
    }

    void OnDisable()
    {
        if (_turns != null) _turns.OnMoveEnd -= OnTurnPassed;
    }

    void OnTurnPassed()
    {
        if (_fading || ++_turnsElapsed < lifetimeTurns) return;
        _fading = true;
        StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FadeAndDestroy()
    {
        var sr = GetComponent<SpriteRenderer>();
        Color start = sr.color;
        for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
        {
            sr.color = new Color(start.r, start.g, start.b,
                Mathf.Lerp(start.a, 0f, t / Mathf.Max(0.01f, fadeSeconds)));
            yield return null;
        }
        Destroy(gameObject);
    }
}
