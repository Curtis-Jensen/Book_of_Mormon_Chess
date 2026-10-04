using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

// Shown once, right after a brand-new account's first login -- LoginMenu checks for an
// existing username and routes here instead of straight to the Online Panel when there
// isn't one yet. This is what other players actually see/search by; the account's email
// never appears anywhere but the login form itself.
public class UsernameSetupMenu : MonoBehaviour
{
    static readonly Regex ValidUsername = new(@"^[A-Za-z0-9_]{3,20}$");

    public GameObject onlinePanel;
    public TMP_InputField usernameInput;
    public TMP_Text statusText;

    public void OnClaimClicked()
    {
        var username = usernameInput != null ? usernameInput.text.Trim() : "";

        if (!ValidUsername.IsMatch(username))
        {
            SetStatus("Usernames are 3-20 letters, numbers, or underscores.");
            return;
        }

        SetStatus("Claiming username...");
        CorrespondenceGameRepository.Instance.ClaimUsername(username,
            onSuccess: () =>
            {
                gameObject.SetActive(false);
                onlinePanel.SetActive(true);
            },
            onError: SetStatus);
    }

    void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
