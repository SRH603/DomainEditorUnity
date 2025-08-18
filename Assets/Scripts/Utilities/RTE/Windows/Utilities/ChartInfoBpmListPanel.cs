using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Battlehub.RTCommon;

public class ChartInfoBpmListPanel : MonoBehaviour
{
    [Header("把 IMGUI 面板画到这个 RectTransform 区域（Dock 面板里的一个空容器）")]
    public RectTransform host;

    [Header("未设置 host 或尺寸无效时的兜底屏幕区域")]
    public Rect fallbackScreenRect = new Rect(40, 80, 880, 520);

    [Header("标题")]
    public string title = "BPM List";

    [Header("外观/紧凑度")]
    public float rowHeight = 22f;
    public int   rowPadV   = 2;
    public bool  arrowOnly = true;

    // —— 运行态 —— 
    private GameData _gameData;
    private readonly List<BPMList> _buffer = new List<BPMList>();
    private Vector2 _scroll;
    private bool _dirty;
    private bool _warnedOnce;

    // 扁平化样式
    private bool _stylesReady;
    private GUIStyle _h1, _toolbar, _elementBox, _fieldLabel, _btn, _btnWarn, _saveBtn, _tagDirty;
    private Texture2D _bgPanel, _bgItem, _bgBtn, _bgBtnHover;

    void Awake()
    {
        TryFetchGameDataFromChartManager();
        if (_gameData == null)
        {
            Debug.LogWarning("[ChartInfo/BPM] 未找到 GameData，面板显示空。");
        }
        else
        {
            PullFromGameData();
        }
    }

    void OnEnable()
    {
        var cm = ChartManager.Instance;
        if (cm != null) cm.OnBpmListChanged += OnExternalBpmChanged;
    }

    void OnDisable()
    {
        var cm = ChartManager.Instance;
        if (cm != null) cm.OnBpmListChanged -= OnExternalBpmChanged;
    }

    private void OnExternalBpmChanged()
    {
        PullFromGameData();
        _dirty = false;
        RepaintDock();
    }

    void OnGUI()
    {
        if (!_stylesReady) BuildStyles();

        Rect drawRect = (host != null) ? GetScreenRect(host) : fallbackScreenRect;
        if (drawRect.width < 8f || drawRect.height < 8f)
        {
            drawRect = fallbackScreenRect;
            if (!_warnedOnce)
            {
                _warnedOnce = true;
                Debug.LogWarning($"[ChartInfo/BPM] host 区域无效，已改用 fallback：{drawRect}");
            }
        }

        GUI.BeginGroup(drawRect);
        {
            GUILayout.BeginArea(new Rect(0, 0, drawRect.width, drawRect.height));
            GUILayout.BeginVertical(GUILayout.Width(drawRect.width));

            HeaderBar();
            GUILayout.Space(6);

            // 面板自己的滚动条
            _scroll = GUILayout.BeginScrollView(_scroll, false, true);
            {
                if (_buffer == null || _buffer.Count == 0)
                {
                    DrawHelpBox("列表为空。使用上方的 [+] 添加一条。");
                }
                else
                {
                    for (int i = 0; i < _buffer.Count; i++)
                        DrawElement(i);
                }
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        GUI.EndGroup();
    }

    // —— 顶栏 —— 
    void HeaderBar()
    {
        GUILayout.BeginHorizontal(_toolbar);
        GUILayout.Label(title, _h1);
        GUILayout.FlexibleSpace();

        if (_dirty)
        {
            GUILayout.Label("● 未保存", _tagDirty, GUILayout.Width(70));
        }

        if (GUILayout.Button("+", _btn, GUILayout.Width(34), GUILayout.Height(28))) AddNew();
        if (GUILayout.Button("↕", _btn, GUILayout.Width(34), GUILayout.Height(28)))
        {
            SortByBeat(_buffer);
            _dirty = true;
        }

        GUILayout.Space(8);
        GUI.enabled = _dirty;
        if (GUILayout.Button("保存", _saveBtn, GUILayout.Width(86), GUILayout.Height(28)))
        {
            SaveWithUndoAndBroadcast();
        }
        GUI.enabled = true;

        if (GUILayout.Button("撤销改动", _btnWarn, GUILayout.Width(96), GUILayout.Height(28)))
        {
            PullFromGameData();
            _dirty = false;
        }
        GUILayout.EndHorizontal();
    }

    // —— 单项 —— 
    void DrawElement(int index)
    {
        var e = _buffer[index];

        GUILayout.BeginVertical(_elementBox);
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label($"Elem {index}", _fieldLabel, GUILayout.Width(70));

            GUILayout.Label("startBeat", _fieldLabel, GUILayout.Width(70));
            int nx = IntField(e.startBeat.x, 52);
            int ny = IntField(e.startBeat.y, 52);
            int nz = IntField(e.startBeat.z, 52);
            if (nx != e.startBeat.x || ny != e.startBeat.y || nz != e.startBeat.z)
            {
                e.startBeat = new Vector3Int(nx, ny, nz);
                _dirty = true;
            }

            GUILayout.Space(12);
            GUILayout.Label("bpm", _fieldLabel, GUILayout.Width(30));
            float nbpm = FloatField(e.bpm, 70);
            if (!Mathf.Approximately(nbpm, e.bpm))
            {
                e.bpm = nbpm;
                _dirty = true;
            }

            GUILayout.FlexibleSpace();

            GUI.enabled = (index > 0);
            if (GUILayout.Button(new GUIContent("▲"), _btn, GUILayout.Width(32), GUILayout.Height(28)))
            { Swap(_buffer, index, index - 1); _dirty = true; }
            GUI.enabled = (index < _buffer.Count - 1);
            if (GUILayout.Button(new GUIContent("▼"), _btn, GUILayout.Width(32), GUILayout.Height(28)))
            { Swap(_buffer, index, index + 1); _dirty = true; }
            GUI.enabled = true;

            if (GUILayout.Button("⧉", _btn, GUILayout.Width(32), GUILayout.Height(28))) { _buffer.Insert(index + 1, Clone(e)); _dirty = true; }
            if (GUILayout.Button("⊼", _btn, GUILayout.Width(32), GUILayout.Height(28))) { _buffer.Insert(index,     Clone(e)); _dirty = true; }
            if (GUILayout.Button("⊻", _btn, GUILayout.Width(32), GUILayout.Height(28))) { _buffer.Insert(index + 1, Clone(e)); _dirty = true; }

            bool canDelete = _buffer.Count > 1;
            GUI.enabled = canDelete;
            if (GUILayout.Button("✕", _btnWarn, GUILayout.Width(32), GUILayout.Height(28)))
            {
                if (canDelete) { _buffer.RemoveAt(index); _dirty = true; }
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.Space(4);
    }

    // —— 保存 —— 
    void SaveWithUndoAndBroadcast()
    {
        if (_gameData == null) { Debug.LogWarning("[ChartInfo/BPM] 无 GameData，保存跳过。"); return; }
        if (_gameData.content == null) _gameData.content = new Content();

        var rte  = IOC.IsRegistered<IRTE>() ? IOC.Resolve<IRTE>() : null;
        var undo = rte != null ? rte.Undo : null;

        var content = _gameData.content;
        var member  = typeof(Content).GetField("bpmList", BindingFlags.Public | BindingFlags.Instance);
        var newArr  = CloneArray(_buffer);

        if (undo != null && member != null)
        {
            undo.BeginRecord();
            undo.BeginRecordValue(content, member);
            content.bpmList = newArr;
            undo.EndRecordValue(content, member);
            undo.EndRecord();
        }
        else
        {
            content.bpmList = newArr;
        }

        ChartManager.Instance?.NotifyBpmListChanged();
        try { DechHub.Instance?.Save(); } catch {}

        _dirty = false;
        Debug.Log("[ChartInfo/BPM] 保存成功（含撤销/重做）。");
    }

    // —— 数据与工具 —— 
    void PullFromGameData()
    {
        _buffer.Clear();
        if (_gameData != null && _gameData.content != null && _gameData.content.bpmList != null && _gameData.content.bpmList.Length > 0)
        {
            foreach (var e in _gameData.content.bpmList) _buffer.Add(Clone(e));
        }
        else
        {
            _buffer.Add(new BPMList { startBeat = new Vector3Int(0, 0, 0), bpm = 200f });
        }
    }

    void AddNew()
    {
        if (_buffer.Count == 0) _buffer.Add(new BPMList { startBeat = new Vector3Int(0,0,0), bpm = 200f });
        else
        {
            var last = _buffer[_buffer.Count - 1];
            _buffer.Add(new BPMList { startBeat = last.startBeat, bpm = last.bpm });
        }
        _dirty = true;
    }

    static BPMList Clone(BPMList src)
        => src == null ? new BPMList { startBeat = new Vector3Int(0,0,0), bpm = 200f }
                       : new BPMList { startBeat = src.startBeat, bpm = src.bpm };

    static BPMList[] CloneArray(List<BPMList> list)
    {
        var arr = new BPMList[list.Count];
        for (int i = 0; i < list.Count; i++) arr[i] = Clone(list[i]);
        return arr;
    }

    static void SortByBeat(List<BPMList> list)
    {
        list.Sort((a, b) =>
        {
            int ax = a.startBeat.x.CompareTo(b.startBeat.x); if (ax != 0) return ax;
            int ay = a.startBeat.y.CompareTo(b.startBeat.y); if (ay != 0) return ay;
            return a.startBeat.z.CompareTo(b.startBeat.z);
        });
    }

    static void Swap<T>(List<T> list, int i, int j)
    {
        if (i < 0 || j < 0 || i >= list.Count || j >= list.Count) return;
        (list[i], list[j]) = (list[j], list[i]);
    }

    int IntField(int v, float w)  { string s = GUILayout.TextField(v.ToString(), GUILayout.Width(w)); return int.TryParse(s, out var nv) ? nv : v; }
    float FloatField(float v, float w) { string s = GUILayout.TextField(v.ToString("0.###"), GUILayout.Width(w)); return float.TryParse(s, out var nv) ? nv : v; }

    Texture2D Tex(Color c) { var t = new Texture2D(1,1); t.SetPixel(0,0,c); t.Apply(); return t; }

    void BuildStyles()
    {
        var colPanel = new Color(0.18f,0.18f,0.18f,1);
        var colItem  = new Color(0.21f,0.21f,0.21f,1);
        var colBtn   = new Color(0.24f,0.24f,0.24f,1);
        var colHover = new Color(0.30f,0.30f,0.30f,1);

        _bgPanel    = Tex(colPanel);
        _bgItem     = Tex(colItem);
        _bgBtn      = Tex(colBtn);
        _bgBtnHover = Tex(colHover);

        _h1 = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } };
        _toolbar = new GUIStyle(GUI.skin.box){ normal = { background = _bgPanel }, padding = new RectOffset(10,10,8,8), margin = new RectOffset(4,4,4,6), border = new RectOffset(0,0,0,0) };
        _elementBox = new GUIStyle(GUI.skin.box){ normal = { background = _bgItem }, padding = new RectOffset(8,8,8,8), margin = new RectOffset(6,6,4,4), border = new RectOffset(0,0,0,0) };
        _fieldLabel = new GUIStyle(GUI.skin.label){ normal = { textColor = new Color(0.85f,0.85f,0.85f) } };
        _btn = new GUIStyle(GUI.skin.button){ fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
            normal = { background = _bgBtn, textColor = Color.white }, hover = { background = _bgBtnHover, textColor = Color.white },
            active = { background = _bgBtnHover, textColor = Color.white },
            padding = new RectOffset(6,6,4,4), border = new RectOffset(0,0,0,0), margin = new RectOffset(4,4,2,2)};
        _btnWarn = new GUIStyle(_btn) { normal = { textColor = new Color(1f,0.55f,0.55f) }, hover = { textColor = Color.white } };
        _saveBtn = new GUIStyle(_btn) { fontStyle = FontStyle.Bold };
        _tagDirty = new GUIStyle(GUI.skin.label){ fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f,0.45f,0.45f) } };
        _stylesReady = true;
    }

    void DrawHelpBox(string msg)
    {
        GUILayout.BeginVertical(_elementBox);
        GUILayout.Label(msg, _fieldLabel);
        GUILayout.EndVertical();
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

    void TryFetchGameDataFromChartManager()
    {
        var cm = ChartManager.Instance;
        if (cm != null && cm.gameData != null)
        {
            _gameData = cm.gameData;
            Debug.Log("[ChartInfo/BPM] 已从 ChartManager.Instance.gameData 自动获取 GameData。");
        }
    }

    void RepaintDock() { /* RTE 下会自动刷新 */ }
}