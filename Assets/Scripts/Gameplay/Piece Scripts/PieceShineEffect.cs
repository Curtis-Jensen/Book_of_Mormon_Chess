using System.Collections;
using UnityEngine;

/// <summary>
/// Periodically sweeps a diagonal shine across a piece so players notice it's special.
/// Mirrors the source SpriteRenderer's current sprite each time it plays, since pieces
/// like StriplingWarrior swap sprites (e.g. wounded state) after this component starts.
/// </summary>
public class PieceShineEffect : MonoBehaviour
{
    [SerializeField] SpriteRenderer sourceRenderer;
    [SerializeField] SpriteRenderer shineRenderer;
    [SerializeField] float interval = 15f;
    [SerializeField, Range(0f, 1f)] float intervalRandomness = 0.3f;
    [SerializeField] float sweepDuration = 0.9f;
    [SerializeField] Color shineColor = new(1f, 1f, 1f, 0.6f);

    const float startOffset = -0.4f;
    const float endOffset = 1.4f;

    Material shineMaterial;
    static readonly int ShineOffsetId = Shader.PropertyToID("_ShineOffset");

    void Awake()
    {
        if (sourceRenderer == null) sourceRenderer = GetComponent<SpriteRenderer>();

        shineMaterial = new Material(Shader.Find("Sprites/PieceShine"));
        shineMaterial.SetColor("_ShineColor", shineColor);
        shineRenderer.material = shineMaterial;
        shineRenderer.enabled = false;
    }

    void OnEnable()
    {
        StartCoroutine(ShineLoop());
    }

    // Piece sprites get assigned by PieceSpawner right after this component's Awake runs,
    // and swap again on wound/unwound, so keep mirroring the source sprite every frame
    // rather than only once when a sweep starts.
    void Update()
    {
        if (shineRenderer.sprite != sourceRenderer.sprite)
            shineRenderer.sprite = sourceRenderer.sprite;
    }

    IEnumerator ShineLoop()
    {
        while (true)
        {
            float wait = interval * Random.Range(1f - intervalRandomness, 1f + intervalRandomness);
            yield return new WaitForSeconds(wait);
            yield return Sweep();
        }
    }

    IEnumerator Sweep()
    {
        if (sourceRenderer.sprite == null) yield break;

        shineRenderer.enabled = true;

        float elapsed = 0f;
        while (elapsed < sweepDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / sweepDuration;
            shineMaterial.SetFloat(ShineOffsetId, Mathf.Lerp(startOffset, endOffset, t));
            yield return null;
        }

        shineRenderer.enabled = false;
    }
}
