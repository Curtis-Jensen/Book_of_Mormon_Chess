using TMPro;
using UnityEngine;

// Public "Quick Match" lobby: anyone can open a game here and anyone else can join it
// straight from the list, no code needed. This is the "play with whoever's around right
// now" answer -- Friends is for specific people you want to keep playing.
public class LobbyMenu : MonoBehaviour
{
    public CorrespondenceMenu correspondenceMenu;
    public TMP_Text statusText;
    public Transform listParent;
    public GameRowView rowPrefab;

    // See MyGamesMenu's identical guard: a reentrant Refresh() (panel reopened before a
    // prior fetch lands) must not let the older fetch's callback add its own duplicate rows.
    int refreshToken;

    void OnEnable()
    {
        Refresh();
    }

    public void Open()
    {
        correspondenceMenu.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        correspondenceMenu.gameObject.SetActive(true);
    }

    public void Refresh()
    {
        SetStatus("Loading open games...");
        foreach (Transform child in listParent) Destroy(child.gameObject);

        int thisRefresh = ++refreshToken;

        CorrespondenceGameRepository.Instance.FetchOpenGames(
            onFetched: games =>
            {
                if (thisRefresh != refreshToken) return;

                if (games.Count == 0)
                {
                    SetStatus("No open games right now -- start one below.");
                    return;
                }

                SetStatus("");
                foreach (var game in games) CreateRow(game);
            },
            onError: message => { if (thisRefresh == refreshToken) SetStatus(message); });
    }

    void CreateRow(CorrespondenceGameRepository.OpenGameSummary game)
    {
        var creatorName = game.doc.playerNames.Count > 0 && !string.IsNullOrEmpty(game.doc.playerNames[0])
            ? game.doc.playerNames[0]
            : "Someone";

        var row = Instantiate(rowPrefab, listParent);
        row.Set(creatorName, "Open -- tap to join", () =>
            CorrespondenceGameRepository.Instance.JoinOpenGame(game.gameId,
                onReady: (localIndex, isCreator, boardSize) =>
                    correspondenceMenu.StartCorrespondenceScene(game.gameId, localIndex, isCreator, boardSize),
                onError: SetStatus));
    }

    public void OnQuickMatchClicked()
    {
        SetStatus("Opening a game...");
        CorrespondenceGameRepository.Instance.CreateOpenGame(
            onReady: (gameId, localIndex, isCreator, boardSize) =>
                correspondenceMenu.StartCorrespondenceScene(gameId, localIndex, isCreator, boardSize),
            onError: SetStatus);
    }

    void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
