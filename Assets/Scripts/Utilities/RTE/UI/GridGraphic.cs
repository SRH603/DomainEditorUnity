using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>在自身局部坐标 (0,0)‒(width,height) 区间绘制整张网格。</summary>
[RequireComponent(typeof(CanvasRenderer))]
public class GridGraphic : Graphic
{
        /* ◆ 外部可调参数 —— 同前 ◆ */
        [Header("格子数量")]
        public int laneCount = 8;
        public int beatCount = 256;

        [Header("单元格大小 (px)")]
        public float laneWidth  = 128f;
        public float beatHeight = 64f;

        [Header("线宽 & 颜色")]
        public float mainLineWidth = 2f;
        public float minorLineWidth = 1f;  // 细分线
        public Color mainLineColor = new(1f, 1f, 1f, .12f);
        public Color minorLineColor = new(1f, 1f, 1f, .12f);
        
        [Header("细分")]
        [Range(1, 12)] public int hSubdiv = 1;   // 横向细分（beat）
        [Range(1, 12)] public int vSubdiv = 1;   // 纵向细分（lane）
        
        [Header("当前拍线")]
        public Color currentLineColor = Color.red;
        public float currentLineWidth = 3f;
        
        /* =========== 运行时 =========== */
        public float CurrentBeat { get; private set; } = 0;
        RectTransform self;

        /* ◆ 关键新增：Content 引用 ◆ */
        [Header("Content (ScrollRect)")]
        public RectTransform contentRect;          // 在 Inspector 拖进 Content

        /* —— 内部对象池 —— */
        readonly List<RectTransform> pool = new();
        Texture2D whiteTex;                        // Unity 内置白纹理

        [ContextMenu("Rebuild Grid")]
        
        void Awake()
        {
            self = transform as RectTransform;
        }
        
        /* 外部调用：刷新当前拍线 */
        public void SetCurrentBeat(float beat)
        {
            CurrentBeat = beat;
            UpdateCurrentBeatLine();
        }

        public void RebuildGrid()
        {
            BuildAllLines();
        }
        
        /* ---------------- 构建网格 ---------------- */
        void BuildAllLines()
        {
            if (whiteTex == null) whiteTex = Texture2D.whiteTexture;

    // 1) 先算出各类线的数量
    int mainV = laneCount + 1;               // 主竖线
    int mainH = beatCount + 1;               // 主横线
    int minorV = vSubdiv  > 1 ? (vSubdiv - 1)  * laneCount : 0;  // 细分竖线
    int minorH = hSubdiv  > 1 ? (hSubdiv - 1)  * beatCount : 0;  // 细分横线
    int total  = mainV + mainH + minorV + minorH + 1;           // +1 是“当前拍线”

    // 2) 确保对象池够大
    EnsurePool(total);

    float totalWidth  = laneCount * laneWidth;
    float totalHeight = beatCount * beatHeight;
    
    // ① 同步 Content
    if (contentRect != null)
    {
        // 记录旧的尺寸和位置
        Vector2 oldSize = contentRect.sizeDelta;
        Vector2 oldPos = contentRect.anchoredPosition;

// 保证锚点和轴心设置为左下角（通常在初始化时调用一次）
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.zero;
        contentRect.pivot = Vector2.zero;

// 设置新的尺寸
        Vector2 newSize = new Vector2(totalWidth, totalHeight);
        contentRect.sizeDelta = newSize;

// 判断尺寸是否真的变化
        if (!Mathf.Approximately(oldSize.x, newSize.x) || !Mathf.Approximately(oldSize.y, newSize.y))
        {
            // 如果是大切换（尺寸有变化），重置位置为原点
            contentRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            // 如果只是小切换（尺寸未变），保持当前位置
            contentRect.anchoredPosition = oldPos;
        }

    }

    // ② 同步 GridLayer 本身 —— 新写法
    RectTransform self = (RectTransform)transform;
    self.anchorMin = self.anchorMax = Vector2.zero;
    self.pivot     = Vector2.zero;
    self.anchoredPosition = Vector2.zero;
    self.sizeDelta        = new Vector2(totalWidth, totalHeight);

    // （后面再开始 idx=0 循环放置各条线……）
    int idx = 0;
    // ─── 主竖 ───
    for(int x=0; x<=laneCount; x++)
    {
        RectTransform rt = Spawn(idx++);
        rt.sizeDelta        = new Vector2(mainLineWidth, totalHeight);
        rt.anchoredPosition = new Vector2(x * laneWidth, 0);
        rt.GetComponent<RawImage>().color = mainLineColor;
    }
    // ─── 主横 ───
    for(int y=0; y<=beatCount; y++)
    {
        RectTransform rt = Spawn(idx++);
        rt.sizeDelta        = new Vector2(totalWidth, mainLineWidth);
        rt.anchoredPosition = new Vector2(0, y * beatHeight);
        rt.GetComponent<RawImage>().color = mainLineColor;
    }
    // ─── 细分竖 ───
    if(vSubdiv > 1)
    {
        float step = laneWidth / vSubdiv;
        for(int lane=0; lane<laneCount; lane++)
            for(int i=1; i<vSubdiv; i++)
            {
                RectTransform rt = Spawn(idx++);
                rt.sizeDelta        = new Vector2(minorLineWidth, totalHeight);
                rt.anchoredPosition = new Vector2(lane * laneWidth + i * step, 0);
                rt.GetComponent<RawImage>().color = minorLineColor;
            }
    }
    // ─── 细分横 ───
    if(hSubdiv > 1)
    {
        float step = beatHeight / hSubdiv;
        for(int beat=0; beat<beatCount; beat++)
            for(int i=1; i<hSubdiv; i++)
            {
                RectTransform rt = Spawn(idx++);
                rt.sizeDelta        = new Vector2(totalWidth, minorLineWidth);
                rt.anchoredPosition = new Vector2(0, beat * beatHeight + i * step);
                rt.GetComponent<RawImage>().color = minorLineColor;
            }
    }
    // ─── 当前拍 ─── （最后放，确保 idx 在池子范围内）
    currentBeatRT = Spawn(idx++);
    currentBeatRT.sizeDelta        = new Vector2(totalWidth, currentLineWidth);
    currentBeatRT.anchoredPosition = Vector2.zero; // 后面 UpdateCurrentBeatLine 会定位
    currentBeatRT.GetComponent<RawImage>().color = currentLineColor;
    UpdateCurrentBeatLine();

    // 多余的全部隐藏
    for(int i=idx; i<pool.Count; i++)
        pool[i].gameObject.SetActive(false);
        }

        /* ——— 对象池工具 ——— */
        void EnsurePool(int n)
        {
            while (pool.Count < n)
            {
                RawImage img = new GameObject("Line", typeof(RectTransform), typeof(RawImage))
                               .GetComponent<RawImage>();
                img.transform.SetParent(transform, false);
                img.transform.SetAsFirstSibling();

                img.texture       = whiteTex;
                img.color         = mainLineColor;
                img.raycastTarget = false;

                RectTransform rt = img.rectTransform;
                rt.pivot = Vector2.zero;
                rt.anchorMin = rt.anchorMax = Vector2.zero;

                pool.Add(rt);
            }
        }
        RectTransform Spawn(int index)
        {
            RectTransform rt = pool[index];
            rt.gameObject.SetActive(true);
            //rt.GetComponent<RawImage>().color = lineColor;
            return rt;
        }
        
        /* 存放当前拍线 RT */
        RectTransform currentBeatRT;

        void UpdateCurrentBeatLine()
        {
            if (currentBeatRT == null) return;
            float y = CurrentBeat * beatHeight;
            currentBeatRT.anchoredPosition = new Vector2(0, y);
        }

}