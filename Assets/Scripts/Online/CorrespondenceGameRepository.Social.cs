using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Usernames, friends, and the open-game lobby. Split from the main file to keep file
// size sane, but it's the same class -- still the one place allowed to know a
// Firestore URL (see the architecture comment at the top of the other half).
public partial class CorrespondenceGameRepository
{
    string localUsername;
    public string LocalUsername => localUsername;

    // ---- Username ----
    // A username is two documents: usernames/{usernameKey} (the reservation -- what makes
    // the name unique and lets anyone look a username up) and users/{uid}.username (the
    // reverse lookup -- what lets code that already has a uid show a display name).

    public void FetchMyUsername(Action<string> onFound, Action onNotSet, Action<string> onError) =>
        StartCoroutine(FetchMyUsernameRoutine(onFound, onNotSet, onError));

    IEnumerator FetchMyUsernameRoutine(Action<string> onFound, Action onNotSet, Action<string> onError)
    {
        using var request = BuildRequest(UserDocUrl(localUserId), "GET", null);
        yield return request.SendWebRequest();

        if (request.responseCode == 404) { onNotSet?.Invoke(); yield break; }

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't check your account: " + request.error);
            yield break;
        }

        var fields = JObject.Parse(request.downloadHandler.text)["fields"] as JObject;
        var username = fields?["username"]?["stringValue"]?.ToString();
        if (string.IsNullOrEmpty(username)) { onNotSet?.Invoke(); yield break; }

        localUsername = username;
        onFound?.Invoke(username);
    }

    // Reserves the username atomically -- currentDocument.exists=false makes Firestore
    // refuse the write if that doc's already there, which is what turns this into a real
    // uniqueness check instead of a check-then-write race between two players.
    public void ClaimUsername(string username, Action onSuccess, Action<string> onError) =>
        StartCoroutine(ClaimUsernameRoutine(username, onSuccess, onError));

    IEnumerator ClaimUsernameRoutine(string username, Action onSuccess, Action<string> onError)
    {
        var usernameKey = username.Trim().ToLowerInvariant();
        var reserveBody = new JObject
        {
            ["fields"] = new JObject
            {
                ["uid"] = new JObject { ["stringValue"] = localUserId },
                ["username"] = new JObject { ["stringValue"] = username }
            }
        }.ToString(Formatting.None);

        using (var reserveRequest = BuildRequest(UsernameDocUrl(usernameKey) + "?currentDocument.exists=false", "PATCH", reserveBody))
        {
            yield return reserveRequest.SendWebRequest();

            if (reserveRequest.result != UnityWebRequest.Result.Success)
            {
                // The failed-precondition response is what the exists=false check returns
                // when the username's already taken -- everything else is a real error.
                onError?.Invoke(reserveRequest.responseCode == 400 || reserveRequest.responseCode == 409
                    ? "That username is already taken."
                    : "Couldn't claim that username: " + reserveRequest.error);
                yield break;
            }
        }

        var userBody = new JObject
        {
            ["fields"] = new JObject
            {
                ["username"] = new JObject { ["stringValue"] = username },
                ["usernameKey"] = new JObject { ["stringValue"] = usernameKey }
            }
        }.ToString(Formatting.None);

        using var userRequest = BuildRequest(UserDocUrl(localUserId), "PATCH", userBody);
        yield return userRequest.SendWebRequest();

        if (userRequest.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't save your username: " + userRequest.error);
            yield break;
        }

        localUsername = username;
        onSuccess?.Invoke();
    }

    // ---- Friend search / requests ----

    public struct FoundUser { public string uid; public string username; }

    public void FindUserByUsername(string username, Action<FoundUser> onFound, Action onNotFound, Action<string> onError) =>
        StartCoroutine(FindUserByUsernameRoutine(username, onFound, onNotFound, onError));

    IEnumerator FindUserByUsernameRoutine(string username, Action<FoundUser> onFound, Action onNotFound, Action<string> onError)
    {
        var usernameKey = username.Trim().ToLowerInvariant();
        using var request = BuildRequest(UsernameDocUrl(usernameKey), "GET", null);
        yield return request.SendWebRequest();

        if (request.responseCode == 404) { onNotFound?.Invoke(); yield break; }

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Search failed: " + request.error);
            yield break;
        }

        var fields = JObject.Parse(request.downloadHandler.text)["fields"] as JObject;
        onFound?.Invoke(new FoundUser
        {
            uid = fields?["uid"]?["stringValue"]?.ToString(),
            username = fields?["username"]?["stringValue"]?.ToString()
        });
    }

    // A request lives as a doc in the RECIPIENT's own incomingRequests subcollection, so
    // listing pending requests is just a plain GET of your own subcollection -- no
    // cross-user query needed.
    public void SendFriendRequest(string toUid, Action onSuccess, Action<string> onError) =>
        StartCoroutine(SendFriendRequestRoutine(toUid, onSuccess, onError));

    IEnumerator SendFriendRequestRoutine(string toUid, Action onSuccess, Action<string> onError)
    {
        var body = new JObject
        {
            ["fields"] = new JObject
            {
                ["fromUid"] = new JObject { ["stringValue"] = localUserId },
                ["fromUsername"] = new JObject { ["stringValue"] = localUsername }
            }
        }.ToString(Formatting.None);

        using var request = BuildRequest(IncomingRequestDocUrl(toUid, localUserId), "PATCH", body);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't send that request: " + request.error);
            yield break;
        }

        onSuccess?.Invoke();
    }

    public struct IncomingRequest { public string fromUid; public string fromUsername; }

    public void FetchIncomingRequests(Action<List<IncomingRequest>> onFetched, Action<string> onError) =>
        StartCoroutine(FetchIncomingRequestsRoutine(onFetched, onError));

    IEnumerator FetchIncomingRequestsRoutine(Action<List<IncomingRequest>> onFetched, Action<string> onError)
    {
        using var request = BuildRequest(UserDocUrl(localUserId) + "/incomingRequests", "GET", null);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't fetch friend requests: " + request.error);
            yield break;
        }

        var results = new List<IncomingRequest>();
        if (JObject.Parse(request.downloadHandler.text)["documents"] is JArray documents)
        {
            foreach (var doc in documents)
            {
                var fields = doc["fields"] as JObject;
                results.Add(new IncomingRequest
                {
                    fromUid = fields?["fromUid"]?["stringValue"]?.ToString(),
                    fromUsername = fields?["fromUsername"]?["stringValue"]?.ToString()
                });
            }
        }

        onFetched?.Invoke(results);
    }

    public void AcceptFriendRequest(string fromUid, string fromUsername, Action onSuccess, Action<string> onError) =>
        StartCoroutine(AcceptFriendRequestRoutine(fromUid, fromUsername, onSuccess, onError));

    // Friendship is symmetric, so both sides need their own "friends" entry written.
    // Three sequential requests rather than Firestore's batch :commit endpoint -- this is
    // a rare, player-initiated action, not something that needs atomicity against
    // concurrent writers the way a move push does.
    IEnumerator AcceptFriendRequestRoutine(string fromUid, string fromUsername, Action onSuccess, Action<string> onError)
    {
        var myEntryBody = new JObject
        {
            ["fields"] = new JObject { ["username"] = new JObject { ["stringValue"] = fromUsername } }
        }.ToString(Formatting.None);

        using (var request = BuildRequest(UserDocUrl(localUserId) + "/friends/" + fromUid, "PATCH", myEntryBody))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke("Couldn't accept: " + request.error);
                yield break;
            }
        }

        var theirEntryBody = new JObject
        {
            ["fields"] = new JObject { ["username"] = new JObject { ["stringValue"] = localUsername } }
        }.ToString(Formatting.None);

        using (var request = BuildRequest(UserDocUrl(fromUid) + "/friends/" + localUserId, "PATCH", theirEntryBody))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke("Couldn't accept: " + request.error);
                yield break;
            }
        }

        using (var request = BuildRequest(IncomingRequestDocUrl(localUserId, fromUid), "DELETE", null))
        {
            yield return request.SendWebRequest();
            // Not fatal -- the friendship itself already succeeded above, this just tidies up.
            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogWarning("Accepted friend request but couldn't clear it: " + request.error);
        }

        onSuccess?.Invoke();
    }

    public void DeclineFriendRequest(string fromUid, Action onSuccess, Action<string> onError) =>
        StartCoroutine(DeleteDocRoutine(IncomingRequestDocUrl(localUserId, fromUid), onSuccess, onError));

    public struct Friend { public string uid; public string username; }

    public void FetchFriends(Action<List<Friend>> onFetched, Action<string> onError) =>
        StartCoroutine(FetchFriendsRoutine(onFetched, onError));

    IEnumerator FetchFriendsRoutine(Action<List<Friend>> onFetched, Action<string> onError)
    {
        using var request = BuildRequest(UserDocUrl(localUserId) + "/friends", "GET", null);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't fetch friends: " + request.error);
            yield break;
        }

        var results = new List<Friend>();
        if (JObject.Parse(request.downloadHandler.text)["documents"] is JArray documents)
        {
            foreach (JObject doc in documents)
            {
                results.Add(new Friend
                {
                    uid = FirestoreJson.ExtractDocumentId(doc),
                    username = (doc["fields"] as JObject)?["username"]?["stringValue"]?.ToString()
                });
            }
        }

        onFetched?.Invoke(results);
    }

    IEnumerator DeleteDocRoutine(string url, Action onSuccess, Action<string> onError)
    {
        using var request = BuildRequest(url, "DELETE", null);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("That didn't work: " + request.error);
            yield break;
        }

        onSuccess?.Invoke();
    }

    // ---- Challenging a friend directly ----

    // Reuses PlayGame's existing create-or-join logic, aimed at a fixed per-friendship
    // room code (order-independent, so whichever side "creates" it, both land in the
    // same room) instead of a code either side has to type or paste.
    public void ChallengeFriend(string friendUid, Action<string, int, bool, int> onReady, Action<string> onError)
    {
        var challengeCode = ChallengeCodeFor(localUserId, friendUid);
        PlayGame(challengeCode,
            (localPlayerIndex, isCreator, boardSize) => onReady?.Invoke(challengeCode, localPlayerIndex, isCreator, boardSize),
            onError);
    }

    static string ChallengeCodeFor(string uidA, string uidB)
    {
        var ordered = string.CompareOrdinal(uidA, uidB) < 0 ? (uidA, uidB) : (uidB, uidA);
        return "challenge_" + ordered.Item1 + "_" + ordered.Item2;
    }

    // ---- Open lobby ----

    public void CreateOpenGame(Action<string, int, bool, int> onReady, Action<string> onError) =>
        StartCoroutine(CreateOpenGameRoutine(onReady, onError));

    IEnumerator CreateOpenGameRoutine(Action<string, int, bool, int> onReady, Action<string> onError)
    {
        var newGame = new GameDoc
        {
            players = new() { localUserId, "" },
            playerNames = new() { localUsername, "" },
            boardSize = PlayerPrefs.GetInt("boardSize", 7),
            currentTurnIndex = 0,
            status = "active",
            isOpen = true
        };

        using var request = BuildRequest(CollectionUrl, "POST", FirestoreJson.ToDocumentJson(newGame));
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't open a game: " + request.error);
            yield break;
        }

        var gameId = FirestoreJson.ExtractDocumentId(JObject.Parse(request.downloadHandler.text));
        onReady?.Invoke(gameId, 0, true, newGame.boardSize);
    }

    public struct OpenGameSummary { public string gameId; public GameDoc doc; }

    public void FetchOpenGames(Action<List<OpenGameSummary>> onFetched, Action<string> onError) =>
        StartCoroutine(FetchOpenGamesRoutine(onFetched, onError));

    IEnumerator FetchOpenGamesRoutine(Action<List<OpenGameSummary>> onFetched, Action<string> onError)
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
                        ["field"] = new JObject { ["fieldPath"] = "isOpen" },
                        ["op"] = "EQUAL",
                        ["value"] = new JObject { ["booleanValue"] = true }
                    }
                },
                ["limit"] = 25
            }
        }.ToString(Formatting.None);

        using var request = BuildRequest(QueryUrl, "POST", queryBody);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't fetch open games: " + request.error);
            yield break;
        }

        var results = new List<OpenGameSummary>();
        foreach (var item in JArray.Parse(request.downloadHandler.text))
        {
            if (item["document"] is not JObject document) continue;
            var doc = FirestoreJson.FromDocument(document);
            if (doc.players.Count > 0 && doc.players[0] == localUserId) continue; // don't show my own open game to myself

            results.Add(new OpenGameSummary { gameId = FirestoreJson.ExtractDocumentId(document), doc = doc });
        }

        onFetched?.Invoke(results);
    }

    public void JoinOpenGame(string gameId, Action<int, bool, int> onReady, Action<string> onError) =>
        StartCoroutine(JoinOpenGameRoutine(gameId, onReady, onError));

    IEnumerator JoinOpenGameRoutine(string gameId, Action<int, bool, int> onReady, Action<string> onError)
    {
        using var getRequest = BuildRequest(DocumentUrl(gameId), "GET", null);
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("That game's no longer available: " + getRequest.error);
            yield break;
        }

        var game = FirestoreJson.FromDocumentJson(getRequest.downloadHandler.text);
        if (game.players.Count < 2) game.players.Add("");

        if (!string.IsNullOrEmpty(game.players[1]))
        {
            onError?.Invoke("Someone already joined that one -- try another.");
            yield break;
        }

        game.players[1] = localUserId;
        if (game.playerNames.Count < 2) game.playerNames.Add("");
        game.playerNames[1] = localUsername;
        game.isOpen = false;

        using var joinRequest = BuildRequest(DocumentUrl(gameId), "PATCH", FirestoreJson.ToDocumentJson(game));
        yield return joinRequest.SendWebRequest();

        if (joinRequest.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Couldn't join that game: " + joinRequest.error);
            yield break;
        }

        onReady?.Invoke(1, false, game.boardSize);
    }

    string UserDocUrl(string uid) => $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{uid}";
    string UsernameDocUrl(string usernameKey) => $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/usernames/{usernameKey}";
    string IncomingRequestDocUrl(string toUid, string fromUid) => $"{UserDocUrl(toUid)}/incomingRequests/{fromUid}";
}
