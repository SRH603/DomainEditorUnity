// Assets/Scripts/SceneLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class SceneLoader
{
    private class Runner : MonoBehaviour { }
    private static Runner runner;
    private static Runner GetRunner()
    {
        if (runner != null) return runner;
        var go = new GameObject("~SceneLoader"); Object.DontDestroyOnLoad(go);
        return runner = go.AddComponent<Runner>();
    }

    public static void Load(string target) => GetRunner().StartCoroutine(LoadRoutine(target));

    // SceneLoader.cs （关键段落）
    private static IEnumerator LoadRoutine(string target)
    {
        /* 阶段 1 —— 让 UI 先出现 */
        /* ① 确保 Overlay 存在并立刻显出来 */
        if (LoadingOverlay.Instance == null)
            Object.Instantiate(Resources.Load<LoadingOverlay>("LoadingOverlay"));
        LoadingOverlay.Instance.Show();              // 立刻 SetActive(true)
        LoadingOverlay.Instance.Show();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        /* 阶段 2 —— 只加载“精简版”场景 */
        var sceneOp = SceneManager.LoadSceneAsync(target, LoadSceneMode.Single);
        while (!sceneOp.isDone) {
            LoadingOverlay.Instance.UpdateProgress(sceneOp.progress * 0.6f); // 0~60%
            yield return null;
        }

        /* 阶段 3 —— 分帧加载音频（Resources 方式示例） */
        string path = "Audio/BGM";                          // Resources/Audio/BGM
        var clips = Resources.LoadAll<AudioClip>(path);     // 同步取 Asset 数组（不解码）
        float  oneStep = 0.4f / clips.Length;               // 40% 预算给音频
        foreach (var clip in clips)
        {
            var req = Resources.LoadAsync<AudioClip>($"{path}/{clip.name}");
            while (!req.isDone) {
                yield return null;      // 让 UI 刷帧
            }
            yield return req;           // 解码完成
            LoadingOverlay.Instance.UpdateProgress(0.6f + oneStep); // 60~100%
        }

        /* (可选) 等玩家按键、淡出 Overlay */
        LoadingOverlay.Instance.Hide();
    }

}