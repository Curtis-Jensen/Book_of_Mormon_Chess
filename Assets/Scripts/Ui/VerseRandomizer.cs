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
    public Passage[] passages;
    public GameObject button;

    void Start()
    {
        int index = UnityEngine.Random.Range(0, passages.Length);
        GetComponent<TMPro.TextMeshProUGUI>().text = $"{passages[index].verse}";

        button.GetComponent<WebButton>().url = passages[index].referenceLink;
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"({passages[index].name})";
    }
}
