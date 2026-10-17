using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// Search-by-username friends list: send/accept requests, see each friend's current
// turn status across any game you already share with them, and challenge them
// directly (no room code to paste) if you don't.
public class FriendsMenu : MonoBehaviour
{
    public CorrespondenceMenu correspondenceMenu;
    public TMP_InputField searchInput;
    public TMP_Text statusText;
    public Transform requestsListParent;
    public Transform friendsListParent;
    public RequestRowView requestRowPrefab;

    [Tooltip("Accept incoming friend requests automatically instead of showing Accept/Decline rows.")]
    public bool autoAcceptRequests = true;
    public GameRowView friendRowPrefab;

    // See MyGamesMenu's identical guard: a reentrant Refresh() must not let a stale
    // fetch's callback add duplicate rows after a newer refresh already cleared the list.
    // Two independent lists here, so two independent tokens.
    int requestsRefreshToken;
    int friendsRefreshToken;

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
        SetStatus("");
        RefreshRequests();
        RefreshFriends();
    }

    void RefreshRequests()
    {
        foreach (Transform child in requestsListParent) Destroy(child.gameObject);
        int thisRefresh = ++requestsRefreshToken;

        CorrespondenceGameRepository.Instance.FetchIncomingRequests(
            onFetched: requests =>
            {
                if (thisRefresh != requestsRefreshToken) return;

                // No friend-approval step: people finding each other fast matters more than
                // gatekeeping right now. Accepting writes both sides, so one refresh at the end
                // (not per request) shows the new friends. The Accept/Decline row UI is kept
                // for when approval is switched back on.
                if (autoAcceptRequests)
                {
                    AutoAccept(requests);
                    return;
                }

                foreach (var req in requests)
                {
                    var row = Instantiate(requestRowPrefab, requestsListParent);
                    row.Set(req.fromUsername + " wants to be friends",
                        onAccept: () => CorrespondenceGameRepository.Instance.AcceptFriendRequest(req.fromUid, req.fromUsername, Refresh, SetStatus),
                        onDecline: () => CorrespondenceGameRepository.Instance.DeclineFriendRequest(req.fromUid, Refresh, SetStatus));
                }
            },
            onError: message => { if (thisRefresh == requestsRefreshToken) SetStatus(message); });
    }

    void AutoAccept(List<CorrespondenceGameRepository.IncomingRequest> requests)
    {
        if (requests.Count == 0) return;

        int remaining = requests.Count;
        foreach (var req in requests)
        {
            CorrespondenceGameRepository.Instance.AcceptFriendRequest(req.fromUid, req.fromUsername,
                onSuccess: () => { if (--remaining == 0) RefreshFriends(); },
                onError: SetStatus); // a failed accept stays pending and is retried on the next open
        }
    }

    void RefreshFriends()
    {
        foreach (Transform child in friendsListParent) Destroy(child.gameObject);
        int thisRefresh = ++friendsRefreshToken;

        CorrespondenceGameRepository.Instance.FetchFriends(
            onFetched: friends =>
            {
                if (thisRefresh != friendsRefreshToken) return;

                if (friends.Count == 0)
                {
                    SetStatus("No friends yet -- search a username to add one.");
                    return;
                }

                // Cross-reference existing games so a friend you're already mid-game with
                // shows "Your turn"/"Their turn" instead of a flat "Challenge".
                CorrespondenceGameRepository.Instance.FetchMyGames(
                    onFetched: games =>
                    {
                        if (thisRefresh != friendsRefreshToken) return;
                        foreach (var friend in friends) CreateFriendRow(friend, games);
                    },
                    onError: _ =>
                    {
                        if (thisRefresh != friendsRefreshToken) return;
                        foreach (var friend in friends) CreateFriendRow(friend, new List<CorrespondenceGameRepository.MyGameSummary>());
                    });
            },
            onError: message => { if (thisRefresh == friendsRefreshToken) SetStatus(message); });
    }

    void CreateFriendRow(CorrespondenceGameRepository.Friend friend, List<CorrespondenceGameRepository.MyGameSummary> myGames)
    {
        var localUserId = CorrespondenceGameRepository.Instance.LocalUserId;
        var sharedGame = myGames.FirstOrDefault(g => g.doc.status == "active" && g.doc.players.Contains(friend.uid));

        string status;
        System.Action onClick;

        if (sharedGame.doc != null)
        {
            int localIndex = sharedGame.doc.players.IndexOf(localUserId);
            status = sharedGame.doc.currentTurnIndex == localIndex ? "Your turn" : "Their turn";
            var gameId = sharedGame.gameId;
            var boardSize = sharedGame.doc.boardSize;
            var isCreator = localIndex == 0;
            onClick = () => correspondenceMenu.StartCorrespondenceScene(gameId, localIndex, isCreator, boardSize);
        }
        else
        {
            status = "Challenge";
            var friendUid = friend.uid;
            onClick = () => CorrespondenceGameRepository.Instance.ChallengeFriend(friendUid,
                onReady: (gameId, localIndex, isCreator, boardSize) =>
                    correspondenceMenu.StartCorrespondenceScene(gameId, localIndex, isCreator, boardSize),
                onError: SetStatus);
        }

        var row = Instantiate(friendRowPrefab, friendsListParent);
        row.Set(friend.username, status, onClick);
    }

    // "Play a Friend" on the hub: friends are challenged from this list, so jump to the search box.
    public void FocusSearch()
    {
        if (searchInput != null) searchInput.ActivateInputField();
    }

    public void OnAddFriendClicked()
    {
        var username = searchInput != null ? searchInput.text.Trim() : "";
        if (string.IsNullOrEmpty(username)) { SetStatus("Enter a username first."); return; }

        SetStatus("Searching...");
        CorrespondenceGameRepository.Instance.FindUserByUsername(username,
            onFound: user =>
            {
                if (user.uid == CorrespondenceGameRepository.Instance.LocalUserId)
                {
                    SetStatus("That's you!");
                    return;
                }

                // Send the request (they auto-accept next time they open the hub), and add them to
                // my own list right now so I can hit Challenge without waiting for them.
                CorrespondenceGameRepository.Instance.SendFriendRequest(user.uid,
                    onSuccess: () => CorrespondenceGameRepository.Instance.AddFriendEntry(user.uid, user.username,
                        onSuccess: () => { RefreshFriends(); SetStatus(user.username + " added -- tap their name to challenge."); },
                        onError: SetStatus),
                    onError: SetStatus);
            },
            onNotFound: () => SetStatus("No one with that username."),
            onError: SetStatus);
    }

    void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
