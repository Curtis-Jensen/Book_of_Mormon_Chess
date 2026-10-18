using UnityEngine;

// Restyles a death-particle system so it looks like a rock shattering. The board is seen from
// above, so there is no gravity: shards burst outward across the board, slow to a stop and
// stay put until the next piece of the same side dies, at which point that side's previous
// shards fade away (so one set per side stays visible).
// Shard shapes are generated at runtime so no art assets are needed.
public static class ShatterEffect
{
    const int ShardVariants = 4;
    const float RestLifetime = 3600f; // effectively "until the next kill"
    static Material _shardMaterial;
    static readonly System.Collections.Generic.Dictionary<int, ParticleSystem> _previousShards =
        new System.Collections.Generic.Dictionary<int, ParticleSystem>();

    public static void Apply(ParticleSystem ps, Color tint, int playerIndex)
    {
        if (_previousShards.TryGetValue(playerIndex, out var previous) && previous != null)
            previous.gameObject.AddComponent<ShardFader>();
        _previousShards[playerIndex] = ps;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = false;
        main.duration = 0.5f;
        main.startLifetime = RestLifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.25f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 40;
        // Vary brightness per shard so the chunks read as faceted stone.
        main.startColor = new ParticleSystem.MinMaxGradient(tint * 0.6f, tint);

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14, 22) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.15f;
        shape.radiusThickness = 1f;

        // Drag slows the shards to a stop so they come to rest on the board.
        var limit = ps.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.dampen = 0.08f;
        limit.drag = 4f;
        limit.multiplyDragByParticleSize = false;
        limit.limit = 0f;

        var rotation = ps.rotationOverLifetime;
        rotation.enabled = false;
        var size = ps.sizeOverLifetime;
        size.enabled = false;

        var sheet = ps.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Grid;
        sheet.numTilesX = ShardVariants;
        sheet.numTilesY = 1;
        sheet.animation = ParticleSystemAnimationType.WholeSheet;
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        sheet.startFrame = new ParticleSystem.MinMaxCurve(0f, 1f); // random shard per particle

        var colorLife = ps.colorOverLifetime;
        colorLife.enabled = false;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = GetShardMaterial();
        // Same layer as the death ghosts, which sits below "Pieces" so shards never cover a piece.
        renderer.sortingLayerName = "Corpses";
        renderer.sortingOrder = 1;

        ps.Play(true);
    }

    static Material GetShardMaterial()
    {
        if (_shardMaterial != null) return _shardMaterial;

        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        _shardMaterial = new Material(shader) { mainTexture = BuildShardSheet() };
        // URP particle shaders need these set for a cutout-style, vertex-coloured look.
        if (_shardMaterial.HasProperty("_BaseMap")) _shardMaterial.SetTexture("_BaseMap", _shardMaterial.mainTexture);
        return _shardMaterial;
    }

    // One row of jagged white polygons (grayscale shading per facet) for tinting by startColor.
    static Texture2D BuildShardSheet()
    {
        const int cell = 32;
        var tex = new Texture2D(cell * ShardVariants, cell, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        var clear = new Color32[tex.width * tex.height];
        tex.SetPixels32(clear);

        var rng = new System.Random(7);
        for (int v = 0; v < ShardVariants; v++)
        {
            // Random convex-ish polygon: 3-5 vertices at jittered angles/radii.
            int n = 3 + v % 3;
            var pts = new Vector2[n];
            float start = (float)rng.NextDouble() * Mathf.PI * 2f;
            for (int i = 0; i < n; i++)
            {
                float a = start + (i + (float)rng.NextDouble() * 0.4f) * Mathf.PI * 2f / n;
                float r = cell * (0.28f + (float)rng.NextDouble() * 0.2f);
                pts[i] = new Vector2(cell / 2f + Mathf.Cos(a) * r, cell / 2f + Mathf.Sin(a) * r);
            }
            for (int y = 0; y < cell; y++)
            for (int x = 0; x < cell; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                if (!Inside(pts, p)) continue;
                // Light-to-dark gradient across the shard fakes a chipped facet.
                float shade = Mathf.Lerp(1f, 0.65f, (x + y) / (cell * 2f));
                tex.SetPixel(v * cell + x, y, new Color(shade, shade, shade, 1f));
            }
        }
        tex.Apply();
        return tex;
    }

    static bool Inside(Vector2[] poly, Vector2 p)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                inside = !inside;
        }
        return inside;
    }
}

// Added to the previous shard system when a new piece dies: fades its shards out, then removes it.
public class ShardFader : MonoBehaviour
{
    const float FadeSeconds = 1.2f;

    ParticleSystem _ps;
    ParticleSystem.Particle[] _buffer;
    float _elapsed;

    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        _buffer = new ParticleSystem.Particle[_ps.main.maxParticles];
    }

    void LateUpdate()
    {
        _elapsed += Time.deltaTime;
        float alpha = 1f - Mathf.Clamp01(_elapsed / FadeSeconds);
        int count = _ps.GetParticles(_buffer);
        for (int i = 0; i < count; i++)
        {
            var c = _buffer[i].startColor;
            c.a = (byte)(255 * alpha);
            _buffer[i].startColor = c;
        }
        _ps.SetParticles(_buffer, count);
        if (_elapsed >= FadeSeconds) Destroy(gameObject);
    }
}
