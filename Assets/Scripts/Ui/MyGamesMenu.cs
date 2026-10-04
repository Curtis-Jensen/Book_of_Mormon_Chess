using TMPro;
using UnityEngine;

// Lists every correspondence game this device's account is a player in, queried
// straight from Firestore by LocalUserId (see CorrespondenceGameRepository.FetchMyGames)
// rather than any locally-remembered code. This is what lets a player resume a game
// without ever having to paste its room code again -- CorrespondenceMenu's code field
// stays reserved for starting a brand-new room or joining someone else's shared code.
public class MyGamesMenu : MonoBehaviour
{
    public CorrespondenceMenu correspondenceMenu;
    public TMP_Text statusText;
    public Transform listParent;
    public GameRowView rowPrefab;

    // Reopening this panel before a prior fetch lands (OnEnable -> Refresh is reentrant)
    // used to leave two sets of rows on screen, since the stale fetch's callback had no
    // way to know a newer one had already cleared and repopulated the list. Each Refresh
    // call stamps its own token; a callback only draws rows if it's still the latest one.
    int refreshToken;

    void OnEnable()
    {
        Refresh();
    }

    // Hooked up to button clicks directly (see CorrespondenceMenu's "My Games" button
    // and this panel's own Back button) so both panels never show at the same time.
    public void Show()
    {
        correspondenceMenu.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void GoBack()
    {
        gameObject.SetActive(false);
        correspondenceMenu.gameObject.SetActive(true);
    }

    public void Refresh()
    {
        // CorrespondenceMenu.Start() kicks off anonymous sign-in, but this panel can
        // become active before that finishes -- retry shortly rather than querying
        // with no auth token yet (which would just 403).
        if (!CorrespondenceGameRepository.Instance.IsReady)
        {
            SetStatus("Connecting...");
            Invoke(nameof(Refresh), 0.5f);
            return;
        }

        SetStatus("Loading your games...");
        foreach (Transform child in listParent) Destroy(child.gameObject);

        int thisRefresh = ++refreshToken;

        CorrespondenceGameRepository.Instance.FetchMyGames(
            onFetched: games =>
            {
                if (thisRefresh != refreshToken) return; // a newer Refresh() superseded this one

                if (games.Count == 0)
                {
                    SetStatus("No games yet -- start one from the lobby.");
                    return;
                }

                SetStatus("");
                foreach (var game in games) CreateRow(game);
            },
            onError: message => { if (thisRefresh == refreshToken) SetStatus(message); });
    }

    void CreateRow(CorrespondenceGameRepository.MyGameSummary game)
    {
        var localUserId = CorrespondenceGameRepository.Instance.LocalUserId;
        int localPlayerIndex = game.doc.players.IndexOf(localUserId);
        if (localPlayerIndex < 0) return; // shouldn't happen -- the query already filtered on this

        bool isCreator = localPlayerIndex == 0;
        bool waitingForOpponent = game.doc.players.Count < 2 || string.IsNullOrEmpty(game.doc.players[1]);

        int opponentIndex = 1 - localPlayerIndex;
        string opponentName = waitingForOpponent
            ? "Waiting for opponent..."
            : (opponentIndex < game.doc.playerNames.Count && !string.IsNullOrEmpty(game.doc.playerNames[opponentIndex])
                ? game.doc.playerNames[opponentIndex]
                : "Opponent");

        string turnStatus = game.doc.status != "active"
            ? (game.doc.winnerIndex == localPlayerIndex ? "You won" : "Game over")
            : waitingForOpponent
                ? "Waiting..."
                : (game.doc.currentTurnIndex == localPlayerIndex ? "Your turn" : "Their turn");

        var row = Instantiate(rowPrefab, listParent);
        row.Set(opponentName, turnStatus, () =>
            correspondenceMenu.StartCorrespondenceScene(game.gameId, localPlayerIndex, isCreator, game.doc.boardSize));
    }

    void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
