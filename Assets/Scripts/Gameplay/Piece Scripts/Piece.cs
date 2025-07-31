using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    [Tooltip("Variable to keep track of \"black\" player or \"white\" player.")]
    public bool teamOne;
    public GameObject destroyParticlesPrefab;
    public GameObject ghost;

    [HideInInspector]
    public bool firstTurnTaken = false;
    public int playerIndex;

    /// <summary>
    /// Abstract method for movement logic
    /// </summary>
    /// <returns></returns>
    public abstract List<Vector2Int> GetMoves();

    public void Die()
    {
        InstantiateDeathEffects();

        AiManager.Instance.aiPieces.Remove(this);
        Destroy(gameObject);
    }

    void InstantiateDeathEffects()
    {
        // Spawn particles
        Instantiate(destroyParticlesPrefab, transform.position, Quaternion.identity, transform.parent);

        //Spawn ghost
        Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        var ghostInstance = Instantiate(ghost, transform.position, randomRotation, transform.parent);

        ghostInstance.GetComponent<SpriteRenderer>().sprite =
                      GetComponent<SpriteRenderer>().sprite;

        var capturedPieceColor = GetComponent<SpriteRenderer>().color;

        ghostInstance.GetComponent<SpriteRenderer>().color = new Color(capturedPieceColor.r,
            capturedPieceColor.g, capturedPieceColor.b, 0.25f);
    }
}
