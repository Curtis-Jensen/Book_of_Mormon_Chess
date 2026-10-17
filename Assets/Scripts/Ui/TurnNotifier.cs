using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

// Polls Firestore for correspondence games waiting on the local player and shows the
// count as a red badge on every "Play Online" button. Installs itself at startup and
// survives scene loads, so no scene wiring is needed. Polling (not push) because the
// game talks to Firestore over REST and WebGL has no background push channel.
public class TurnNotifier : MonoBehaviour
{
    const float PollSeconds = 30f;
    const string ButtonLabel = "Play Online";

    public static int WaitingCount { get; private set; }

    int pollToken;
    int lastNotifiedCount;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void RequestBrowserNotificationPermission();
    [DllImport("__Internal")] static extern void ShowBrowserNotification(string title, string body);
#endif

    // Browsers only allow the permission prompt from a user gesture, so any click on a
    // "Play Online" button asks (once -- a no-op after the player answers).
    public static void RequestPermission()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RequestBrowserNotificationPermission();
#endif
    }

    static void ShowBrowserNotificationSafe(int count)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowBrowserNotification("BOM Chess", count == 1 ? "It's your turn!" : count + " games are waiting on you!");
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        var go = new GameObject("TurnNotifier");
        DontDestroyOnLoad(go);
        go.AddComponent<TurnNotifier>();
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        InvokeRepeating(nameof(Poll), 1f, PollSeconds);
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        CancelInvoke();
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        ApplyBadges();
        Poll(); // fresh count right after returning from a game
    }

    void Poll()
    {
        var repo = CorrespondenceGameRepository.Instance;
        if (repo == null || !repo.IsReady) return;

        int token = ++pollToken;
        repo.FetchMyGames(
            onFetched: games =>
            {
                if (token != pollToken) return;
                WaitingCount = games.Count(g => g.IsLocalTurn(repo.LocalUserId));
                // Only pop a browser notification when the count goes up -- not every poll.
                if (WaitingCount > lastNotifiedCount) ShowBrowserNotificationSafe(WaitingCount);
                lastNotifiedCount = WaitingCount;
                ApplyBadges();
            },
            onError: _ => { }); // a failed poll just keeps the last known count
    }

    void ApplyBadges()
    {
        foreach (var label in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (label.text != ButtonLabel) continue;
            var button = label.GetComponentInParent<Button>(true);
            if (button == null) continue;
            if (button.GetComponent<NotificationPermissionHook>() == null)
                button.gameObject.AddComponent<NotificationPermissionHook>();

            var badge = button.transform.Find("TurnBadge");
            if (badge == null)
            {
                if (WaitingCount == 0) continue;
                badge = CreateBadge(button.transform);
            }
            badge.gameObject.SetActive(WaitingCount > 0);
            badge.GetComponentInChildren<TMP_Text>().text = WaitingCount.ToString();
        }
    }

    static Transform CreateBadge(Transform parent)
    {
        var root = new GameObject("TurnBadge", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)root.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(36f, 36f);
        rect.anchoredPosition = new Vector2(-8f, -8f);
        var image = root.GetComponent<Image>();
        image.color = new Color(0.85f, 0.15f, 0.15f);
        image.raycastTarget = false;

        var textGo = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
        var textRect = (RectTransform)textGo.transform;
        textRect.SetParent(rect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 22;
        text.color = Color.white;
        text.raycastTarget = false;
        return root.transform;
    }
}

// Lives on each Play Online button so the click that opens the hub also requests permission.
class NotificationPermissionHook : MonoBehaviour
{
    void Awake() => GetComponent<Button>().onClick.AddListener(TurnNotifier.RequestPermission);
}
