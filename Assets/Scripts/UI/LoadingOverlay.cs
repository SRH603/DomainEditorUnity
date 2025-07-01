/* ────────────────────────────────
 *  Assets/Scripts/LoadingOverlay.cs
 *  单例式全屏加载层：淡入 → 更新进度 → 淡出
 * ──────────────────────────────── */
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingOverlay : MonoBehaviour
{
    [Header("UI")]
    public Slider      progressBar;
    public TMP_Text    progressText;
    public TMP_Text    tipText;
    public CanvasGroup canvasGroup;          // 用 α 做淡入淡出
    [TextArea] public string[] tips;

    public static LoadingOverlay Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);     // 默认隐藏
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /* ---------- 对外接口 ---------- */
    public void Show(float fadeTime = .25f)
    {
        gameObject.SetActive(true);
        tipText.text = tips.Length == 0 ? "Loading…" :
                       tips[Random.Range(0, tips.Length)];

        progressBar.value = 0f;
        progressText.text = "0%";

        StopAllCoroutines();
        StartCoroutine(FadeCanvas(0f, 1f, fadeTime));   // 淡入
    }

    public void Hide(float fadeTime = .25f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCanvas(1f, 0f, fadeTime, () =>
        {
            gameObject.SetActive(false);
        }));
    }

    public void UpdateProgress(float p)          // 0.0‒1.0
    {
        p = Mathf.Clamp01(p);
        progressBar.value  = p;
        progressText.text  = Mathf.RoundToInt(p * 100f) + "%";
    }

    /* ---------- 内部协程 ---------- */
    IEnumerator FadeCanvas(float from, float to, float time, System.Action done = null)
    {
        for (float t = 0; t < time; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        canvasGroup.alpha = to;
        done?.Invoke();
    }
}
