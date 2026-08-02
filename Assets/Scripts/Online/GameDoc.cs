using System.Collections.Generic;

// A single tile-to-tile move, replayed through TurnProgresser's real move pipeline
// on the receiving end (OnTileClicked/MovePiece) rather than reconstructed from a
// snapshot -- that way captures, promotion, StriplingWarrior's wounding, etc. all
// go through their actual gameplay logic instead of us reimplementing it. Sentinel
// -1 (rather than null) marks "no move yet", to keep FirestoreJson's mapping simple.
public class LastMoveDto
{
    public int fromX = -1;
    public int fromY = -1;
    public int toX = -1;
    public int toY = -1;
}

// The full Firestore document for one correspondence game.
// Split from GameStateDto because GameStateDto only carries board/turn state
// (also used purely locally); this adds the multiplayer bookkeeping.
// Plain data class -- FirestoreJson maps this to/from Firestore's REST document
// format, since we talk to Firestore over plain HTTP (see CorrespondenceGameRepository)
// rather than the native SDK, which doesn't support WebGL.
public class GameDoc
{
    public List<string> players = new();
    public List<string> playerNames = new();
    public int boardSize;
    public List<string> backRowPrefabNames = new();
    public int currentTurnIndex;
    public string status = "active";
    public int winnerIndex = -1;
    // Only meaningful for the very first sync (the joiner has no board of their own yet);
    // after that, moves propagate via lastMove instead of resending the whole board.
    public List<PieceStateDto> pieces = new();
    public LastMoveDto lastMove = new();
}
