using UnityEngine;

public class SceneButton : MonoBehaviour
{
    public SceneLoader sceneLoader;
    public int configIndex;

    public void OnClick()
    {
        sceneLoader.LoadWithConfig(sceneLoader.sceneConfigs[configIndex]);
    }
}
