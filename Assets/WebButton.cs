using TMPro;
using UnityEngine;

public class WebButton : MonoBehaviour
{
    public string url = "https://www.churchofjesuschrist.org/study/scriptures/bofm?lang=eng"; // Default URL

    public void PressButton()
    {
        Application.OpenURL(url);
    }
}