using UnityEngine;

/// <summary>
/// Conducts the three-phase threat update after each move, in guaranteed order:
///   1. Clear all tile threat lists
///   2. Each piece declares what it threatens
///   3. Fire OnThreatsReady so subscribers (e.g. King) can read fresh data
/// </summary>
public class ThreatCoordinator : MonoBehaviour
{
    public static ThreatCoordinator Instance { get; private set; }

    public delegate void ThreatsReadyHandler();
    public event ThreatsReadyHandler OnThreatsReady;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        TurnManager.Instance.OnMoveEnd += UpdateThreats;
    }

    void UpdateThreats()
    {
        RebuildSilently();

        // ✅ Phase 3: Notify subscribers that threat data is fresh
        OnThreatsReady?.Invoke();
    }

    // 🔇 Runs phases 1 and 2 only — used by simulation so no game reactions fire mid-check
    public void RebuildSilently()
    {
        // 🧹 Phase 1: Clear all tile threat lists
        var tiles = TurnManager.Instance.tiles;
        foreach (var tile in tiles)
            tile.GetComponent<TileThreats>().Clear();

        // 📢 Phase 2: Each piece declares what it threatens
        foreach (var player in TurnManager.Instance.pieceSpawner.players)
            foreach (var piece in player.pieces)
                piece.DeclareThreats();
    }
}
