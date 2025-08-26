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

    [HideInInspector]
    public bool firstTurnTaken = false;
    [HideInInspector]
    public int playerIndex;

    protected EndingManager endingManager;

    protected virtual void Start()
    {
        endingManager = FindAnyObjectByType<EndingManager>();

        endingManager.ReportSpawn(playerIndex, materialValue);
    }
    /// <summary>
    /// Abstract method for movement logic
    /// </summary>
    /// <returns></returns>
    public abstract List<Vector2Int> GetMoves();

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
        Instantiate(destroyParticlesPrefab, transform.position, Quaternion.identity);

        //Spawn ghost
        Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        var ghostInstance = Instantiate(ghost, transform.position, randomRotation);

        ghostInstance.GetComponent<SpriteRenderer>().sprite = 
        GetComponent<SpriteRenderer>().sprite;

        var capturedPieceColor = GetComponent<SpriteRenderer>().color;

        ghostInstance.GetComponent<SpriteRenderer>().color = new Color(capturedPieceColor.r,
            capturedPieceColor.g, capturedPieceColor.b, 0.25f);
    }
}
