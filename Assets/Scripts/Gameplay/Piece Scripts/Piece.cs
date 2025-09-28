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
        //Mostly exists for pawn promotion override
    }

    private Coroutine danceCoroutine;
    public void Dance()
    {
        danceCoroutine = StartCoroutine(DanceAnimation());
    }

    private IEnumerator DanceAnimation()
    {
        Vector3 startPosition = transform.position;
        
        while (true)
        {
            // Bounce up
            float elapsedTime = 0f;
            while (elapsedTime < bounceDuration / 2)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / (bounceDuration / 2);
                float height = Mathf.Sin(progress * Mathf.PI) * bounceHeight;
                transform.position = startPosition + new Vector3(0, height, 0);
                yield return null;
            }

            // Bounce down
            elapsedTime = 0f;
            while (elapsedTime < bounceDuration / 2)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / (bounceDuration / 2);
                float height = Mathf.Sin((1 - progress) * Mathf.PI) * bounceHeight;
                transform.position = startPosition + new Vector3(0, height, 0);
                yield return null;
            }

            transform.position = startPosition;
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
