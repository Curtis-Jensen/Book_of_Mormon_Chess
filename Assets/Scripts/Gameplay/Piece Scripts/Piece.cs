using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Faction
{
    Nephite,
    Lamanite,
    Inanimate
}


[RequireComponent(typeof(SpriteRenderer))]
public abstract class Piece : MonoBehaviour
{
    [Tooltip("Variable to keep track of affiliation")]
    public Faction faction;
    [Tooltip("Represents how valuable this piece is")]
    public int materialValue;
    public GameObject destroyParticlesPrefab;
    public GameObject ghost;

    public int playerIndex;

    [Header("Dance Properties")]
    [Tooltip("How high the piece bounces in units")]
    public float bounceHeight = 0.5f;
    [Tooltip("How long each bounce cycle takes in seconds")]
    public float bounceDuration = 0.5f;
    [Tooltip("Maximum random delay added between bounces (in seconds)")]
    public float maxBounceDelay = 0.3f;

    [HideInInspector]
    public bool firstTurnTaken = false;

    protected HoardEndingManager hoardEndingManager;

    [HideInInspector]
    public int boardSize;

    protected virtual void Start()
    {
        hoardEndingManager = FindAnyObjectByType<HoardEndingManager>();
        hoardEndingManager.ReportSpawn(playerIndex, materialValue);
    }

    /// <summary>
    /// Abstract method for movement logic
    /// </summary>
    /// <returns></returns>
    public abstract List<Vector2Int> GetMoves();

    public bool IsTileEmpty(Vector2Int position)
    {
        //Check for out of bounds
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSize) return false;

        Tile tile = TurnManager.Instance.tiles[position.x, position.y];
        return tile.piece == null;
    }

    /// <summary>
    /// Check if a tile contains an enemy piece
    /// </summary>
    /// <param name="position"></param>
    /// <param name="isLight"></param>
    /// <returns></returns>
    public bool IsEnemyPiece(Vector2Int position)
    {
        //Check for out of bounds
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSize) return false;

        Tile tile = TurnManager.Instance.tiles[position.x, position.y];

        if (tile.piece == null) return false;
        else if (tile.piece.faction == Faction.Inanimate) return false;
        else
        return tile.piece.faction != faction;
    }

    public virtual void MoveEnd()
    {
        //Mostly exists for pawn promotion override, but we can't make it abstract because then every piece would need to implement it
    }

    public void Dance()
    {
        StartCoroutine(DanceAnimation());
    }

    private IEnumerator DanceAnimation()
    {
        Vector3 startPosition = transform.position;
        bool firstBounce = true;
        
        while (true)
        {
            // Complete bounce cycle
            float elapsedTime = 0f;
            float fullDuration = bounceDuration;
            
            while (elapsedTime < fullDuration)
            {
                float normalizedTime = elapsedTime / fullDuration;
                // Full sine wave from 0 to PI for smooth up and down motion
                float height = Mathf.Sin(normalizedTime * Mathf.PI) * bounceHeight;
                transform.position = startPosition + new Vector3(0, height, 0);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure we end at the start position and stay there during delay
            transform.position = startPosition;
            
            // Add delay after bounce (except for first bounce)
            if (!firstBounce && maxBounceDelay > 0)
            {
                float waitDuration = Random.Range(0f, maxBounceDelay);
                float waitedTime = 0f;
                
                while (waitedTime < waitDuration)
                {
                    waitedTime += Time.deltaTime;
                    // Make sure we stay at the ground position during the wait
                    transform.position = startPosition;
                    yield return null;
                }
            }
            firstBounce = false;
        }
    }

    public virtual void Die()
    {
        InstantiateDeathEffects();

        FindAnyObjectByType<PieceSpawner>().players[playerIndex].pieces.Remove(this);
        
        hoardEndingManager.ReportDeath(playerIndex, materialValue);

        Destroy(gameObject);
    }

    void InstantiateDeathEffects()
    {
        // Spawn particles
        var deathParticles = Instantiate(destroyParticlesPrefab, transform.position, Quaternion.identity);
        var main = deathParticles.GetComponent<ParticleSystem>().main;
        var capturedPieceColor = GetComponent<SpriteRenderer>().color;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(capturedPieceColor.r,
            capturedPieceColor.g, capturedPieceColor.b, 1f));

        //Spawn ghost
        Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        var ghostInstance = Instantiate(ghost, transform.position, randomRotation);

        ghostInstance.GetComponent<SpriteRenderer>().sprite = 
        GetComponent<SpriteRenderer>().sprite;

        ghostInstance.GetComponent<SpriteRenderer>().color = new Color(capturedPieceColor.r,
            capturedPieceColor.g, capturedPieceColor.b, 0.25f);
    }
}
