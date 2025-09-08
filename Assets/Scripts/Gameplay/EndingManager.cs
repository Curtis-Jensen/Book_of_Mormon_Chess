using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Hard coded for The Nephites’ Last Stand at the moment
public class EndingManager : MonoBehaviour
{
    public GameObject winScreen;

    int[] teamCounts;

    void Awake()
    {
        teamCounts = new int[2];
        for (int i = 0; i < teamCounts.Length; i++)
        {
            teamCounts[i] = 0;
        }
    }

    public void ReportSpawn(int playerIndex, int materialValue)
    {
        teamCounts[playerIndex] += materialValue;
    }

    public void ReportDeath(int playerIndex, int materialValue)
    {
        teamCounts[playerIndex] -= materialValue;

        if (teamCounts[playerIndex] <= 0 && playerIndex == 0)
        {
            Debug.Log($"Player 1 has lost all their pieces and thus lost the game!");
            winScreen.SetActive(true);
        }
    }
}
