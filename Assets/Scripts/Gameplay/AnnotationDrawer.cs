using System.Collections.Generic;
using UnityEngine;

public class AnnotationDrawer : MonoBehaviour
{
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private AudioClip annotateClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private Color fallbackArrowColor = new Color(1f, 0.987f, 0.542f, 0.404f);
    [SerializeField, Range(0f, 0.3f)] private float pitchVariation = 0.1f;
    [SerializeField] private float arrowWidth = 0.07f;
    [SerializeField, Range(0f, 0.1f)] private float scratchiness = 0.03f;
    [SerializeField] private float arrowHeadLength = 0.4f;
    [SerializeField] private float arrowHeadWidth = 0.4f;
    [SerializeField] private int arrowSortingOrder = 50;

    private GameObject circleInstance;
    // Arrows that start on this tile, keyed by the tile they point at
    private readonly Dictionary<AnnotationDrawer, GameObject> arrows = new Dictionary<AnnotationDrawer, GameObject>();

    // The tile where the current right-button drag began
    private static AnnotationDrawer dragStart;
    private static Material arrowMaterial;

    void Start()
    {
        if (TurnProgresser.Instance != null) TurnProgresser.Instance.OnMoveEnd += ClearAnnotations;
    }

    void OnDestroy()
    {
        if (TurnProgresser.Instance != null) TurnProgresser.Instance.OnMoveEnd -= ClearAnnotations;
    }

    // Annotations are only relevant to the position they were drawn in
    void ClearAnnotations()
    {
        if (circleInstance != null) DestroyAnnotation(circleInstance);
        circleInstance = null;
        foreach (var arrow in arrows.Values)
        {
            if (arrow != null) DestroyAnnotation(arrow);
        }
        arrows.Clear();
    }

    // Same gold as the circle annotation, so every annotation reads as the viewer's own marking
    Color GetArrowColor()
    {
        var circleSprite = circlePrefab != null ? circlePrefab.GetComponent<SpriteRenderer>() : null;
        if (circleSprite == null) return fallbackArrowColor;
        var gold = circleSprite.color;
        return gold;
    }

    // Destroy isn't allowed outside play mode, which the edit-mode tests run in
    static void DestroyAnnotation(GameObject go)
    {
        if (Application.isPlaying) Destroy(go);
        else DestroyImmediate(go);
    }

    public int ArrowCount => arrows.Count;
    public bool HasCircle => circleInstance != null;

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && IsUnderMouse()) Press(this);
        if (Input.GetMouseButtonUp(1) && IsUnderMouse()) Release(this);
    }

    // Every tile's Update runs in arbitrary order, so a tile must never clear the drag
    // on its own; the drag is only cleared here, after all Updates have seen the release.
    void LateUpdate()
    {
        if (Input.GetMouseButtonUp(1)) dragStart = null;
    }

    // Right button went down on this tile
    public static void Press(AnnotationDrawer tile)
    {
        dragStart = tile;
    }

    // Right button came up on this tile: same tile is a circle, another tile is an arrow
    public static void Release(AnnotationDrawer tile)
    {
        if (dragStart == null) return;
        if (dragStart == tile) tile.ToggleCircle();
        else dragStart.ToggleArrow(tile);
        dragStart = null;
    }

    bool IsUnderMouse()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // A piece standing on the tile sits in front of it, so check everything under the cursor
        foreach (var hit in Physics2D.RaycastAll(worldPos, Vector2.zero))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject) return true;
        }
        return false;
    }

    void ToggleCircle()
    {
        if (circleInstance != null)
        {
            DestroyAnnotation(circleInstance);
            circleInstance = null;
        }
        else if (circlePrefab != null)
        {
            PlaySound();
            circleInstance = Instantiate(circlePrefab, transform.position, Quaternion.identity, transform);
        }
    }

    void ToggleArrow(AnnotationDrawer target)
    {
        if (arrows.TryGetValue(target, out GameObject existing))
        {
            if (existing != null) DestroyAnnotation(existing);
            arrows.Remove(target);
            return;
        }
        PlaySound();
        arrows[target] = BuildArrow(target.transform.position);
    }

    GameObject BuildArrow(Vector3 endWorld)
    {
        Vector3 start = transform.position;
        Vector2 delta = endWorld - start;
        float length = delta.magnitude;
        float headLen = Mathf.Min(arrowHeadLength, length * 0.5f);
        float shaftLen = length - headLen;
        float hh = arrowHeadWidth * 0.5f;

        // Built in local space pointing along +X, then rotated into place.
        // Each line is drawn twice with a little wobble, like a pen going over it.
        var verts = new List<Vector3>();
        var tris = new List<int>();
        var tip = new Vector2(length, 0);
        for (int pass = 0; pass < 2; pass++)
        {
            AddScratchyStroke(verts, tris, Vector2.zero, new Vector2(shaftLen, 0));
            AddScratchyStroke(verts, tris, tip, new Vector2(shaftLen, -hh));
            AddScratchyStroke(verts, tris, tip, new Vector2(shaftLen, hh));
        }

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        var color = GetArrowColor();
        var colors = new Color[verts.Count];
        for (int i = 0; i < colors.Length; i++) colors[i] = color;
        mesh.colors = colors;

        var go = new GameObject("ArrowAnnotation");
        go.transform.SetParent(transform, true);
        go.transform.position = start;
        go.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        if (arrowMaterial == null) arrowMaterial = new Material(Shader.Find("Sprites/Default"));
        mr.sharedMaterial = arrowMaterial;
        mr.sortingOrder = arrowSortingOrder;
        return go;
    }

    // A thin wobbly strip from a to b: points jitter sideways and the width varies along it
    void AddScratchyStroke(List<Vector3> verts, List<int> tris, Vector2 a, Vector2 b)
    {
        Vector2 dir = b - a;
        float len = dir.magnitude;
        if (len < 0.001f) return;
        dir /= len;
        Vector2 normal = new Vector2(-dir.y, dir.x);
        int segments = Mathf.Max(2, Mathf.CeilToInt(len / 0.25f));
        int first = verts.Count;
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector2 p = Vector2.Lerp(a, b, t) + normal * Random.Range(-scratchiness, scratchiness);
            if (i == 0 || i == segments) p += dir * Random.Range(-scratchiness, scratchiness);
            float hw = arrowWidth * 0.5f * Random.Range(0.6f, 1.2f);
            verts.Add(p + normal * hw);
            verts.Add(p - normal * hw);
            if (i > 0)
            {
                int k = first + (i - 1) * 2;
                tris.AddRange(new[] { k, k + 1, k + 2, k + 1, k + 3, k + 2 });
            }
        }
    }

    void PlaySound()
    {
        if (annotateClip == null) return;
        float sfx = PlayerPrefs.GetFloat(ButtonClickSounds.VolumeKey, ButtonClickSounds.DefaultVolume);
        // Slight random pitch so repeated annotations don't sound identical
        var go = new GameObject("AnnotationSound");
        go.transform.position = Camera.main.transform.position;
        var src = go.AddComponent<AudioSource>();
        src.clip = annotateClip;
        src.volume = volume * sfx;
        src.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        src.Play();
        Destroy(go, annotateClip.length / src.pitch + 0.1f);
    }
}
