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

    [HideInInspector]
    public bool firstTurnTaken = false;

    protected EndingManager endingManager;
    protected TurnManager turnManager;

    protected virtual void Start()
    {
        endingManager = FindAnyObjectByType<EndingManager>();
        endingManager.ReportSpawn(playerIndex, materialValue);

        turnManager = FindAnyObjectByType<TurnManager>();
    }

    /// <summary>
    /// Abstract method for movement logic
    /// </summary>
    /// <returns></returns>
    public abstract List<Vector2Int> GetMoves();

    public virtual void MoveEnd()
    {
        TargetTiles();
    }

    void TargetTiles()
    {
        var parentTile = turnManager.tiles[(int)transform.position.x, (int)transform.position.y];

        
        foreach (var move in GetMoves())
        {
            parentTile.targetedBy.Add(move);
        }
    }

    public void Die()
    {
        InstantiateDeathEffects();

        FindAnyObjectByType<AiManager>().aiPieces[playerIndex].Remove(this);

        endingManager.ReportDeath(playerIndex, materialValue);

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
