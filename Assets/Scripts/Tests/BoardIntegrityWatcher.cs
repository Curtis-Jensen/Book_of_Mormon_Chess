using UnityEngine;

// -------------------------------------------------------
// BoardIntegrityWatcher
// Runs quietly during real play, listening to TurnProgresser.OnMoveEnd.
// After every move it cross-checks the tiles[] grid against every piece's
// own transform position, in both directions, and logs the exact move
// number the moment they disagree.
// Written to catch the float-drift desync bug (a piece's transform easing
// just under an integer, e.g. 0.9999995, so grid-index rounding/truncation
// picked the wrong tile) -- but it doesn't assume that's the only possible
// cause, so it stays on as a general low-cost safety net.
// -------------------------------------------------------
public class BoardIntegrityWatcher : MonoBehaviour
{
    int moveCount = 0;

    void Start()
    {
        TurnProgresser.Instance.OnMoveEnd += CheckBoard;
    }

    void OnDestroy()
    {
        if (TurnProgresser.Instance != null) TurnProgresser.Instance.OnMoveEnd -= CheckBoard;
    }

    // -------------------------------------------------------
    // CheckBoard()
    // Fires after every move (TurnProgresser.OnMoveEnd). Scans the whole
    // board for any piece whose registered tile doesn't match its own
    // transform position, or vice versa, and logs the move number that
    // first exposed the mismatch.
    // -------------------------------------------------------
    void CheckBoard()
    {
        moveCount++;

        var turnProgresser = TurnProgresser.Instance;
        var tiles = turnProgresser.tiles;
        var boardSize = turnProgresser.boardSize;

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                var tile = tiles[x, y];
                if (tile.piece == null) continue;

                var piecePos = tile.piece.transform.position;
                if (Mathf.RoundToInt(piecePos.x) != x || Mathf.RoundToInt(piecePos.y) != y)
                {
                    Debug.LogError($"🩺 Board desync after move #{moveCount}: tiles[{x},{y}].piece is " +
                        $"{tile.piece.name}, but that piece's transform is at ({piecePos.x}, {piecePos.y}).");
                }
            }
        }

        foreach (var player in turnProgresser.pieceSpawner.players)
        {
            foreach (var piece in player.pieces)
            {
                if (piece == null) continue;

                var pos = piece.transform.position;
                int px = Mathf.RoundToInt(pos.x), py = Mathf.RoundToInt(pos.y);
                if (px < 0 || px >= boardSize || py < 0 || py >= boardSize)
                {
                    Debug.LogError($"🩺 Board desync after move #{moveCount}: {piece.name} is off-board at ({pos.x}, {pos.y}).");
                    continue;
                }

                if (tiles[px, py].piece != piece)
                {
                    var occupantName = tiles[px, py].piece == null ? "null" : tiles[px, py].piece.name;
                    Debug.LogError($"🩺 Board desync after move #{moveCount}: {piece.name} sits at ({px},{py}) " +
                        $"but tiles[{px},{py}].piece is {occupantName}.");
                }
            }
        }
    }
}
