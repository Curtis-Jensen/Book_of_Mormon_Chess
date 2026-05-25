using System.Collections;
using UnityEngine;

/// <summary>
/// ⚔️ Tracks the percentage of Nephite troops remaining and publishes events as troops fall.
///
/// This script is intentionally separate from wave defense logic.  Its only job is to
/// watch how many of the player's pieces survive, then broadcast that ratio so other
/// systems (music, lighting, etc.) can react however they like.
///
/// Subscribe to <see cref="OnTroopsPercentageChanged"/> to receive updates.
/// The value ranges from 1.0 (all troops alive) down to 0.0 (no troops left).
/// </summary>
public class NephitesMoodTracker : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────
    //  Singleton
    // ──────────────────────────────────────────────────────────────
    public static NephitesMoodTracker Instance { get; private set; }

    // ──────────────────────────────────────────────────────────────
    //  Event
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Fired after every move whenever the Nephite troop count has changed.
    /// <para>Parameter: fraction of original troops still alive (0.0 – 1.0).</para>
    /// </summary>
    public delegate void TroopPercentageHandler(float nephiteTroopsRemainingPercent);
    public static event TroopPercentageHandler OnTroopsPercentageChanged;

    // ──────────────────────────────────────────────────────────────
    //  Private state
    // ──────────────────────────────────────────────────────────────
    int initialNephiteTroopCount;
    int lastReportedCount = -1; // 🔇 Suppress duplicate events

    // ──────────────────────────────────────────────────────────────
    //  Unity lifecycle
    // ──────────────────────────────────────────────────────────────
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // ⏳ Wait one frame so BoardSetup.Start() and HoardTurnManager.Start()
        //    have both finished spawning all pieces before we record the baseline.
        StartCoroutine(InitializeAfterSpawn());
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnMoveEnd -= HandleMoveEnd;
    }

    // ──────────────────────────────────────────────────────────────
    //  Initialization
    // ──────────────────────────────────────────────────────────────
    IEnumerator InitializeAfterSpawn()
    {
        yield return null; // wait one frame

        initialNephiteTroopCount = GetNephiteTroopCount();
        lastReportedCount = initialNephiteTroopCount;

        Debug.Log($"⚔️ NephitesMoodTracker: Nephite army starts with {initialNephiteTroopCount} troops");

        if (TurnManager.Instance != null)
            TurnManager.Instance.OnMoveEnd += HandleMoveEnd;
        else
            Debug.LogWarning("⚔️ NephitesMoodTracker: TurnManager.Instance not found – mood events will not fire");
    }

    // ──────────────────────────────────────────────────────────────
    //  Move-end handler
    // ──────────────────────────────────────────────────────────────
    void HandleMoveEnd()
    {
        if (initialNephiteTroopCount <= 0) return;

        int currentCount = GetNephiteTroopCount();

        // 🔇 Only broadcast when the count actually changes
        if (currentCount == lastReportedCount) return;
        lastReportedCount = currentCount;

        float percentage = Mathf.Clamp01((float)currentCount / initialNephiteTroopCount);

        Debug.Log($"⚔️ NephitesMoodTracker: {currentCount}/{initialNephiteTroopCount} Nephite troops remaining ({percentage * 100f:F0}%)");

        OnTroopsPercentageChanged?.Invoke(percentage);
    }

    // ──────────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the total number of living pieces belonging to Nephite players.
    /// </summary>
    int GetNephiteTroopCount()
    {
        var spawner = FindAnyObjectByType<PieceSpawner>();
        if (spawner == null) return 0;

        int count = 0;
        foreach (var player in spawner.players)
        {
            if (player.faction == Faction.Nephite)
                count += player.pieces.Count;
        }
        return count;
    }
}
