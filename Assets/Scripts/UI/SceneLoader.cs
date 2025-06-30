using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string targetScene;

    public static void LoadSceneWithLoading(string target)
    {
        targetScene = target;
        SceneManager.LoadScene("LoadingScene");
    }
}