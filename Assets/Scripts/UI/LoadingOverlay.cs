/// Assets/Scripts/LoadingOverlay.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingOverlay : MonoBehaviour
{
    /* ---------- 在 Inspector 里拖引用 ---------- */
    public Slider         progressBar;
    public TMP_Text       progressText;
    public TMP_Text       tipText;
    public CanvasGroup    canvasGroup;   // 用来渐隐渐显
    [TextArea] public string[] tips;     // 自行填 Tips

    public static LoadingOverlay Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);            // 默认隐藏
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /* ----------------- 对外接口 ----------------- */
    /*
    public void Show()
    {
        gameObject.SetActive(true);
        tipText.text = tips.Length == 0 ? "Loading..." :
            tips[Random.Range(0, tips.Length)];
        progressBar.value = 0;
        progressText.text = "0%";
        StopAllCoroutines();
        StartCoroutine(FadeCanvas(0f, 1f, 0.25f));
    }
    */
    
    public void Show()
    {
        gameObject.SetActive(true);          // 关键：立刻激活
        canvasGroup.alpha = 1f;              // 如果想淡入再改成 0 → 1
        /* …更新随机 Tip、进度条归零等… */
    }


    public void Hide()  => StartCoroutine(FadeCanvas(1f, 0f, 0.25f, () =>
    {
        gameObject.SetActive(false);
    }));

    public void UpdateProgress(float p)
    {
        progressBar.value = p;
        progressText.text = Mathf.RoundToInt(p * 100f) + "%";
    }

    /* -------------- 私有淡入淡出协程 -------------- */
    private IEnumerator FadeCanvas(float from, float to, float time, System.Action done = null)
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