using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderHook : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        FindAnyObjectByType<SceneLoader>().LoadScene(sceneName);
    }
}
