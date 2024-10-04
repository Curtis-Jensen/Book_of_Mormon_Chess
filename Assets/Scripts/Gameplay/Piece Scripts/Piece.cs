using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    [Tooltip("Variable to keep track of \"black\" player or \"white\" player.")]
    public bool isLight;

    [Tooltip("Particle system to play when the piece is destroyed.")]
    public GameObject destroyParticlesPrefab;

    [HideInInspector]
    public bool firstTurnTaken = false;

    /// <summary>
    /// Abstract method for movement logic
    /// </summary>
    /// <returns></returns>
    public abstract List<Vector2Int> GetMoves();

    void OnDestroy()
    {
        //Set the position the particles need to spawn to be visible
        Vector3 position = new Vector3 (transform.position.x, transform.position.y, -5);
        // Spawn particles
        GameObject particles = Instantiate(destroyParticlesPrefab, position, Quaternion.identity);
    }
}
