using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiSelfDestruct : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("isAi") == 1) Destroy(gameObject);
    }
}
