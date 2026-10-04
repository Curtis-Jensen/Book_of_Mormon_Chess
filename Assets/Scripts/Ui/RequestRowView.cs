using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One row in the incoming-friend-requests list -- needs two independent actions
// (Accept/Decline), unlike GameRowView's single whole-row click.
public class RequestRowView : MonoBehaviour
{
    public TMP_Text usernameText;
    public Button acceptButton;
    public Button declineButton;

    public void Set(string message, Action onAccept, Action onDecline)
    {
        if (usernameText != null) usernameText.text = message;

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(() => onAccept());

        declineButton.onClick.RemoveAllListeners();
        declineButton.onClick.AddListener(() => onDecline());
    }
}
