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

    public void Initialize(Action onReady, Action<string> onError)
    {
        StartCoroutine(SignInAnonymouslyRoutine(onReady, onError));
    }

    IEnumerator SignInAnonymouslyRoutine(Action onReady, Action<string> onError)
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

        onReady?.Invoke();
    }

    public void CreateGame(GameDoc initialState, Action<string> onCreated, Action<string> onError)
    {
        initialState.players = new() { localUserId, "" };
        StartCoroutine(CreateGameRoutine(initialState, onCreated, onError));
    }

    IEnumerator CreateGameRoutine(GameDoc initialState, Action<string> onCreated, Action<string> onError)
    {
        var url = $"{CollectionUrl}";
        using var request = BuildRequest(url, "POST", FirestoreJson.ToDocumentJson(initialState));
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("CreateGame failed: " + request.error);
            yield break;
        }

        onCreated?.Invoke(FirestoreJson.ExtractDocumentId(request.downloadHandler.text));
    }

    public void JoinGame(string gameId, Action<int> onJoined, Action<string> onError)
    {
        StartCoroutine(JoinGameRoutine(gameId, onJoined, onError));
    }

    IEnumerator JoinGameRoutine(string gameId, Action<int> onJoined, Action<string> onError)
    {
        using var getRequest = BuildRequest(DocumentUrl(gameId), "GET", null);
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Game not found: " + gameId);
            yield break;
        }

        var game = FirestoreJson.FromDocumentJson(getRequest.downloadHandler.text);
        if (game.players.Count < 2) game.players.Add("");

        if (!string.IsNullOrEmpty(game.players[1]))
        {
            onError?.Invoke("Game is already full.");
            yield break;
        }

        game.players[1] = localUserId;

        using var patchRequest = BuildRequest(DocumentUrl(gameId), "PATCH", FirestoreJson.ToDocumentJson(game));
        yield return patchRequest.SendWebRequest();

        if (patchRequest.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("JoinGame failed: " + patchRequest.error);
            yield break;
        }

        onJoined?.Invoke(1);
    }

    public void PushState(string gameId, GameDoc state)
    {
        StartCoroutine(PushStateRoutine(gameId, state));
    }

    IEnumerator PushStateRoutine(string gameId, GameDoc state)
    {
        using var request = BuildRequest(DocumentUrl(gameId), "PATCH", FirestoreJson.ToDocumentJson(state));
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError("PushState failed: " + request.error);
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
