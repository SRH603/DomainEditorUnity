// Assets/Scripts/Utilities/RTE/Windows/Utilities/ChartInfoInfoPanel.cs
using System.Reflection;
using UnityEngine;
using Battlehub.RTCommon; // IRTE.Undo

public class ChartInfoInfoPanel : MonoBehaviour
{
    [Header("把 IMGUI 面板画到这个 RectTransform 区域")]
    public RectTransform host;

    [Header("标题")]
    public string title = "Info";

    private GameData _gameData;
    private Info _info;

    // offset 的“缓冲值”（编辑框里改，按 Apply 才写入 + Undo）
    private string _offsetBuffer;

    // 反射到 Info.offset 字段，用于 Undo Begin/EndRecordValue
    private static readonly FieldInfo kInfoOffsetField =
        typeof(Info).GetField("offset", BindingFlags.Public | BindingFlags.Instance);

    // 滚动条（横向+纵向）
    private Vector2 _scroll = Vector2.zero;

    // 样式
    private bool _stylesReady;
    private GUIStyle _h1, _label, _box, _textFlat;
    private Texture2D _bgBox, _bgInput;
    private bool _warnedOnce;

    void OnEnable()  { var cm = ChartManager.Instance; if (cm != null) cm.OnBpmListChanged += OnExternalChanged; }
    void OnDisable() { var cm = ChartManager.Instance; if (cm != null) cm.OnBpmListChanged -= OnExternalChanged; }

    private void OnExternalChanged()
    {
        PullFromGameData();   // 重新把 info.designer / bpm / rating / offset 等灌回 UI
    }

    void Awake()
    {
        var cm = ChartManager.Instance;
        _gameData = cm != null ? cm.gameData : null;

        if (_gameData == null)
        {
            Debug.LogWarning("[ChartInfo/Info] 未找到 GameData。");
            return;
        }
        if (_gameData.info == null) _gameData.info = new Info();
        _info = _gameData.info;
        _offsetBuffer = _info.offset.ToString("0.###");
    }

    void OnGUI()
    {
        if (!_stylesReady) BuildStyles();

        // 宿主区域换算到屏幕坐标
        Rect r = GetScreenRect(host);
        if (r.width < 2f || r.height < 2f)
        {
            if (!_warnedOnce)
            {
                _warnedOnce = true;
                Debug.LogWarning("[ChartInfo/Info] host 区域无效（RectTransform 面积太小？）");
            }
            return;
        }

        GUI.BeginGroup(r);
        GUILayout.BeginVertical(GUILayout.Width(r.width));

        // 顶部标题条
        GUILayout.BeginHorizontal(_box);
        GUILayout.Label(title, _h1);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // 可滚动区域：横向+纵向滚动条（按需显示）
        _scroll = GUILayout.BeginScrollView(_scroll, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));
        {
            GUILayout.BeginVertical(_box); // 内容外框

            // designer（即时写回，不走 undo）
            LineString("Designer", ref _info.designer, 240);

            // bpm (string)（即时写回，不走 undo）
            LineString("BPM (string)", ref _info.bpm, 120);

            // rating (double)（即时写回，不走 undo）
            LineDouble("Rating", ref _info.rating, 120);

            // offset (float) —— 使用“缓冲 + Apply（含 Undo/Redo）”
            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset (sec)", _label, GUILayout.Width(120));
            _offsetBuffer = GUILayout.TextField(_offsetBuffer ?? "", _textFlat, GUILayout.Width(120));

            if (GUILayout.Button("Apply", GUILayout.Width(72)))
            {
                if (float.TryParse(_offsetBuffer, out var nv))
                {
                    var rte  = Battlehub.RTCommon.IOC.IsRegistered<IRTE>() ? Battlehub.RTCommon.IOC.Resolve<IRTE>() : null;
                    var undo = rte != null ? rte.Undo : null;

                    if (undo != null && kInfoOffsetField != null)
                    {
                        undo.BeginRecord();
                        undo.BeginRecordValue(_info, kInfoOffsetField);
                        _info.offset = nv;
                        undo.EndRecordValue(_info, kInfoOffsetField);
                        undo.EndRecord();
                    }
                    else
                    {
                        _info.offset = nv;
                    }

                    // 通知外界（复用 BPM 的刷新事件，方便统一监听）
                    ChartManager.Instance?.NotifyBpmListChanged();

                    Debug.Log($"[ChartInfo/Info] Offset Apply 成功：{nv}");
                }
                else
                {
                    Debug.LogWarning($"[ChartInfo/Info] Offset 解析失败：\"{_offsetBuffer}\"");
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical(); // /内容外框
        }
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
        GUI.EndGroup();
    }

    // ========== 公共：从 GameData 拉取最新值到 UI 缓冲 ==========
    public void PullFromGameData()
    {
        if (_gameData == null) return;
        if (_gameData.info == null) _gameData.info = new Info();
        _info = _gameData.info;

        // 同步 offset 到输入缓冲（用于“撤销/重做后界面自动还原”）
        _offsetBuffer = _info != null ? _info.offset.ToString("0.###") : "0";
    }

    /* ----------------- 小部件封装 ----------------- */
    void LineString(string label, ref string value, float fieldWidth)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, _label, GUILayout.Width(120));
        string nv = GUILayout.TextField(value ?? "", _textFlat, GUILayout.Width(fieldWidth));
        if (nv != value) value = nv; // 立即写入，不走 Undo
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    void LineDouble(string label, ref double value, float fieldWidth)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, _label, GUILayout.Width(120));
        string s = GUILayout.TextField(value.ToString("0.###"), _textFlat, GUILayout.Width(fieldWidth));
        if (double.TryParse(s, out var nv)) value = nv; // 立即写入
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    /* ----------------- 样式 ----------------- */
    Texture2D Tex(Color c) { var t=new Texture2D(1,1); t.SetPixel(0,0,c); t.Apply(); return t; }

    void BuildStyles()
    {
        var colBox   = new Color(0.18f,0.18f,0.18f,1);
        var colInput = new Color(0.22f,0.22f,0.22f,1);

        _bgBox   = Tex(colBox);
        _bgInput = Tex(colInput);

        _h1    = new GUIStyle(GUI.skin.label){ fontSize=15, fontStyle=FontStyle.Bold, normal={ textColor = Color.white } };
        _label = new GUIStyle(GUI.skin.label){ normal={ textColor = new Color(0.85f,0.85f,0.85f)} };
        _box   = new GUIStyle(GUI.skin.box){
            normal = { background = _bgBox }, padding = new RectOffset(10,10,6,6),
            margin = new RectOffset(4,4,4,4), border = new RectOffset(0,0,0,0)
        };

        // 扁平输入框：无描边，纯色底
        _textFlat = new GUIStyle(GUI.skin.textField){
            normal = { background = _bgInput, textColor = Color.white },
            focused = { background = _bgInput, textColor = Color.white },
            hover = { background = _bgInput, textColor = Color.white },
            active = { background = _bgInput, textColor = Color.white },
            border = new RectOffset(0,0,0,0),
            margin = new RectOffset(4,4,2,2),
            padding = new RectOffset(6,6,4,4),
            alignment = TextAnchor.MiddleLeft
        };

        _stylesReady = true;
    }

    /* ----------------- 工具 ----------------- */
    static Rect GetScreenRect(RectTransform rt)
    {
        if (rt == null) return new Rect(0,0,0,0);
        var canvas = rt.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

        var world = new Vector3[4];
        rt.GetWorldCorners(world);
        Vector3 p0 = RectTransformUtility.WorldToScreenPoint(cam, world[0]);
        Vector3 p2 = RectTransformUtility.WorldToScreenPoint(cam, world[2]);

        float x = Mathf.Min(p0.x, p2.x);
        float yMin = Mathf.Min(p0.y, p2.y);
        float width = Mathf.Abs(p2.x - p0.x);
        float height = Mathf.Abs(p2.y - p0.y);
        float yTop = yMin + height;
        float y = Screen.height - yTop;

        if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(width) || !float.IsFinite(height))
            return new Rect(0,0,0,0);

        return new Rect(x, y, width, height);
    }
}