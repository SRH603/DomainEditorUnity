/* ────────────────────────────────
 * Assets/Scripts/SceneLoader.cs
 * 统一异步加载场景，带淡入/淡出与实时进度
 * ──────────────────────────────── */
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class SceneLoader
{
    /* 内部 Runner：协程执行器（DontDestroy） */
    private class Runner : MonoBehaviour { }
    private static Runner runner;
    private static Runner GetRunner()
    {
        if (runner) return runner;
        var go = new GameObject("~SceneLoader");
        Object.DontDestroyOnLoad(go);
        return runner = go.AddComponent<Runner>();
    }

    /* ---------- 公共接口 ---------- */
    public static void Load(string targetScene) =>
        GetRunner().StartCoroutine(LoadRoutine(targetScene));

    /* ---------- 主流程协程 ---------- */
    private static IEnumerator LoadRoutine(string target)
    {
        /* 0) 保障 LoadingOverlay 单例存在 */
        if (LoadingOverlay.Instance == null)
            Object.Instantiate(Resources.Load<LoadingOverlay>("LoadingOverlay"));

        /* 1) 淡入（可自行调 Show 的淡入时长） */
        LoadingOverlay.Instance.Show();                        // 0 %
        yield return new WaitForSecondsRealtime(.25f);         // 淡入结束

        /* 2) 开始异步加载场景，但先禁止激活 */
        var op = SceneManager.LoadSceneAsync(target, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        float displayProg = 0f;                                // 显示用的进度
        const float SMOOTH = .15f;                             // 平滑系数

        /* 2-A) 场景磁盘 I/O 阶段（0-0.9） */
        while (op.progress < .9f)
        {
            float raw = op.progress / .9f;                     // 0-1
            displayProg = Mathf.Lerp(displayProg, raw, SMOOTH);
            LoadingOverlay.Instance.UpdateProgress(displayProg);
            yield return null;                                 // 每帧让出
        }

        /* 2-B) 阶段完成，进度补满到 100 % */
        while (displayProg < 1f - 0.001f)
        {
            displayProg = Mathf.Lerp(displayProg, 1f, SMOOTH);
            LoadingOverlay.Instance.UpdateProgress(displayProg);
            yield return null;
        }
        LoadingOverlay.Instance.UpdateProgress(1f);

        /* 3) 允许场景激活，进入新场景 */
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;                  // 等真正完成

        /* 4) 等首帧渲染完，避免白屏/黑屏一闪 */
        yield return new WaitForEndOfFrame();
        
        /* 4.5) 额外等待 0.2 秒 */
        yield return new WaitForSecondsRealtime(0.1f);   // ← 新增这一行

        /* 5) 淡出并在淡出结束后隐藏 Overlay */
        LoadingOverlay.Instance.Hide();                        // 内部带渐隐
    }
}
