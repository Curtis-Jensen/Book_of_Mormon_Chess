using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class Piece : MonoBehaviour
{
    [Tooltip("Variable to keep track of \"black\" player or \"white\" player.")]
    public bool teamOne;
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

    public virtual void MoveEnd()
    {
        //Mostly exists for pawn promotion override, but we can't make it virtual because then every piece would need to implement it
    }

    private Coroutine danceCoroutine;
    public void Dance()
    {
        danceCoroutine = StartCoroutine(DanceAnimation());
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

    public void Die()
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
