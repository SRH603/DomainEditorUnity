using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LoadingScreen : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipText;

    [TextArea]
    public List<string> tips = new List<string>();  // 可在Inspector里填入任意条Tips

    public string sceneToLoad;  // 设置要加载的场景名

    void Start()
    {
        ShowRandomTip();
        StartCoroutine(LoadSceneAsync(SceneLoader.targetScene));
    }

    void ShowRandomTip()
    {
        if (tips.Count == 0)
        {
            tipText.text = "Did you know? You can add your own tips!";
        }
        else
        {
            int index = Random.Range(0, tips.Count);
            tipText.text = tips[index];
        }
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";

            if (operation.progress >= 0.9f)
            {
                progressText.text = "Press any key to continue";
                if (Input.anyKeyDown)
                {
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

}