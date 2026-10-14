using UnityEngine;

public class QuitButton : MonoBehaviour
{
    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // A browser tab can't be closed from inside the game
        gameObject.SetActive(false);
#endif
    }

    public void Quit()
    {
        Application.Quit();
    }
}
