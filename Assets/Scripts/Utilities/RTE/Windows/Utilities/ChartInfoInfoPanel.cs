using System.Reflection;
using UnityEngine;
using Battlehub.RTCommon; // IRTE.Undo

public class ChartInfoInfoPanel : MonoBehaviour
{
    [Header("把 IMGUI 面板画到这个 RectTransform 区域")]
    public RectTransform host;

    [Header("未设置 host 或尺寸无效时的兜底屏幕区域")]
    public Rect fallbackScreenRect = new Rect(40, 40, 880, 200);

    [Header("标题")]
    public string title = "Info";

    private GameData _gameData;
    private Info _info;

    // —— 缓冲 —— 
    private string _offsetBuffer;          // Offset 仍使用 Apply
    private string _ratingBuffer;          // Rating 改为缓冲：允许空+失焦回退

    // 反射到 Info.offset 字段，用于 Undo Begin/EndRecordValue
    private static readonly FieldInfo kInfoOffsetField =
        typeof(Info).GetField("offset", BindingFlags.Public | BindingFlags.Instance);

    // 样式
    private bool _stylesReady;
    private GUIStyle _h1, _label, _box, _tf;
    private Texture2D _bgBox, _bgText;
    private bool _warnedOnce;

    void OnEnable()
    {
        var cm = ChartManager.Instance; 
        if (cm != null) cm.OnBpmListChanged += OnExternalChanged;
    }
    void OnDisable()
    {
        var cm = ChartManager.Instance; 
        if (cm != null) cm.OnBpmListChanged -= OnExternalChanged;
    }

    private void OnExternalChanged()
    {
        PullFromGameData();   // 重新把 info.* 灌回 UI（含 offset/rating 缓冲）
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

        PullFromGameData();
    }

    // —— 拉取最新 GameData/Info 到 UI 缓冲 —— 
    private void PullFromGameData()
    {
        if (_gameData == null)
        {
            var cm = ChartManager.Instance;
            if (cm != null) _gameData = cm.gameData;
        }
        if (_gameData == null)
        {
            Debug.LogWarning("[ChartInfo/Info] PullFromGameData 跳过：_gameData 为空。");
            return;
        }

        if (_gameData.info == null)
            _gameData.info = new Info();

        _info = _gameData.info;

        _offsetBuffer = _info.offset.ToString("0.###");
        _ratingBuffer = _info.rating.ToString("0.###");
        // 其它即时字段（designer、bpm(string)）直接用 _info.*，不需要缓冲
    }

    void OnGUI()
    {
        if (!_stylesReady) BuildStyles();

        Rect r;
        if (host != null)
        {
            r = GetScreenRect(host);
            if (r.width < 8f || r.height < 8f)
            {
                r = fallbackScreenRect;
                if (!_warnedOnce)
                {
                    _warnedOnce = true;
                    Debug.LogWarning($"[ChartInfo/Info] host 区域无效，已改用 fallback：{r}");
                }
            }
        }
        else r = fallbackScreenRect;

        GUI.BeginGroup(r);
        GUILayout.BeginVertical(GUILayout.Width(r.width));

        GUILayout.BeginHorizontal(_box);
        GUILayout.Label(title, _h1);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(_box);

        // designer（即时写回，允许为空）
        LineString("Designer", ref _info.designer, 260);

        // bpm (string)（即时写回，允许为空）
        LineString("BPM (string)", ref _info.bpm, 140);

        // rating (double) —— 缓冲：允许空 + 失焦回退 + 成功解析才写回
        LineDoubleBuffered("Rating", ref _ratingBuffer, 120,
            onValid: (nv) => _info.rating = nv);

        // offset (float) —— 使用“缓冲 + Apply（含 Undo/Redo）”，文本框扁平风格
        GUILayout.BeginHorizontal();
        GUILayout.Label("Offset (sec)", _label, GUILayout.Width(120));

        string ctrlOffset = "info_offset";
        GUI.SetNextControlName(ctrlOffset);
        _offsetBuffer = GUILayout.TextField(_offsetBuffer ?? "", _tf, GUILayout.Width(120));

        // 失焦为空 → 回退
        if (GUI.GetNameOfFocusedControl() != ctrlOffset && string.IsNullOrEmpty(_offsetBuffer))
        {
            _offsetBuffer = _info.offset.ToString("0.###");
        }

        // 旁边的 Apply
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

        GUILayout.EndVertical();

        GUILayout.EndVertical();
        GUI.EndGroup();
    }

    void LineString(string label, ref string value, float fieldWidth)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, _label, GUILayout.Width(120));
        string nv = GUILayout.TextField(value ?? "", _tf, GUILayout.Width(fieldWidth));
        if (nv != value) value = nv; // 立即写入（字符串允许为空）
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    void LineDoubleBuffered(string label, ref string buffer, float fieldWidth, System.Action<double> onValid)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, _label, GUILayout.Width(120));

        string ctrl = "info_" + label;
        GUI.SetNextControlName(ctrl);
        buffer = GUILayout.TextField(buffer ?? "", _tf, GUILayout.Width(fieldWidth));

        // 失焦时：空 → 回退；非空可解析 → 写回；否则保持缓冲（等待用户继续编辑）
        bool focused = GUI.GetNameOfFocusedControl() == ctrl;
        if (!focused)
        {
            if (string.IsNullOrEmpty(buffer))
            {
                // 回退为当前 info 上的值
                double cur = label == "Rating" ? _info.rating : 0.0;
                buffer = cur.ToString("0.###");
            }
            else if (double.TryParse(buffer, out var nv))
            {
                onValid?.Invoke(nv);
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    Texture2D Tex(Color c) { var t=new Texture2D(1,1); t.SetPixel(0,0,c); t.Apply(); return t; }

    void BuildStyles()
    {
        var colBox  = new Color(0.18f,0.18f,0.18f,1);
        var colText = new Color(0.16f,0.16f,0.16f,1);

        _bgBox  = Tex(colBox);
        _bgText = Tex(colText);

        _h1 = new GUIStyle(GUI.skin.label){ fontSize=15, fontStyle=FontStyle.Bold, normal={ textColor = Color.white } };
        _label = new GUIStyle(GUI.skin.label){ normal={ textColor = new Color(0.85f,0.85f,0.85f)} };
        _box = new GUIStyle(GUI.skin.box){
            normal = { background = _bgBox },
            padding = new RectOffset(10,10,6,6),
            margin  = new RectOffset(4,4,4,4),
            border  = new RectOffset(0,0,0,0)
        };

        // 扁平输入框
        _tf = new GUIStyle(GUI.skin.textField)
        {
            normal   = { background = _bgText,  textColor = Color.white },
            focused  = { background = _bgText,  textColor = Color.white },
            active   = { background = _bgText,  textColor = Color.white },
            hover    = { background = _bgText,  textColor = Color.white },
            border   = new RectOffset(0,0,0,0),
            margin   = new RectOffset(2,2,2,2),
            padding  = new RectOffset(4,4,2,2),
            alignment= TextAnchor.MiddleLeft,
            fontSize = 12
        };

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
