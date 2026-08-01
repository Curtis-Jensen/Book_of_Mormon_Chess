using System.Collections.Generic;

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
    public List<PieceStateDto> pieces = new();
}
