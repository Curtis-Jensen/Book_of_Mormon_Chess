using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Talks to Firestore and Firebase Auth over plain REST/HTTP (via UnityWebRequest)
// instead of the native Firebase SDK, which has no WebGL support -- see
// https://firebase.google.com/docs/firestore/use-rest-api. This is why there's
// no live listener: REST has no push, so callers poll FetchGame instead.
public class CorrespondenceGameRepository : MonoBehaviour
{
    public static CorrespondenceGameRepository Instance { get; private set; }

    [Tooltip("Firebase project's Web API Key (Project Settings -> General). Safe to be client-visible -- access is enforced by Firestore security rules, not by hiding this key.")]
    [SerializeField] string webApiKey;
    [SerializeField] string projectId = "book-of-mormon-chess";

    string idToken;
    string localUserId;

    public string LocalUserId => localUserId;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    const string RefreshTokenPrefKey = "correspondenceRefreshToken";

    public void Initialize(Action onReady, Action<string> onError)
    {
        var storedRefreshToken = PlayerPrefs.GetString(RefreshTokenPrefKey, "");
        if (!string.IsNullOrEmpty(storedRefreshToken))
        {
            StartCoroutine(RefreshSignInRoutine(storedRefreshToken, onReady, onError));
        }
        else
        {
            StartCoroutine(SignUpAnonymouslyRoutine(onReady, onError));
        }
    }

    // Creates a brand-new anonymous identity. Only used the very first time this
    // device plays -- afterwards we persist the refresh token and reuse the same
    // identity (see RefreshSignInRoutine), otherwise every session would look like
    // a different stranger to Firestore and "rejoining" your own room would fail.
    IEnumerator SignUpAnonymouslyRoutine(Action onReady, Action<string> onError)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={webApiKey}";

        using var request = BuildRequest(url, "POST", "{\"returnSecureToken\":true}", withAuth: false);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"Anonymous sign-in failed: {request.error} | {request.downloadHandler.text}");
            yield break;
        }

        var response = JObject.Parse(request.downloadHandler.text);
        idToken = response["idToken"]?.ToString();
        localUserId = response["localId"]?.ToString();
        StoreRefreshToken(response["refreshToken"]?.ToString());

        onReady?.Invoke();
    }

    // Exchanges a previously-stored refresh token for a fresh ID token, keeping the
    // same underlying identity (and thus the same seat) across app restarts.
    IEnumerator RefreshSignInRoutine(string refreshToken, Action onReady, Action<string> onError)
    {
        var url = $"https://securetoken.googleapis.com/v1/token?key={webApiKey}";
        var formBody = $"grant_type=refresh_token&refresh_token={UnityWebRequest.EscapeURL(refreshToken)}";

        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(formBody));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            // Stored token might be stale/revoked -- fall back to a fresh identity rather than getting stuck.
            Debug.LogWarning("Refresh sign-in failed, falling back to a new anonymous identity: " + request.error);
            yield return SignUpAnonymouslyRoutine(onReady, onError);
            yield break;
        }

        var response = JObject.Parse(request.downloadHandler.text);
        idToken = response["id_token"]?.ToString();
        localUserId = response["user_id"]?.ToString();
        StoreRefreshToken(response["refresh_token"]?.ToString());

        onReady?.Invoke();
    }

    void StoreRefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken)) return;
        PlayerPrefs.SetString(RefreshTokenPrefKey, refreshToken);
        // WebGL specifically has no reliable "save on quit" moment when a browser tab
        // is just closed -- without an explicit Save(), this can live only in memory
        // for the page session and vanish the instant the tab closes.
        PlayerPrefs.Save();
    }

    // One button, one code: if the room doesn't exist yet we create it (caller becomes
    // player 0), otherwise we join the open seat (player 1) -- or rejoin our own seat if
    // this device already occupies one. onReady gets (localPlayerIndex, isCreator, boardSize)
    // -- boardSize must be applied (PlayerPrefs "boardSize") *before* the Duel Scene loads and
    // builds its tile grid, so both devices build an identically-sized board from the start
    // rather than the joiner's own local setting silently disagreeing with the creator's.
    public void PlayGame(string code, Action<int, bool, int> onReady, Action<string> onError)
    {
        StartCoroutine(PlayGameRoutine(code, onReady, onError));
    }

    IEnumerator PlayGameRoutine(string code, Action<int, bool, int> onReady, Action<string> onError)
    {
        using var getRequest = BuildRequest(DocumentUrl(code), "GET", null);
        yield return getRequest.SendWebRequest();

        if (getRequest.responseCode == 404)
        {
            var newGame = new GameDoc
            {
                players = new() { localUserId, "" },
                boardSize = PlayerPrefs.GetInt("boardSize", 7),
                currentTurnIndex = 0,
                status = "active"
            };

            using var createRequest = BuildRequest(DocumentUrl(code), "PATCH", FirestoreJson.ToDocumentJson(newGame));
            yield return createRequest.SendWebRequest();

            if (createRequest.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke("Couldn't create room: " + createRequest.error);
                yield break;
            }

            onReady?.Invoke(0, true, newGame.boardSize);
            yield break;
        }

        if (getRequest.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't reach that room: " + getRequest.error);
            yield break;
        }

        var game = FirestoreJson.FromDocumentJson(getRequest.downloadHandler.text);
        if (game.players.Count < 2) game.players.Add("");

        if (game.players[0] == localUserId) { onReady?.Invoke(0, true, game.boardSize); yield break; }
        if (game.players[1] == localUserId) { onReady?.Invoke(1, false, game.boardSize); yield break; }

        if (!string.IsNullOrEmpty(game.players[1]))
        {
            onError?.Invoke("That room is already full.");
            yield break;
        }

        game.players[1] = localUserId;

        using var joinRequest = BuildRequest(DocumentUrl(code), "PATCH", FirestoreJson.ToDocumentJson(game));
        yield return joinRequest.SendWebRequest();

        if (joinRequest.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't join that room: " + joinRequest.error);
            yield break;
        }

        onReady?.Invoke(1, false, game.boardSize);
    }

    const int PushStateMaxAttempts = 3;

    // A failed push used to just log to Debug.LogError and vanish -- invisible to a
    // real player in a shipped build, and with no retry, a single flaky request could
    // silently lose a move: the local player sees their move animate fine and has no
    // idea the opponent never received it. Retries a couple of times with backoff
    // before giving up, and always reports back which one happened so a caller can
    // show something on-screen instead of leaving the player in the dark.
    public void PushState(string gameId, GameDoc state, Action onSuccess = null, Action<string> onFailure = null)
    {
        StartCoroutine(PushStateRoutine(gameId, state, onSuccess, onFailure));
    }

    IEnumerator PushStateRoutine(string gameId, GameDoc state, Action onSuccess, Action<string> onFailure)
    {
        var body = FirestoreJson.ToDocumentJson(state);
        string lastError = null;

        for (int attempt = 1; attempt <= PushStateMaxAttempts; attempt++)
        {
            using var request = BuildRequest(DocumentUrl(gameId), "PATCH", body);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
                yield break;
            }

            lastError = request.error;
            Debug.LogWarning($"PushState attempt {attempt}/{PushStateMaxAttempts} failed: {lastError}");

            if (attempt < PushStateMaxAttempts)
                yield return new WaitForSeconds(attempt * 1.5f);
        }

        Debug.LogError("PushState failed after retries: " + lastError);
        onFailure?.Invoke(lastError);
    }

    // No live listener over REST -- callers poll this (e.g. when opening a game, or on a timer).
    public void FetchGame(string gameId, Action<GameDoc> onFetched, Action<string> onError)
    {
        StartCoroutine(FetchGameRoutine(gameId, onFetched, onError));
    }

    IEnumerator FetchGameRoutine(string gameId, Action<GameDoc> onFetched, Action<string> onError)
    {
        using var request = BuildRequest(DocumentUrl(gameId), "GET", null);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("FetchGame failed: " + request.error);
            yield break;
        }

        onFetched?.Invoke(FirestoreJson.FromDocumentJson(request.downloadHandler.text));
    }

    string CollectionUrl => $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/games";
    string DocumentUrl(string gameId) => $"{CollectionUrl}/{gameId}";

    UnityWebRequest BuildRequest(string url, string method, string jsonBody, bool withAuth = true)
    {
        var request = new UnityWebRequest(url, method);
        if (jsonBody != null)
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        if (withAuth) request.SetRequestHeader("Authorization", "Bearer " + idToken);

        return request;
    }
}
