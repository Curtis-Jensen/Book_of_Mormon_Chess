using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        if (playerIndex == 0)
        {
            Debug.Log($"🙂 New material value: {teamCounts[playerIndex]}");
        }
    }

    public void ReportDeath(int playerIndex, int materialValue)
    {
        teamCounts[playerIndex] -= materialValue;

        if (playerIndex == 0)
        {
            Debug.Log($"💀Remaining material value: {teamCounts[playerIndex]}");
        }
        if (teamCounts[playerIndex] <= 0 && playerIndex == 0)
        {
            Debug.Log($"Player {playerIndex} has lost all their pieces and thus lost the game!");
            winScreen.SetActive(true);
        }
    }
}
