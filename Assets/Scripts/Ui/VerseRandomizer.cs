using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Passage
{
    public string name;
    [TextArea(2, 10)]
    public string verse;
    public string referenceLink =
    "https://www.churchofjesuschrist.org/study/scriptures/bofm/bofm-title?lang=eng";
}

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class VerseRandomizer : MonoBehaviour
{
    [TextArea(2,5)]
    public string[] verses;
    public Passage[] passages;

    void Start()
    {
        int index = UnityEngine.Random.Range(0, verses.Length);
        GetComponent<TMPro.TextMeshProUGUI>().text = verses[index];
    }
}
