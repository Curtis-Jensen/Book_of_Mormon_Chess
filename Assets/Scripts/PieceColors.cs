using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public class ColorSet : MonoBehaviour
{
    public Color baseColor;
    public Color kingColor;
}

[CreateAssetMenu(fileName = "PieceColors", menuName = "Custom/PieceColors")]
public class PieceColors : ScriptableObject
{
    public ColorSet[] colors;
}
