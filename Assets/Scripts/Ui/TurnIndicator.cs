using TMPro;
using UnityEngine;

// Online games only: tells the player whether it's their move or they're waiting.
[RequireComponent(typeof(TMP_Text))]
public class TurnIndicator : MonoBehaviour
{
    TMP_Text label;
    HoardEndingManager endingManager;

    void Start()
    {
        label = GetComponent<TMP_Text>();
        endingManager = FindAnyObjectByType<HoardEndingManager>();

        if (PlayerPrefs.GetInt("correspondenceMode") != 1) gameObject.SetActive(false);
    }

    void Update()
    {
        var turns = TurnProgresser.Instance;
        if (turns == null) return;

        if (endingManager != null && endingManager.gameOver) label.text = "";
        else if (!turns.HasInitialState && !turns.isGameCreator) label.text = "Loading game...";
        else if (turns.CurrentTurn == turns.localPlayerIndex) label.text = "Your move";
        else label.text = "Waiting for opponent...";
    }
}
