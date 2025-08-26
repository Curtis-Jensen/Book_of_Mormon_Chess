using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class VerseRandomizer : MonoBehaviour
{
    [TextArea(2,5)]
    public String[] verses;

    void Start()
    {
        int index = UnityEngine.Random.Range(0, verses.Length);
        GetComponent<TMPro.TextMeshProUGUI>().text = verses[index];
    }
}
