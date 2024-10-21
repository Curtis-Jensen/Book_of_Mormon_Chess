using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
	public InputField input;

	public void SetColor(int playerNum)
	{
		var playerNumString = playerNum.ToString();
  		Debug.Log(input.text);
		PlayerPrefs.SetString($"player{playerNumString}Color", input.text);
	}
}
