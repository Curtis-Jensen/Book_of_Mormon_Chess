using TMPro;
using UnityEngine;

// Lets a guest attach an email and password to their account so they can log in from
// another browser or device (and don't lose everything if this browser's storage is cleared).
// The account keeps its user id and username, so games and friends carry over.
public class SaveAccountMenu : MonoBehaviour
{
    public GameObject onlinePanel;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    public void Open()
    {
        SetStatus("");
        onlinePanel.SetActive(false);
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        onlinePanel.SetActive(true);
    }

    public void OnSaveClicked()
    {
        var email = emailInput != null ? emailInput.text.Trim() : "";
        var password = passwordInput != null ? passwordInput.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("Enter an email and password first.");
            return;
        }

        SetStatus("Saving...");
        CorrespondenceGameRepository.Instance.LinkEmailPassword(email, password,
            onSuccess: Close,
            onError: SetStatus);
    }

    void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
