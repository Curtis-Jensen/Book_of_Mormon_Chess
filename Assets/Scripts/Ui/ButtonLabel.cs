using TMPro;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class ButtonLabel : MonoBehaviour
{
    void OnValidate()
    {
        string label = gameObject.name.Trim('"');
        TMP_Text text = GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = label;
    }
}
