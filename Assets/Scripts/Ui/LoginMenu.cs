using TMPro;
using UnityEngine;

// Gate in front of the Online Panel: resumes a stored session automatically if
// one exists, otherwise asks for email/password. This replaces the old silent
// per-device anonymous identity -- a real account means the same player's games
// show up in "My Games" no matter which device/browser/build they log in from.
public class LoginMenu : MonoBehaviour
{
    public GameObject onlinePanel;
    public GameObject usernameSetupPanel;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    // TMP_InputField has no built-in Tab-to-next-field behavior for a single-line
    // field (that's only wired up for multiline/TextArea navigation), so Tab does
    // nothing by default here -- handle the two-field case directly instead of
    // pulling in a general-purpose navigation component for just this.
    void Update()
    {
        if (emailInput == null || passwordInput == null || !Input.GetKeyDown(KeyCode.Tab)) return;

        var selected = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject;
        var next = selected == emailInput.gameObject ? passwordInput
            : selected == passwordInput.gameObject ? emailInput
            : null;

        if (next == null) return;

        next.Select();
        next.ActivateInputField();
    }

    void OnEnable()
    {
        SetStatus("Checking for a saved login...");
        CorrespondenceGameRepository.Instance.Initialize(
            onReady: OnAuthenticated,
            onNoStoredSession: () => SetStatus("Log in or sign up to play online."),
            onError: SetStatus);
    }

    public void OnLogInClicked()
    {
        if (!TryGetCredentials(out var email, out var password)) return;

        SetStatus("Logging in...");
        CorrespondenceGameRepository.Instance.SignIn(email, password, OnAuthenticated, SetStatus);
    }

    public void OnSignUpClicked()
    {
        if (!TryGetCredentials(out var email, out var password)) return;

        SetStatus("Creating your account...");
        CorrespondenceGameRepository.Instance.SignUp(email, password, OnAuthenticated, SetStatus);
    }

    // Logged in, but a brand-new account has no username yet -- route to username setup
    // instead of the Online Panel until that's claimed, since usernames are what the
    // friends list and open lobby actually display/search by.
    void OnAuthenticated()
    {
        CorrespondenceGameRepository.Instance.FetchMyUsername(
            onFound: _ => GoToOnlinePanel(),
            onNotSet: () =>
            {
                gameObject.SetActive(false);
                usernameSetupPanel.SetActive(true);
            },
            onError: SetStatus);
    }

    bool TryGetCredentials(out string email, out string password)
    {
        email = emailInput != null ? emailInput.text.Trim() : "";
        password = passwordInput != null ? passwordInput.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("Enter an email and password first.");
            return false;
        }

        return true;
    }

    void GoToOnlinePanel()
    {
        gameObject.SetActive(false);
        onlinePanel.SetActive(true);
    }

    void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
