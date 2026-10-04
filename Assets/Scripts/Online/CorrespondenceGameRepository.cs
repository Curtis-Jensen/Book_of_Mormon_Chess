using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Talks to Firestore and Firebase Auth over plain REST/HTTP (via UnityWebRequest)
// instead of the native Firebase SDK, which has no WebGL support -- see
// https://firebase.google.com/docs/firestore/use-rest-api. This is why there's
// no live listener: REST has no push, so callers poll FetchGame instead.
// Split across two files (this one: auth + game CRUD; CorrespondenceGameRepository.Social.cs:
// usernames, friends, the open lobby) to keep the file size sane, but it's still one class --
// still the only place allowed to know a Firestore URL.
public partial class CorrespondenceGameRepository : MonoBehaviour
{
    public static CorrespondenceGameRepository Instance { get; private set; }

    [Tooltip("Firebase project's Web API Key (Project Settings -> General). Safe to be client-visible -- access is enforced by Firestore security rules, not by hiding this key.")]
    [SerializeField] string webApiKey;
    [SerializeField] string projectId = "book-of-mormon-chess";

    string idToken;
    string localUserId;

    public string LocalUserId => localUserId;
    public bool IsReady => !string.IsNullOrEmpty(idToken);

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

    // Tries to resume a previously logged-in session via its stored refresh token.
    // Calls onNoStoredSession (not onError) when there's simply nothing to resume --
    // that's the expected first-launch case, not a failure -- so a caller like
    // LoginMenu can show the email/password form instead of an error message.
    public void Initialize(Action onReady, Action onNoStoredSession, Action<string> onError)
    {
        var storedRefreshToken = PlayerPrefs.GetString(RefreshTokenPrefKey, "");
        if (!string.IsNullOrEmpty(storedRefreshToken))
            StartCoroutine(RefreshSignInRoutine(storedRefreshToken, onReady, onNoStoredSession, onError));
        else
            onNoStoredSession?.Invoke();
    }

    // Exchanges a previously-stored refresh token for a fresh ID token, keeping the
    // same underlying identity (and thus the same account/seat) across app restarts.
    IEnumerator RefreshSignInRoutine(string refreshToken, Action onReady, Action onNoStoredSession, Action<string> onError)
    {
        var url = $"https://securetoken.googleapis.com/v1/token?key={webApiKey}";
        var formBody = $"grant_type=refresh_token&refresh_token={UnityWebRequest.EscapeURL(refreshToken)}";

        using var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(formBody));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            // A 400 here means the token itself is bad (expired/revoked/account deleted) --
            // treat that as "no session" so the player's sent back to the login form instead
            // of stuck on a permanent error. Anything else (offline, Firebase hiccup) is a
            // real error worth telling them about instead of silently asking them to log in again.
            if (request.responseCode == 400)
            {
                Debug.LogWarning("Stored session is no longer valid, asking to log in again: " + request.downloadHandler.text);
                PlayerPrefs.DeleteKey(RefreshTokenPrefKey);
                onNoStoredSession?.Invoke();
            }
            else
            {
                onError?.Invoke("Couldn't reconnect: " + request.error);
            }

            yield break;
        }

        var response = JObject.Parse(request.downloadHandler.text);
        idToken = response["id_token"]?.ToString();
        localUserId = response["user_id"]?.ToString();
        StoreRefreshToken(response["refresh_token"]?.ToString());

        onReady?.Invoke();
    }

    // Real accounts (not per-device anonymous identities) so the same player's games
    // show up in "My Games" no matter which device/browser/build they log in from --
    // same Identity Toolkit REST API as the old anonymous sign-up, just with a
    // password instead of an empty body. Still no native Firebase SDK (WebGL-unsafe).
    public void SignUp(string email, string password, Action onReady, Action<string> onError) =>
        StartCoroutine(EmailAuthRoutine("accounts:signUp", email, password, onReady, onError));

    public void SignIn(string email, string password, Action onReady, Action<string> onError) =>
        StartCoroutine(EmailAuthRoutine("accounts:signInWithPassword", email, password, onReady, onError));

    IEnumerator EmailAuthRoutine(string endpoint, string email, string password, Action onReady, Action<string> onError)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/{endpoint}?key={webApiKey}";
        var body = new JObject
        {
            ["email"] = email,
            ["password"] = password,
            ["returnSecureToken"] = true
        }.ToString(Formatting.None);

        using var request = BuildRequest(url, "POST", body, withAuth: false);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            // The on-screen message stays player-friendly ("wrong password"), but the raw
            // response is what you actually need to diagnose e.g. a disabled sign-in
            // provider in the Firebase console (OPERATION_NOT_ALLOWED) -- that one in
            // particular looks identical to any other failure from the UI alone.
            Debug.LogError($"{endpoint} failed ({request.responseCode}): {request.downloadHandler.text}");
            onError?.Invoke(FriendlyAuthError(request.downloadHandler.text));
            yield break;
        }

        var response = JObject.Parse(request.downloadHandler.text);
        idToken = response["idToken"]?.ToString();
        localUserId = response["localId"]?.ToString();
        StoreRefreshToken(response["refreshToken"]?.ToString());

        onReady?.Invoke();
    }

    // Identity Toolkit reports errors as {"error":{"message":"EMAIL_EXISTS", ...}} --
    // translate the handful a player can actually hit into something readable.
    static string FriendlyAuthError(string rawJson)
    {
        string code;
        try { code = JObject.Parse(rawJson)["error"]?["message"]?.ToString() ?? ""; }
        catch { return "Something went wrong -- try again."; }

        if (code.StartsWith("WEAK_PASSWORD")) return "Password needs to be at least 6 characters.";

        return code switch
        {
            "EMAIL_EXISTS" => "An account with that email already exists -- try logging in instead.",
            "EMAIL_NOT_FOUND" => "No account found with that email -- try signing up instead.",
            "INVALID_PASSWORD" => "Wrong password.",
            "INVALID_LOGIN_CREDENTIALS" => "Wrong email or password.",
            "INVALID_EMAIL" => "That doesn't look like a valid email address.",
            "OPERATION_NOT_ALLOWED" => "Email/password sign-in isn't turned on for this project yet -- check the console.",
            _ => "Something went wrong -- try again."
        };
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
                playerNames = new() { localUsername, "" },
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
        if (game.playerNames.Count < 2) game.playerNames.Add("");
        game.playerNames[1] = localUsername;

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

    // One entry per game this account is a player in -- lets a "My Games" screen list
    // every room the player can resume without the player ever having saved its code.
    public struct MyGameSummary
    {
        public string gameId;
        public GameDoc doc;
    }

    public void FetchMyGames(Action<List<MyGameSummary>> onFetched, Action<string> onError)
    {
        StartCoroutine(FetchMyGamesRoutine(onFetched, onError));
    }

    IEnumerator FetchMyGamesRoutine(Action<List<MyGameSummary>> onFetched, Action<string> onError)
    {
        var queryBody = new JObject
        {
            ["structuredQuery"] = new JObject
            {
                ["from"] = new JArray(new JObject { ["collectionId"] = "games" }),
                ["where"] = new JObject
                {
                    ["fieldFilter"] = new JObject
                    {
                        ["field"] = new JObject { ["fieldPath"] = "players" },
                        ["op"] = "ARRAY_CONTAINS",
                        ["value"] = new JObject { ["stringValue"] = localUserId }
                    }
                }
            }
        }.ToString(Formatting.None);

        using var request = BuildRequest(QueryUrl, "POST", queryBody);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't fetch your games: " + request.error);
            yield break;
        }

        var results = new List<MyGameSummary>();
        foreach (var item in JArray.Parse(request.downloadHandler.text))
        {
            // Firestore's runQuery emits one entry per matched document, but can also emit
            // bare "readTime" entries with no document -- skip those.
            if (item["document"] is not JObject document) continue;

            results.Add(new MyGameSummary
            {
                gameId = FirestoreJson.ExtractDocumentId(document),
                doc = FirestoreJson.FromDocument(document)
            });
        }

        onFetched?.Invoke(results);
    }

    string CollectionUrl => $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/games";
    string DocumentUrl(string gameId) => $"{CollectionUrl}/{gameId}";
    string QueryUrl => $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents:runQuery";

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
