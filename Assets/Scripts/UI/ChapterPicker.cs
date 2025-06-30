// ChapterGallery.cs  (2025-06-29 修正版)
// -------------------------------------------------------------
// * 横向拖动画廊 + 圆点指示 + SongSelect.ChapterSelect(idx)
// * 支持窗口宽度动态变化
// * 拖动松手后吸附最近页 (≥30% 翻页)
// -------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChapterPicker : MonoBehaviour
{
    [Header("依赖")]
    public SongSelect songSelect;               // 场景中的 SongSelect
    private PacksContainer Packs => songSelect.packsContainer;

    [Header("画廊 UI")]
    public ScrollRect      scrollRect;          // 必须 Horizontal
    public RectTransform   contentRoot;         // ScrollRect.Content
    public GameObject      itemPrefab;          // 单页预制

    [Header("控制按钮")]
    public Button leftArrowBtn;
    public Button rightArrowBtn;
    public Button enterBtn;
    public Button allChaptersBtn;

    [Header("底部圆点")]
    public GameObject dotPrefab;
    public Transform  dotContainer;
    public Color dotNormal = new(1,1,1,0.35f);
    public Color dotActive = Color.white;

    [Header("可选文本")]
    public TMP_Text chapterNameText;
    public TMP_Text chapterDescText;

    [Header("滑动 / 吸附")]
    [Range(5f, 25f)] public float snapSpeed = 14f; // 越大吸附越快
    [Range(0.2f, 0.5f)] public float turnThreshold = .3f; // 拖过多少占比算翻页 (默认 30%)

    //────────────────────────────────────
    [SerializeField] private bool   dragging;
    [SerializeField] public ChapterScrollbar scrollbar;
    private int    curIndex;
    private float  targetPos;
    private readonly List<Image> dots = new();

    private RectTransform viewportRT;
    private float lastViewportW = -1f;
    private float pageStep => Packs.packs.Count <= 1 ? 0 : 1f / (Packs.packs.Count - 1);

    //────────────────────────────────────
    #region Unity
    private void Awake()
    {
        if (songSelect == null) songSelect = FindObjectOfType<SongSelect>();
        viewportRT = (RectTransform)scrollRect.viewport;

        BuildGallery();
        BuildDots();

        curIndex = Mathf.Clamp(songSelect.currentChapterIndex, 0, Packs.packs.Count-1);
        //JumpTo(curIndex, true);
        JumpTo(0, true);
        RefreshTexts();

        leftArrowBtn .onClick.AddListener(() => Shift(-1));
        rightArrowBtn.onClick.AddListener(() => Shift(+1));
        enterBtn     .onClick.AddListener(EnterCurrentChapter);
        allChaptersBtn?.onClick.AddListener(()=>songSelect.ChapterSelect(-1));
    }

    private void Update()
    {
        if (ViewportChanged()) RebuildLayout();

        if (!dragging)
        {
            float np = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, targetPos, Time.deltaTime * snapSpeed);
            scrollRect.horizontalNormalizedPosition = np;
        }

        // 实时页码监控
        int idx = Mathf.RoundToInt(scrollRect.horizontalNormalizedPosition / pageStep);
        idx = Mathf.Clamp(idx, 0, Packs.packs.Count-1);
        if (idx != curIndex)
        {
            curIndex = idx;
            RefreshDots();
            RefreshTexts();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        // UI 布局变化比 Update 更早捕获
        if (ViewportChanged()) RebuildLayout();
    }
    #endregion

    //────────────────────────────────────
    #region Build
    private void BuildGallery()
    {
        foreach (Transform c in contentRoot) Destroy(c.gameObject);

        for (int i = 0; i < Packs.packs.Count; i++)
        {
            var pack = Packs.packs[i];
            var go   = Instantiate(itemPrefab, contentRoot);
            var img  = go.GetComponentInChildren<Image>();

            Sprite sp = null;
            var fld = pack.GetType().GetField("illustration");
            if (fld != null) sp = fld.GetValue(pack) as Sprite;
            //if (sp == null && pack.tracks?.Count > 0) sp = pack.tracks[0].previewIllustration;
            img.sprite = sp;

            if (go.TryGetComponent(out Button b))
                b.onClick.AddListener(EnterCurrentChapter);
        }

        RebuildLayout();
    }

    private void BuildDots()
    {
        foreach (Transform d in dotContainer) Destroy(d.gameObject);
        dots.Clear();
        for (int i = 0; i < Packs.packs.Count; i++)
        {
            var g = Instantiate(dotPrefab, dotContainer);
            var img = g.GetComponent<Image>();
            img.color = dotNormal;
            dots.Add(img);
        }
    }

    private void RebuildLayout()
    {
        float vw = viewportRT.rect.width;
        if (vw <= 0) return;

        // 1) content 宽 = Viewport * 页数
        contentRoot.sizeDelta = new Vector2(vw * Packs.packs.Count, contentRoot.sizeDelta.y);

        // 2) 每个子 item 宽 = Viewport
        foreach (RectTransform child in contentRoot)
            child.sizeDelta = new Vector2(vw, child.sizeDelta.y);

        lastViewportW = vw;

        // 3) 立刻校正位置
        JumpTo(curIndex, true);
    }
    #endregion

    //────────────────────────────────────
    #region Drag / Snap

    public void OnBeginDrag()
    {
        dragging = true;
        //Debug.Log("Begin");
    }

    public void OnEndDrag()
    {
        //Debug.Log("End");
        dragging = false;

        // 拖动总位移占比
        float progress = scrollRect.horizontalNormalizedPosition / pageStep - curIndex;

        if      (progress >=  turnThreshold) curIndex = Mathf.Min(curIndex + 1, Packs.packs.Count - 1);
        else if (progress <= -turnThreshold) curIndex = Mathf.Max(curIndex - 1, 0);

        JumpTo(curIndex);
        RefreshDots();
        RefreshTexts();
    }

    private void Shift(int delta)
    {
        curIndex = Mathf.Clamp(curIndex + delta, 0, Packs.packs.Count - 1);
        JumpTo(curIndex);
    }

    private void JumpTo(int idx, bool immediate=false)
    {
        curIndex  = idx;
        targetPos = idx * pageStep;
        targetPos = Mathf.Clamp01(targetPos);

        if (immediate)
            scrollRect.horizontalNormalizedPosition = targetPos;
        
        RefreshDots();
        RefreshTexts();
        //Debug.Log(curIndex);
    }
    #endregion

    //────────────────────────────────────
    #region UI Refresh & Enter
    private void RefreshDots()
    {
        for (int i = 0; i < dots.Count; i++)
            dots[i].color = (i == curIndex) ? dotActive : dotNormal;
    }

    private void RefreshTexts()
    {
        var pack = Packs.packs[curIndex];
        if (chapterNameText)  chapterNameText.text = pack.name.en;
        if (chapterDescText)  chapterDescText.text = pack.description.en;
    }

    private void EnterCurrentChapter()
    {
        songSelect.ChapterSelect(curIndex);
        PlayerPrefs.SetInt("ChapterIndex", curIndex);
    }

    public void ChapterIndexSynchronization()
    {
        if (songSelect.currentChapterIndex != -1)
            JumpTo(songSelect.currentChapterIndex, true);
    }
    #endregion

    //────────────────────────────────────
    #region Helpers
    private bool ViewportChanged()
    {
        return !Mathf.Approximately(viewportRT.rect.width, lastViewportW);
    }
    #endregion
}
