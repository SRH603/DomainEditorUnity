using System.Reflection;
using UnityEngine;
using Battlehub.RTCommon; // IRTE.Undo

public class ChartInfoInfoPanel : MonoBehaviour
{
    [Header("把 IMGUI 面板画到这个 RectTransform 区域")]
    public RectTransform host;

    [Header("未设置 host 或尺寸无效时的兜底屏幕区域")]
    public Rect fallbackScreenRect = new Rect(40, 40, 880, 160);

    [Header("标题")]
    public string title = "Info";

    private GameData _gameData;
    private Info _info;

    // offset 的“缓冲值”（编辑框里改，按 Apply 才写入 + Undo）
    private string _offsetBuffer;

    // 反射到 Info.offset 字段，用于 Undo Begin/EndRecordValue
    private static readonly FieldInfo kInfoOffsetField =
        typeof(Info).GetField("offset", BindingFlags.Public | BindingFlags.Instance);

    // 样式
    private bool _stylesReady;
    private GUIStyle _h1, _label, _box;
    private Texture2D _bgBox;
    private bool _warnedOnce;

    // 外部裁剪 + 整窗滚动偏移
    [System.NonSerialized] public System.Nullable<Rect> clipViewport;
    [System.NonSerialized] public float scrollOffsetY;

    void OnEnable()  { var cm = ChartManager.Instance; if (cm != null) cm.OnBpmListChanged += OnExternalChanged; }
    void OnDisable() { var cm = ChartManager.Instance; if (cm != null) cm.OnBpmListChanged -= OnExternalChanged; }

    private void OnExternalChanged()
    {
        PullFromGameData();   // 重新把 info.designer / info.bpm / info.rating / info.offset 等灌回 UI
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

        // 1) 宿主矩形
        Rect hostRect = (host != null) ? GetScreenRect(host) : fallbackScreenRect;
        if (hostRect.width < 8f || hostRect.height < 8f)
        {
            hostRect = fallbackScreenRect;
            if (!_warnedOnce)
            {
                _warnedOnce = true;
                Debug.LogWarning($"[ChartInfo/Info] host 区域无效，已改用 fallback：{hostRect}");
            }
        }

        // 2) 与 viewport 求交
        Rect drawRect = hostRect;
        if (clipViewport.HasValue)
        {
            var v = clipViewport.Value;
            float x  = Mathf.Max(hostRect.xMin, v.xMin);
            float y  = Mathf.Max(hostRect.yMin, v.yMin);
            float x2 = Mathf.Min(hostRect.xMax, v.xMax);
            float y2 = Mathf.Min(hostRect.yMax, v.yMax);
            if (x2 <= x || y2 <= y) return;
            drawRect = new Rect(x, y, x2 - x, y2 - y);
        }

        GUI.BeginGroup(drawRect);
        {
            // 整窗滚动（与 BPM 同步）
            Rect localArea = new Rect(hostRect.x - drawRect.x,
                                      hostRect.y - drawRect.y - scrollOffsetY,
                                      hostRect.width, hostRect.height);
            GUILayout.BeginArea(localArea);

            GUILayout.BeginVertical(GUILayout.Width(hostRect.width));

            GUILayout.BeginHorizontal(_box);
            GUILayout.Label(title, _h1);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(_box);

            // designer（即时写回，不走 undo）
            LineString("Designer", ref _info.designer, 240);

            // bpm (string)（即时写回，不走 undo）
            LineString("BPM (string)", ref _info.bpm, 120);

            // rating (double)（即时写回，不走 undo）
            LineDouble("Rating", ref _info.rating, 120);

            // offset (float) —— 使用“缓冲 + Apply（含 Undo/Redo）”
            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset (sec)", _label, GUILayout.Width(120));
            _offsetBuffer = GUILayout.TextField(_offsetBuffer ?? "", GUILayout.Width(120));
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

                    // 触发刷新（复用 BPM 的刷新事件，方便下游都监听一个）
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

            GUILayout.EndVertical(); // _box

            GUILayout.EndVertical(); // root vbox
            GUILayout.EndArea();
        }
        GUI.EndGroup();
    }

    void PullFromGameData()
    {
        if (_gameData == null) return;
        if (_gameData.info == null) _gameData.info = new Info();
        _info = _gameData.info;
        _offsetBuffer = _info != null ? _info.offset.ToString("0.###") : "0";
    }

    void LineString(string label, ref string value, float fieldWidth)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, _label, GUILayout.Width(120));
        string nv = GUILayout.TextField(value ?? "", GUILayout.Width(fieldWidth));
        if (nv != value) value = nv; // 立即写入，不走 Undo
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    void LineDouble(string label, ref double value, float fieldWidth)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, _label, GUILayout.Width(120));
        string s = GUILayout.TextField(value.ToString("0.###"), GUILayout.Width(fieldWidth));
        if (double.TryParse(s, out var nv)) value = nv; // 立即写入
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    Texture2D Tex(Color c) { var t=new Texture2D(1,1); t.SetPixel(0,0,c); t.Apply(); return t; }

    void BuildStyles()
    {
        var colBox = new Color(0.18f,0.18f,0.18f,1);

        _bgBox = Tex(colBox);

        _h1 = new GUIStyle(GUI.skin.label){ fontSize=15, fontStyle=FontStyle.Bold, normal={ textColor = Color.white } };
        _label = new GUIStyle(GUI.skin.label){ normal={ textColor = new Color(0.85f,0.85f,0.85f)} };
        _box = new GUIStyle(GUI.skin.box){ normal = { background = _bgBox }, padding = new RectOffset(10,10,6,6), margin = new RectOffset(4,4,4,4), border = new RectOffset(0,0,0,0) };
        _stylesReady = true;
    }

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