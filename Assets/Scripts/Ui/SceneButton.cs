using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneButton : MonoBehaviour
{
    public void SelectScene()
    {
        PlayerPrefs.SetString("gameMode", gameObject.name);

        //FindObjectOfType<MenuTabController>().ShowTabbedMenu();
    }
}
