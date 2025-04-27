using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "PieceColors", menuName = "Custom/PieceColors")]
public class PieceColors : ScriptableObject
{
    public ColorSet[] colors;
}

[System.Serializable]
public class ColorSet
{
    public Color baseColor;
    public Color kingColor;
}
