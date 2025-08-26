using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingManager : MonoBehaviour
{
    int[] teamCounts;

    void Awake()
    {
        teamCounts = new int[2];
    }

    public void ReportSpawn(int playerIndex, int materialValue)
    {
        teamCounts[playerIndex] += materialValue;

        // if (playerIndex == 0)
        // {
        //     Debug.Log($"Gained a piece worth {materialValue}. New material value: {teamCounts[playerIndex]}");
        // }
    }

    public void ReportDeath(int playerIndex, int materialValue)
    {
        teamCounts[playerIndex] -= materialValue;

        // if (playerIndex == 0)
        // {
        //     Debug.Log($"Lost a piece worth {materialValue}. Remaining material value: {teamCounts[playerIndex]}");
        // }
        if (teamCounts[playerIndex] <= 0)
        {
            Debug.Log($"Player {playerIndex} has lost all their pieces and thus lost the game!");
        }
    }
}
