/* ────────────────────────────────────────────────────────────────
 *  Assets/Scripts/PlayingSceneLoadingOverlay.cs
 *  单例式全屏加载层：AssignInfo → 淡入 → 更新进度 → 淡出
 * ──────────────────────────────────────────────────────────────── */
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayingSceneLoadingOverlay : MonoBehaviour
{
    /* ───────────── UI 绑定 ───────────── */
    [Header("进度条")]
    public Slider   progressBar;
    public TMP_Text progressPercent;     // 0-100%

    [Header("曲目信息区")]
    public TMP_Text titleText;           // Title
    public TMP_Text artistText;          // Artist
    public TMP_Text chartText;           // “Chart: EZ / HD …”
    public TMP_Text illustratorText;     // “Illustration: xxx”
    public Image    illustrationImage;   // 封面

    [Header("CanvasGroup（做淡入淡出）")]
    public CanvasGroup canvasGroup;

    /* ───────────── 单例 ───────────── */
    public static PlayingSceneLoadingOverlay Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);     // 默认隐藏
    }

    /* ───────────── 公共接口 ───────────── */

    /// <summary>一次性把所有 UI 文本 / 图片写进去</summary>
    public void AssignInfo(string title, string artist, string chart, string illustrator, Sprite illustration)
    {
        titleText.text        = title;
        artistText.text       = artist;
        chartText.text        = $"Chart: {chart}";
        illustratorText.text  = $"Illustration: {illustrator}";
        illustrationImage.sprite = illustration;
    }

    /// <summary>淡入</summary>
    public void Show(float fadeTime = .25f)
    {
        StopAllCoroutines();
        gameObject.SetActive(true);

        progressBar.value   = 0f;
        progressPercent.text = "0%";

        StartCoroutine(FadeCanvas(0f, 1f, fadeTime));      // α: 0 → 1
    }

    /// <summary>淡出</summary>
    public void Hide(float fadeTime = .25f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCanvas(1f, 0f, fadeTime, () =>
        {
            gameObject.SetActive(false);
        }));
    }

    /// <summary>更新进度（0-1）</summary>
    public void UpdateProgress(float p)
    {
        p = Mathf.Clamp01(p);
        progressBar.value  = p;
        progressPercent.text = Mathf.RoundToInt(p * 100f) + "%";
    }

    /* ───────────── 私有协程 ───────────── */
    IEnumerator FadeCanvas(float from, float to, float time, System.Action onDone = null)
    {
        for (float t = 0; t < time; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        canvasGroup.alpha = to;
        onDone?.Invoke();
    }
}
