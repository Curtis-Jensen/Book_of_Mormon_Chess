using TMPro;
using UnityEngine;

// Minimal correspondence lobby: create a game and get a code to share, or
// join one by typing in a code someone shared with you. No matchmaking,
// no game list yet -- see the plan doc for what's deliberately out of scope.
public class CorrespondenceMenu : MonoBehaviour
{
    [Header("Reuses the existing SceneLoader config for a plain 2-player Duel")]
    public SceneLoader sceneLoader;
    public string duelConfigDropdownName = "All Units";

    [Header("New Game")]
    public TMP_Text gameCodeDisplay;

    [Header("Join Game")]
    public TMP_InputField joinCodeInput;

    [Header("Shared")]
    public TMP_Text statusText;

    string pendingGameId;
    int pendingLocalPlayerIndex;

    void Start()
    {
        SetStatus("Connecting...");
        CorrespondenceGameRepository.Instance.Initialize(
            onReady: () => SetStatus("Ready."),
            onError: message => SetStatus("Couldn't connect: " + message));
    }

    public void OnCreateGameClicked()
    {
        SetStatus("Creating game...");

        var boardSize = PlayerPrefs.GetInt("boardSize", 7);
        var newGame = new GameDoc
        {
            boardSize = boardSize,
            currentTurnIndex = 0,
            status = "active"
        };

        CorrespondenceGameRepository.Instance.CreateGame(
            newGame,
            onCreated: gameId =>
            {
                pendingGameId = gameId;
                pendingLocalPlayerIndex = 0;

                if (gameCodeDisplay != null) gameCodeDisplay.text = gameId;
                SetStatus("Game created! Share this code, then click Continue when you're ready to play.");
            },
            onError: message => SetStatus("Couldn't create game: " + message));
    }

    // Separate from OnCreateGameClicked so there's a chance to actually read/share
    // the code before the panel (and that text) gets destroyed by the scene load.
    public void OnContinueClicked()
    {
        if (string.IsNullOrEmpty(pendingGameId))
        {
            SetStatus("Create or join a game first.");
            return;
        }

        StartCorrespondenceScene(pendingGameId, pendingLocalPlayerIndex, isCreator: pendingLocalPlayerIndex == 0);
    }

    public void OnJoinGameClicked()
    {
        if (joinCodeInput == null || string.IsNullOrWhiteSpace(joinCodeInput.text))
        {
            SetStatus("Enter a game code first.");
            return;
        }

        var gameId = joinCodeInput.text.Trim();
        SetStatus("Joining game...");

        CorrespondenceGameRepository.Instance.JoinGame(
            gameId,
            onJoined: assignedPlayerIndex =>
            {
                pendingGameId = gameId;
                pendingLocalPlayerIndex = assignedPlayerIndex;
                SetStatus("Joined! Click Continue to start playing.");
            },
            onError: message => SetStatus("Couldn't join game: " + message));
    }

    void StartCorrespondenceScene(string gameId, int localPlayerIndex, bool isCreator)
    {
        var config = System.Array.Find(sceneLoader.sceneConfigs, c => c.dropDownOptionName == duelConfigDropdownName);
        if (config == null)
        {
            SetStatus($"Couldn't find a SceneConfig named '{duelConfigDropdownName}' on the SceneLoader.");
            return;
        }

        PlayerPrefs.SetInt("4playerMode", 0);
        PlayerPrefs.SetInt("1isAI", 0);
        PlayerPrefs.SetInt("2isAI", 0);

        PlayerPrefs.SetInt("correspondenceMode", 1);
        PlayerPrefs.SetString("correspondenceGameId", gameId);
        PlayerPrefs.SetInt("correspondenceLocalPlayerIndex", localPlayerIndex);
        PlayerPrefs.SetInt("correspondenceIsCreator", isCreator ? 1 : 0);

        // Reuses SceneLoader's existing backRowCount/backRowPrefab_N PlayerPrefs handoff,
        // so BoardSetup spawns the identical piece set it always would for "All Units".
        sceneLoader.LoadWithConfig(config);
    }

    void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
        Debug.Log("CorrespondenceMenu: " + message);
    }
}
