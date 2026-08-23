using TMPro;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

// Minimal correspondence lobby: one auto-generated, editable room code and one
// button. Playing with the generated code creates a new room; editing it to
// match a code someone shared with you joins their room instead.
public class CorrespondenceMenu : MonoBehaviour
{
    const string CodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"; // no 0/O/1/I/L -- easy to read aloud/type

    [Header("Reuses the existing SceneLoader config for a plain 2-player Duel")]
    public SceneLoader sceneLoader;
    public string duelConfigDropdownName = "All Units";

    public TMP_InputField roomCodeInput;
    public TMP_Text statusText;

    // Guards against clicking Play before anonymous sign-in actually finishes -- without
    // this, PlayGame's Firestore request goes out with no valid auth token yet and gets
    // rejected with a 403, which looks like "can't reach that room" but is really just
    // a click-before-ready race.
    bool isReady;

    void Start()
    {
        if (roomCodeInput != null) roomCodeInput.text = GenerateRoomCode();

        SetStatus("Connecting...");
        CorrespondenceGameRepository.Instance.Initialize(
            onReady: () => { isReady = true; SetStatus("Ready."); },
            onError: message => SetStatus("Couldn't connect: " + message));
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void CopyToClipboard(string text);

    [DllImport("__Internal")]
    static extern void FocusPasteCatcher(string targetObjectName);
#endif

    public void OnCopyCodeClicked()
    {
        if (roomCodeInput == null || string.IsNullOrWhiteSpace(roomCodeInput.text))
            return;

        var code = roomCodeInput.text.Trim().ToUpperInvariant();

#if UNITY_WEBGL && !UNITY_EDITOR
        CopyToClipboard(code);
#else
        GUIUtility.systemCopyBuffer = code;
#endif

        SetStatus("Code copied!");
    }

    public void OnPasteCodeClicked()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Focuses a hidden native input and waits for the browser's own paste
        // event -- result comes back later via OnClipboardPasted/OnClipboardPasteFailed.
        FocusPasteCatcher(gameObject.name);
        SetStatus("Press Ctrl+V to paste...");
#else
        if (roomCodeInput != null) roomCodeInput.text = GUIUtility.systemCopyBuffer;
        SetStatus("Code pasted.");
#endif
    }

    // Called by ClipboardCopy.jslib via SendMessage once the browser's native paste event fires.
    void OnClipboardPasted(string text)
    {
        if (roomCodeInput != null) roomCodeInput.text = text.Trim().ToUpperInvariant();
        SetStatus("Code pasted.");
    }

    void OnClipboardPasteFailed(string _)
    {
        SetStatus("Couldn't read what was pasted -- try typing it in.");
    }

    public void OnPlayClicked()
    {
        if (!isReady)
        {
            SetStatus("Still connecting -- try again in a moment.");
            return;
        }

        if (roomCodeInput == null || string.IsNullOrWhiteSpace(roomCodeInput.text))
        {
            SetStatus("Enter a room code first.");
            return;
        }

        var code = roomCodeInput.text.Trim().ToUpperInvariant();
        SetStatus("Connecting to room...");

        CorrespondenceGameRepository.Instance.PlayGame(
            code,
            onReady: (localPlayerIndex, isCreator, boardSize) =>
                StartCorrespondenceScene(code, localPlayerIndex, isCreator, boardSize),
            onError: message => SetStatus(message));
    }

    static string GenerateRoomCode()
    {
        var chars = new char[5];
        for (int i = 0; i < chars.Length; i++)
            chars[i] = CodeAlphabet[Random.Range(0, CodeAlphabet.Length)];

        return new string(chars);
    }

    void StartCorrespondenceScene(string gameId, int localPlayerIndex, bool isCreator, int boardSize)
    {
        var config = System.Array.Find(sceneLoader.sceneConfigs, c => c.dropDownOptionName == duelConfigDropdownName);
        if (config == null)
        {
            SetStatus($"Couldn't find a SceneConfig named '{duelConfigDropdownName}' on the SceneLoader.");
            return;
        }

        // Both devices must build an identically-sized tile grid, so the joiner adopts
        // whatever board size the room was created with instead of its own local setting.
        PlayerPrefs.SetInt("boardSize", boardSize);

        PlayerPrefs.SetInt("4playerMode", 0);

        PlayerPrefs.SetInt("correspondenceMode", 1);
        PlayerPrefs.SetString("correspondenceGameId", gameId);
        PlayerPrefs.SetInt("correspondenceLocalPlayerIndex", localPlayerIndex);
        PlayerPrefs.SetInt("correspondenceIsCreator", isCreator ? 1 : 0);

        // Reuses SceneLoader's existing backRowCount/backRowPrefab_N PlayerPrefs handoff,
        // so BoardSetup spawns the identical piece set it always would for "All Units".
        sceneLoader.LoadWithConfig(config, isCorrespondence: true);
    }

    void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
        Debug.Log("CorrespondenceMenu: " + message);
    }
}
