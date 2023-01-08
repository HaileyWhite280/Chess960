using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour
{
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public static void ToChess()
    {
        GameManager.Instance.isChess = true;

        LoadScene("main_scene");
    }

    public static void To960()
    {
        GameManager.Instance.isChess = false;

        LoadScene("main_scene");
    }

    public static void ToTitle()
    {
        LoadScene("Title");
    }
}
