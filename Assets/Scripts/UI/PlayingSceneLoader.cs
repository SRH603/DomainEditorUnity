using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 异步场景加载器（带进度条淡入/淡出）
/// </summary>
public static class PlayingSceneLoader
{
    /* ────────────── 启动即注入 Overlay + Runner ────────────── */
    private const string OVERLAY_PATH = "PlayingSceneLoadingOverlay";

    // 游戏首次加载（含 Editor 进入 Play）立刻执行
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        // ① 保证 Runner 先存在
        GetRunner();

        // ② 若 Overlay 单例还没生成，则 Resources.Instantiate 并挂在 DontDestroyOnLoad
        if (PlayingSceneLoadingOverlay.Instance == null)
        {
            var prefab = Resources.Load<PlayingSceneLoadingOverlay>(OVERLAY_PATH);
            if (prefab == null)
            {
                Debug.LogError($"[SceneLoader] Resources/{OVERLAY_PATH}.prefab 不存在！");
                return;
            }
            Object.DontDestroyOnLoad(Object.Instantiate(prefab).gameObject);
        }
    }

    /* ────────────── 协程执行器 ────────────── */
    private class Runner : MonoBehaviour { }
    private static Runner runner;
    private static Runner GetRunner()
    {
        if (runner) return runner;
        var go = new GameObject("~PlayingSceneLoader");
        Object.DontDestroyOnLoad(go);
        return runner = go.AddComponent<Runner>();
    }

    /* ────────────── 公共接口 ────────────── */
    public static void Load(string targetScene) =>
        GetRunner().StartCoroutine(LoadRoutine(targetScene));

    /* ────────────── 主流程协程 ────────────── */
    private static IEnumerator LoadRoutine(string target)
    {
        /* 0) Overlay 已在 Bootstrap 中创建并标记 DontDestroy，无需再次 Instantiate */

        /* 1) 淡入 */
        PlayingSceneLoadingOverlay.Instance.Show();
        yield return new WaitForSecondsRealtime(0.25f);

        /* 2) 异步加载场景（禁止激活） */
        var op = SceneManager.LoadSceneAsync(target, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        float shown = 0f;
        const float SMOOTH = 0.15f;

        while (op.progress < 0.9f)
        {
            shown = Mathf.Lerp(shown, op.progress / 0.9f, SMOOTH);
            PlayingSceneLoadingOverlay.Instance.UpdateProgress(shown);
            yield return null;
        }
        while (shown < 0.999f)
        {
            shown = Mathf.Lerp(shown, 1f, SMOOTH);
            PlayingSceneLoadingOverlay.Instance.UpdateProgress(shown);
            yield return null;
        }
        PlayingSceneLoadingOverlay.Instance.UpdateProgress(1f);

        /* 3) 激活场景 */
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        /* 4) 等首帧渲染完 */
        yield return new WaitForEndOfFrame();
        
        /* 4.5) 额外等待 0.2 秒 */
        yield return new WaitForSecondsRealtime(0.2f);   // ← 新增这一行

        /* 5) 淡出 */
        PlayingSceneLoadingOverlay.Instance.Hide();
    }
}
