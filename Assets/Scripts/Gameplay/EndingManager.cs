using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : HoardEndingManager
{
    [SerializeField] PieceSets pieceSets;
    [SerializeField] Slider[] materialSliders = new Slider[2];
    [SerializeField] TextMeshProUGUI materialCountText;
    [SerializeField] Color tieColor;

    [Header("Victory screen")]
    [SerializeField] TMP_Text winnerText;
    [SerializeField] TMP_Text unlockText;
    [SerializeField] GameObject playAgainButton;
    [SerializeField] GameObject nextChapterButton;

    string unlockedChapter;

    int[] factionCounts;

    // Each side's representative piece color (see InitializeSideColors), reused for
    // both the sliders' fill and the material-lead number -- null entries mean that
    // side's color couldn't be resolved (missing PieceSets/player), so callers fall
    // back to the tie color rather than showing black/default.
    Color?[] sideColors;

    void Awake()
    {
        pieceSpawner = FindAnyObjectByType<PieceSpawner>();

        factionCounts = new int[2];
        for (int i = 0; i < factionCounts.Length; i++)
        {
            factionCounts[i] = 0;
        }

        InitializeSideColors();
    }

    // The sliders (and the material-lead number) used to have their colors hardcoded
    // in the scene/Inspector -- always wrong whenever a player picked a different color
    // in Settings. Side 0 is players 0 (and 2, in 4-player mode) by the same
    // playerIndex % 2 grouping UpdateMaterial/BoardSetup use elsewhere; teammates could
    // in theory pick different colors from each other, so this just takes the
    // lower-index player on each side as that side's representative color.
    void InitializeSideColors()
    {
        sideColors = new Color?[materialSliders.Length];

        if (pieceSets == null || pieceSpawner == null || pieceSpawner.players == null) return;

        for (int side = 0; side < materialSliders.Length && side < pieceSpawner.players.Length; side++)
        {
            var player = pieceSpawner.players[side];
            var colorIndex = PlayerPrefs.GetInt(player.name + "color");
            if (colorIndex < 0 || colorIndex >= pieceSets.colorSets.Length) continue;

            var teamColor = pieceSets.colorSets[colorIndex].baseColor;
            sideColors[side] = teamColor;

            if (materialSliders[side] != null && materialSliders[side].fillRect != null)
            {
                var fillImage = materialSliders[side].fillRect.GetComponent<Image>();
                if (fillImage != null) fillImage.color = teamColor;
            }
        }
    }

    public void UpdateMaterial(int playerIndex, int materialValue)
    {
        var factionIndex = playerIndex % 2;
        factionCounts[factionIndex] += materialValue;

        //Color changes based on who is winning -- matches that side's actual piece
        //color (same lookup as the sliders) instead of a fixed win/lose color, so the
        //number always agrees with whichever color is actually ahead on the board.
        if (factionCounts[0] == factionCounts[1])
        {
            materialCountText.color = tieColor;
        }
        else
        {
            var leadingSide = factionCounts[0] > factionCounts[1] ? 0 : 1;
            materialCountText.color = sideColors[leadingSide] ?? tieColor;
        }

        //Update sliders
        UpdateSliders(factionIndex);

        //Update text
        materialCountText.text = (factionCounts[0] - factionCounts[1]).ToString();
    }

        void UpdateSliders(int factionIndex)
    {
        if (factionCounts[factionIndex] > materialSliders[factionIndex].maxValue)
        {
            materialSliders[factionIndex].maxValue = factionCounts[factionIndex];
        }

        materialSliders[factionIndex].value = factionCounts[factionIndex];
    }

    public override void CheckEnd()
    {
        for (int i = 0; i < pieceSpawner.players.Length; i++)
        {
            if (CheckNoMoves(i))
            {
                EndGame(i);
            }
        }
    }

    public override void EndGame(int playerIndex)
    {
        base.EndGame(playerIndex);

        int winningPlayerIndex;
        if (playerIndex == 0)
        {
            winningPlayerIndex = 1;
        }
        else
        {
            winningPlayerIndex = 0;
        }

        SetFlagColor(winningPlayerIndex);
        CreateParty(winningPlayerIndex);

        bool online = PlayerPrefs.GetInt("correspondenceMode") == 1;

        // Only a human beating the board unlocks the next training chapter
        unlockedChapter = null;
        if (!pieceSpawner.players[winningPlayerIndex].isAi && !online)
            unlockedChapter = ChapterProgress.MarkCurrentChapterWon();

        ShowVictoryDetails(winningPlayerIndex, online);
    }

    void ShowVictoryDetails(int winningPlayerIndex, bool online)
    {
        if (winnerText != null)
            winnerText.text = winningPlayerIndex % 2 == 0 ? "Nephites win!" : "Lamanites win!";

        if (unlockText != null)
        {
            unlockText.gameObject.SetActive(unlockedChapter != null);
            unlockText.text = $"{unlockedChapter} unlocked";
        }

        // Online games can't be restarted from here, and a chapter only offers
        // "Next" once it's actually unlocked
        if (playAgainButton != null) playAgainButton.SetActive(!online);

        string next = ChapterProgress.NextOf(ChapterProgress.CurrentChapter);
        if (nextChapterButton != null)
            nextChapterButton.SetActive(!online && next != null && ChapterProgress.IsUnlocked(next));
    }

    public void PlayAgain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void NextChapter()
    {
        string next = ChapterProgress.NextOf(ChapterProgress.CurrentChapter);

        // Falls back to the menu if this save predates chapter setups being stored
        if (next == null || !ChapterProgress.TryLaunch(next))
            UnityEngine.SceneManagement.SceneManager.LoadScene("BOM Main Menu");
    }

    void SetFlagColor(int winningPlayerIndex)
    {
        var winningPlayerName = pieceSpawner.players[winningPlayerIndex].name;
        var colorSelection = PlayerPrefs.GetInt(winningPlayerName + "color");//🎨

        // The flag's UI/FlagWave shader multiplies by this tint and applies its own _Alpha (0.65),
        // so force the tint opaque to keep the flag's transparency consistent across color sets.
        var tint = pieceSets.colorSets[colorSelection].baseColor;
        tint.a = 1f;
        winScreen.GetComponent<Image>().color = tint;
    }

    void CreateParty(int winningPlayerIndex)
    {
        foreach (Piece piece in pieceSpawner.players[winningPlayerIndex].pieces)
        {
            piece.GetComponent<Piece>().Dance();
        }
    }
}
