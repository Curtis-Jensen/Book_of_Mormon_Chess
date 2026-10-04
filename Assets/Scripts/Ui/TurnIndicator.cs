using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Online games only: tells the player whether it's their move or they're waiting.
[RequireComponent(typeof(TMP_Text))]
public class TurnIndicator : MonoBehaviour
{
    [Tooltip("Background panel behind the label -- tinted to the active player's piece color so the banner reads as 'their turn' at a glance.")]
    [SerializeField] Image background;

    TMP_Text label;
    HoardEndingManager endingManager;

    void Start()
    {
        label = GetComponent<TMP_Text>();
        endingManager = FindAnyObjectByType<HoardEndingManager>();

        if (PlayerPrefs.GetInt("correspondenceMode") != 1)
        {
            gameObject.SetActive(false);
            // background is a sibling, not a child -- hiding this GameObject alone
            // doesn't hide it, so it'd otherwise be left behind as an empty dark box.
            if (background != null) background.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        var turns = TurnProgresser.Instance;
        if (turns == null) return;

        if (endingManager != null && endingManager.gameOver) label.text = "";
        else if (!turns.HasInitialState && !turns.isGameCreator) label.text = "Loading game...";
        else if (turns.CurrentTurn == turns.localPlayerIndex) label.text = "Your move";
        else label.text = "Waiting for opponent...";

        UpdateBackgroundColor(turns);
    }

    // Tints the banner to whichever team is actually on the move, using each player's
    // own chosen piece color (not a hardcoded red/blue) -- same lookup PieceSpawner
    // uses to color the pieces themselves, so the banner always matches the board.
    void UpdateBackgroundColor(TurnProgresser turns)
    {
        if (background == null) return;

        var spawner = turns.pieceSpawner;
        if (spawner == null || spawner.players == null || spawner.pieceSets == null) return;
        if (turns.CurrentTurn < 0 || turns.CurrentTurn >= spawner.players.Length) return;

        var activePlayer = spawner.players[turns.CurrentTurn];
        var colorIndex = PlayerPrefs.GetInt(activePlayer.name + "color");
        if (colorIndex < 0 || colorIndex >= spawner.pieceSets.colorSets.Length) return;

        var teamColor = spawner.pieceSets.colorSets[colorIndex].baseColor;
        background.color = new Color(teamColor.r, teamColor.g, teamColor.b, 0.75f);
    }
}
