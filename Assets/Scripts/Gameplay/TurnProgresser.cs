using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

// 🚨TECH DEBT TODO🚨: This class is doing two jobs -- local turn sequencing (its
// original purpose) and correspondence multiplayer sync orchestration (polling,
// pushing, replaying remote moves, tracking room state -- see the "Correspondence
// sync" region below). Extract the latter into its own script. The real work is
// giving MovePiece/ChangeTurn/OnTileClicked clean hook points for that script to
// plug into (gate input, capture the move that just happened, get notified on
// turn change, trigger a replay) rather than having all of it live inline here.
public class TurnProgresser : MonoBehaviour
{
    public static TurnProgresser Instance { get; private set; }
    public delegate void MoveEndHandler();
    public event MoveEndHandler OnMoveEnd;

    protected virtual void Awake()
    {
        Instance = this;

        // A correspondence game is regular duel chess played across two devices via
        // Firestore instead of hotseat -- see PushMoveUpdate/OnRemoteStateReceived below.
        // These are written by CorrespondenceMenu before this scene loads.
        correspondenceMode = PlayerPrefs.GetInt("correspondenceMode") == 1;
        if (correspondenceMode)
        {
            gameId = PlayerPrefs.GetString("correspondenceGameId");
            localPlayerIndex = PlayerPrefs.GetInt("correspondenceLocalPlayerIndex");
            isGameCreator = PlayerPrefs.GetInt("correspondenceIsCreator") == 1;
            HoardEndingManager.OnGameEnd += OnGameEnded;
        }
    }

    void OnDestroy()
    {
        if (correspondenceMode) HoardEndingManager.OnGameEnd -= OnGameEnded;
    }

    int winnerPlayerIndex = -1;
    void OnGameEnded(int losingPlayerIndex) => winnerPlayerIndex = 1 - losingPlayerIndex;

    [Tooltip("How often to poll Firestore for the opponent's move while waiting on our turn. REST has no live listener, so we ask instead of being told.")]
    public float correspondencePollSeconds = 5f;

    [Tooltip("Optional: only shown when a sync actually fails, so a failed push isn't silently invisible to the player. Stays empty/hidden the rest of the time. Safe to leave unassigned.")]
    public TMP_Text errorText;

    void Start()
    {
        if (correspondenceMode)
        {
            StartCoroutine(PollGameStateRoutine());
        }
    }

    IEnumerator PollGameStateRoutine()
    {
        while (true)
        {
            // Skip while a previous replay is still animating -- otherwise a slow network
            // could let this fire again mid-move and replay the same lastMove twice.
            if (!applyingRemoteState)
            {
                CorrespondenceGameRepository.Instance.FetchGame(
                    gameId,
                    onFetched: game => { ClearError(); OnRemoteStateReceived(game); },
                    onError: message => ShowError("Couldn't check for your opponent's move -- will try again."));
            }

            yield return new WaitForSeconds(correspondencePollSeconds);
        }
    }

    void ShowError(string message)
    {
        if (errorText != null) errorText.text = message;
    }

    void ClearError() => ShowError("");

    public TileSelector[,] tiles;
    public float moveTime = 0.5f;

    [HideInInspector] public int boardSize = 8;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public AiManager aiManager;
    [HideInInspector] public HoardEndingManager endingManager;
    [HideInInspector] public PieceSpawner pieceSpawner;

    [HideInInspector] public bool correspondenceMode;
    [HideInInspector] public string gameId;
    [HideInInspector] public int localPlayerIndex;
    [HideInInspector] public bool isGameCreator;

    GameDoc latestGame;
    bool applyingRemoteState;
    bool hasAppliedInitialState;
    int lastMoveFromX = -1, lastMoveFromY = -1, lastMoveToX = -1, lastMoveToY = -1;

    protected int playerTurn = 0;

    public Piece selectedPiece;

    List<TileSelector> selectedTiles = new();

    /// <summary>
    /// Makes decisions on what to do if the tile is clicked in different states
    /// </summary>
    /// <param name="clickedTile"></param>
    public virtual void OnTileClicked(TileSelector clickedTile)
    {
        if (correspondenceMode && playerTurn != localPlayerIndex) return; // Not this device's turn

        //If the tile is not already selected, deselect other tiles and attempt to select the underlying piece
        if (!clickedTile.selected)
        {
            selectedTiles = DeselectTiles(selectedTiles);
            PreviewPieceMoves(clickedTile.piece);
        }
        //If the tile is selected, that means a piece is selected, so move that piece
        else
        {
            MovePiece(clickedTile.transform.position);
        }
    }

    #region🖱️ Tile Interaction: Selection & Movement

    /// <summary>
    /// Deselects all currently selected tiles.  
    /// Called when another piece is selected or a piecce moves
    /// </summary>
    public List<TileSelector> DeselectTiles(List<TileSelector> selectedTiles)
    {
        foreach (var tile in selectedTiles)
        {
            tile.selected= false;
            tile.Highlight(false);
        }
        selectedTiles.Clear();

        return selectedTiles;
    }

    void PreviewPieceMoves(Piece piece)
    {
        //If no piece is on the tile
        if (piece == null) return;
        //If no piece is on the tile, or it's not that piece's turn
        if (playerTurn != piece.playerIndex) return;

        // Access the move data or any other data from the piece script
        selectedTiles = HilightPossibleTiles(piece.GetMoves(), piece, selectedTiles);
    }

    public List<TileSelector> HilightPossibleTiles(List<Vector2Int> attemptedMoves, Piece selectedPiece, List<TileSelector> selectedTiles)
    {
        var tileUnderPiece =
            tiles[Mathf.RoundToInt(selectedPiece.transform.position.x), Mathf.RoundToInt(selectedPiece.transform.position.y)];

        tileUnderPiece.Highlight(true);

        selectedTiles.Add(tileUnderPiece);
        this.selectedPiece = selectedPiece;

        foreach (Vector2Int move in attemptedMoves)
        {
            // 🟩 Get the tile at the attempted move position
            var possibleTile = tiles[move.x, move.y];

            possibleTile.selected = true;
            possibleTile.Highlight(true);

            selectedTiles.Add(possibleTile);
        }

        return selectedTiles;
    }
    #endregion
	
    public void MovePiece(Vector2 destination)
    {
        // Captured before DeselectPreviousPiece clears the tile/before the lerp moves the piece.
        // Not captured while replaying an opponent's move -- we already know it from the DTO.
        if (correspondenceMode && !applyingRemoteState)
        {
            lastMoveFromX = Mathf.RoundToInt(selectedPiece.transform.position.x);
            lastMoveFromY = Mathf.RoundToInt(selectedPiece.transform.position.y);
            lastMoveToX = (int)destination.x;
            lastMoveToY = (int)destination.y;
        }

        selectedTiles = DeselectTiles(selectedTiles);
        DeselectPreviousPiece();
        StartCoroutine(PhysicallyMovePiece(selectedPiece.gameObject, destination, selectedPiece));
    }

    #region ➡Moving sub-methods
    /// <summary>
    /// Handles the transition of the piece to the new tile by clearing old state and preparing the destination tile
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="destinationTile"></param>
    void DeselectPreviousPiece()
    {
        // 🚫👪 Orphan the piece from the tile script so en passants aren't eternal
        var piecePosition = selectedPiece.transform.position;
        TileSelector startingTile = tiles[Mathf.RoundToInt(piecePosition.x), Mathf.RoundToInt(piecePosition.y)];

        if(selectedPiece == null)
        {
            Debug.LogError("selectedPiece is null");
        }

        if(selectedPiece.gameObject == null)
        {
            Debug.LogError("selectedPiece.gameObject is null");
        }

        if (startingTile == null)
        {
            Debug.LogError("startingTile is null");
        }

        if(startingTile.piece == null)
        {
            Debug.LogError("startingTile.piece is null");
        }

        if (startingTile.piece != selectedPiece)
        {
            Debug.LogError
                ($"Expected to deselect {selectedPiece.gameObject.name}" +
                $" but instead almost deselected {startingTile.piece.gameObject.name}");
        }

        startingTile.piece = null;
    }

    protected virtual IEnumerator PhysicallyMovePiece(GameObject piece, Vector2 destination, Piece selectedPiece)
    {
        float time = 0;
        Vector3 startPosition = piece.transform.position;
        Vector3 endPosition = new(destination.x, destination.y, startPosition.z); // Preserve z-axis position
        var destinationTile = tiles[(int)destination.x, (int)destination.y];

        while (time < moveTime)
        {
            piece.transform.position = Vector3.Lerp(startPosition, endPosition, time / moveTime);
            time += Time.deltaTime;
            yield return null;
        }

        //If this is not here the piece will end up slightly off the tile.  Probably due to the lerp above.
        piece.transform.position = endPosition;

        MoveEnd(destinationTile);
    }

    /// <summary>
    /// 👪 Set the piece's new parent to the destination tile both in transform and in script
    /// </summary>
    /// <param name="destinationTile"></param>
    protected void AssignNewParent(TileSelector destinationTile, Piece selectedPiece)
    {
        selectedPiece.transform.SetParent(destinationTile.transform);

        // SetParent's worldPositionStays recomputes the local position via a matrix inverse --
        // over hundreds of reparents that accumulates tiny floating-point error (e.g. 0.9999995
        // instead of 1), which is enough to flip an (int) grid-index cast to the wrong tile.
        // Re-snap to the exact integer coordinate every time to stop that drift from building up.
        selectedPiece.transform.position = destinationTile.transform.position;

        destinationTile.piece = selectedPiece;
    }

    void MoveEnd(TileSelector destinationTile)
    {
        // A move that animated locally (capture effects, etc.) but then throws before
        // reaching ChangeTurn()/PushMoveUpdate() is indistinguishable from "the game
        // just froze" to a real player -- an uncaught exception here can kill whatever
        // coroutine/callback chain was mid-flight and never resume it. Catching it
        // means the move locally completes-or-fails visibly instead of silently
        // vanishing, and ex.ToString() below prints the FULL stack trace as plain log
        // text rather than a collapsed browser error someone has to manually expand.
        try
        {
            if (destinationTile.piece != null)
            {
                destinationTile.piece.Die();
            }

            AssignNewParent(destinationTile, selectedPiece);

            audioSource.Play();

            selectedPiece.MoveEnd();

            OnMoveEnd?.Invoke();

            endingManager.CheckEnd();

            // ChangeTurn() is what pushes the move to Firestore in correspondence mode --
            // it needs to run even on the winning move, or the opponent never finds out
            // the game ended and is left polling forever. Only the local hotseat/AI path
            // (which has nothing to notify) skips it once the game is won.
            if (endingManager.gameOver && !correspondenceMode) return;

            ChangeTurn();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"TurnProgresser.MoveEnd threw partway through a move: {ex}");
            if (correspondenceMode) ShowError("Something went wrong finishing that move -- see the console for details.");
        }
    }

    protected virtual void ChangeTurn()
    {
        // Move to next player
        playerTurn++;
        if (playerTurn >= pieceSpawner.players.Length)
        {
            playerTurn = 0;
        }

        if (correspondenceMode)
        {
            if (!applyingRemoteState) PushMoveUpdate();
            applyingRemoteState = false; // Whether this turn came from a local move or a replayed remote one, it's done now
            return; // No AI opponent in correspondence mode
        }

        if (pieceSpawner.players[playerTurn].isAi)
        {
            AiTurn();
        }
    }

    public void AiTurn()
    {
        var aiChoice = aiManager.ChooseMove(playerTurn);

        selectedPiece = aiChoice.chosenPiece;

        MovePiece(aiChoice.moveTo);
    }

    #region 📡 Correspondence sync

    // Full board snapshot -- only needed once, for the joiner's very first sync
    // (they have no board of their own to diff against yet).
    void PushInitialState()
    {
        if (latestGame == null)
        {
            Debug.LogError("TurnProgresser: tried to push state before the game document loaded.");
            return;
        }

        var boardState = GameStateSerializer.Serialize(this, playerTurn);
        latestGame.boardSize = boardState.boardSize;
        latestGame.currentTurnIndex = boardState.currentTurnIndex;
        latestGame.pieces = boardState.pieces;

        CorrespondenceGameRepository.Instance.PushState(gameId, latestGame,
            onSuccess: ClearError,
            onFailure: error => ShowError("Couldn't reach the server to set up the game. Check your connection."));
    }

    // Every move after the initial handshake: pushes lastMove (replayed through the
    // real pipeline on a client that's already open and in sync, for the animated
    // experience) AND a fresh full snapshot (the actual ground truth). The snapshot
    // matters for correspondence play specifically -- a client that was closed and
    // reopened has no "already in sync" local board to replay a single move onto, so
    // it needs a snapshot that's actually current, not just the opening position.
    void PushMoveUpdate()
    {
        if (latestGame == null)
        {
            Debug.LogError("TurnProgresser: tried to push a move before the game document loaded.");
            return;
        }

        var boardState = GameStateSerializer.Serialize(this, playerTurn);
        latestGame.boardSize = boardState.boardSize;
        latestGame.currentTurnIndex = boardState.currentTurnIndex;
        latestGame.pieces = boardState.pieces;
        latestGame.lastMove = new LastMoveDto
        {
            fromX = lastMoveFromX,
            fromY = lastMoveFromY,
            toX = lastMoveToX,
            toY = lastMoveToY
        };

        if (endingManager.gameOver)
        {
            latestGame.status = "complete";
            latestGame.winnerIndex = winnerPlayerIndex;
        }

        CorrespondenceGameRepository.Instance.PushState(gameId, latestGame,
            onSuccess: ClearError,
            onFailure: error => ShowError("Your move didn't reach the server! Check your connection -- your opponent can't see it yet."));
    }

    void OnRemoteStateReceived(GameDoc game)
    {
        latestGame = game;

        // The creator's own freshly-spawned board IS the initial state; push it
        // once instead of waiting to receive it back (pieces will be empty until then).
        if (isGameCreator && !hasAppliedInitialState && game.pieces.Count == 0)
        {
            hasAppliedInitialState = true;
            playerTurn = 0;
            PushInitialState();
            return;
        }

        if (!hasAppliedInitialState)
        {
            // Joiner's first sync: nothing local exists yet, so this one time we need
            // the full snapshot rather than a move to replay.
            applyingRemoteState = true;
            GameStateSerializer.Apply(
                new GameStateDto { boardSize = game.boardSize, currentTurnIndex = game.currentTurnIndex, pieces = game.pieces },
                this, pieceSpawner);
            playerTurn = game.currentTurnIndex;
            hasAppliedInitialState = true;
            applyingRemoteState = false;
            return;
        }

        if (game.currentTurnIndex == playerTurn) return; // Already in sync, nothing new to replay

        ReplayRemoteMove(game.lastMove);
    }

    // Reuses the real move pipeline (OnTileClicked would normally do this) so captures,
    // promotion, StriplingWarrior's wounding, etc. all run through their actual gameplay
    // logic instead of us reconstructing that state by hand.
    void ReplayRemoteMove(LastMoveDto move)
    {
        if (move == null || move.fromX < 0)
        {
            Debug.LogWarning("TurnProgresser: expected an opponent move to replay but found none.");
            return;
        }

        var fromTile = tiles[move.fromX, move.fromY];
        if (fromTile.piece == null)
        {
            Debug.LogError($"TurnProgresser: no piece found at replay source ({move.fromX},{move.fromY}) -- local board may be out of sync.");
            return;
        }

        applyingRemoteState = true;
        selectedPiece = fromTile.piece;
        MovePiece(new Vector2(move.toX, move.toY));
    }

    #endregion

    void MoveTest()
    {
        if (selectedPiece == null)
        {
            Debug.LogError("SelectedPiece is null!");
        }
    }
    #endregion
}
