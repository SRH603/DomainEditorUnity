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

    // 紧凑样式参数
    [Header("外观/紧凑度")]
    public float rowHeight = 20f;   // 单行更紧凑
    public int   rowPadV   = 1;
    public bool  arrowOnly = true;  // 仅显示 ▲/▼

    // —— 运行态 —— 
    private GameData _gameData;                            // 当前会话 GameData
    private List<BPMList> _buffer = new List<BPMList>();   // 工作区副本（仅保存时回写）
    private List<RowEdit> _edits  = new List<RowEdit>();   // 每行的文本缓冲（允许为空）
    private Vector2 _scroll;
    private bool _dirty;
    private bool _warnedOnce;

    // 扁平化样式
    private bool _stylesReady;
    private GUIStyle _h1, _toolbar, _elementBox, _fieldLabel, _btn, _btnWarn, _saveBtn, _tagDirty, _tf;
    private Texture2D _bgPanel, _bgItem, _bgBtn, _bgBtnHover, _bgText;

    // —— 行编辑缓冲 —— 
    private class RowEdit
    {
        public string sx, sy, sz;   // startBeat x/y/z 缓冲
        public string sbpm;         // bpm 缓冲
        public RowEdit() { }
        public RowEdit(BPMList e)
        {
            sx   = e.startBeat.x.ToString();
            sy   = e.startBeat.y.ToString();
            sz   = e.startBeat.z.ToString();
            sbpm = e.bpm.ToString("0.###");
        }
    }

    void Awake()
    {
        TryFetchGameDataFromChartManager();
        if (_gameData == null)
        {
            Debug.LogWarning("[ChartInfo/BPM] 未找到 GameData，面板显示空。");
        }
        else
        {
            PullFromGameData(); // 首次拉取副本 + 缓冲
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
        // 外部变更 / 撤销重做后：重新拉取，清理脏标记
        PullFromGameData();
        _dirty = false;
        RepaintDock();
    }

    void OnGUI()
    {
        if (!_stylesReady) BuildStyles();

        Rect drawRect;
        if (host != null)
        {
            drawRect = GetScreenRect(host);
            if (drawRect.width < 8f || drawRect.height < 8f)
            {
                drawRect = fallbackScreenRect;
                if (!_warnedOnce)
                {
                    _warnedOnce = true;
                    Debug.LogWarning($"[ChartInfo/BPM] host 区域无效，已改用 fallback：{drawRect}");
                }
            }
        }
        else drawRect = fallbackScreenRect;

        GUI.BeginGroup(drawRect);
        {
            GUILayout.BeginVertical(GUILayout.Width(drawRect.width));
            HeaderBar();
            GUILayout.Space(4);

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

        if (GUILayout.Button("+", _btn, GUILayout.Width(30), GUILayout.Height(24)))
        {
            AddNew();
        }
        if (GUILayout.Button("↕", _btn, GUILayout.Width(30), GUILayout.Height(24)))
        {
            SortByBeatSync();
            _dirty = true;
        }

        GUILayout.Space(6);
        GUI.enabled = _dirty;
        if (GUILayout.Button("保存", _saveBtn, GUILayout.Width(78), GUILayout.Height(24)))
        {
            SaveWithUndoAndBroadcast();
        }
        GUI.enabled = true;

        if (GUILayout.Button("撤销改动", _btnWarn, GUILayout.Width(86), GUILayout.Height(24)))
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
        var edit = _edits[index];

        GUILayout.BeginVertical(_elementBox, GUILayout.MinHeight(rowHeight + rowPadV * 2));
        {
            GUILayout.BeginHorizontal();

            // 左侧：拍与 BPM 字段
            GUILayout.Label($"[{index}]", _fieldLabel, GUILayout.Width(42));

            GUILayout.Label("Beat", _fieldLabel, GUILayout.Width(36));

            // x
            string cx = $"sx_{index}";
            GUI.SetNextControlName(cx);
            edit.sx = GUILayout.TextField(edit.sx ?? "", _tf, GUILayout.Width(46), GUILayout.MinHeight(rowHeight));
            CommitIntIfValidAndUnfocused(cx, edit.sx, e.startBeat.x, (nv) =>
            {
                if (nv != e.startBeat.x)
                {
                    e.startBeat = new Vector3Int(nv, e.startBeat.y, e.startBeat.z);
                    _dirty = true;
                }
            },
            // 失焦空 → 回退到旧值
            onEmptyLoseFocus: () => edit.sx = e.startBeat.x.ToString());

            // y
            GUILayout.Label("/", _fieldLabel, GUILayout.Width(12));
            string cy = $"sy_{index}";
            GUI.SetNextControlName(cy);
            edit.sy = GUILayout.TextField(edit.sy ?? "", _tf, GUILayout.Width(46), GUILayout.MinHeight(rowHeight));
            CommitIntIfValidAndUnfocused(cy, edit.sy, e.startBeat.y, (nv) =>
            {
                if (nv != e.startBeat.y)
                {
                    e.startBeat = new Vector3Int(e.startBeat.x, nv, e.startBeat.z);
                    _dirty = true;
                }
            },
            onEmptyLoseFocus: () => edit.sy = e.startBeat.y.ToString());

            // z
            GUILayout.Label("/", _fieldLabel, GUILayout.Width(12));
            string cz = $"sz_{index}";
            GUI.SetNextControlName(cz);
            edit.sz = GUILayout.TextField(edit.sz ?? "", _tf, GUILayout.Width(46), GUILayout.MinHeight(rowHeight));
            CommitIntIfValidAndUnfocused(cz, edit.sz, e.startBeat.z, (nv) =>
            {
                if (nv != e.startBeat.z)
                {
                    e.startBeat = new Vector3Int(e.startBeat.x, e.startBeat.y, nv);
                    _dirty = true;
                }
            },
            onEmptyLoseFocus: () => edit.sz = e.startBeat.z.ToString());

            GUILayout.Space(10);

            // bpm
            GUILayout.Label("BPM", _fieldLabel, GUILayout.Width(34));
            string cbpm = $"bpm_{index}";
            GUI.SetNextControlName(cbpm);
            edit.sbpm = GUILayout.TextField(edit.sbpm ?? "", _tf, GUILayout.Width(70), GUILayout.MinHeight(rowHeight));
            CommitFloatIfValidAndUnfocused(cbpm, edit.sbpm, e.bpm, (nv) =>
            {
                if (!Mathf.Approximately(nv, e.bpm))
                {
                    e.bpm = nv;
                    _dirty = true;
                }
            },
            onEmptyLoseFocus: () => edit.sbpm = e.bpm.ToString("0.###"));

            GUILayout.FlexibleSpace();

            // 右侧：操作（只保留箭头图标）
            GUI.enabled = (index > 0);
            if (GUILayout.Button(new GUIContent("▲"), _btn, GUILayout.Width(26), GUILayout.Height(24)))
            {
                Swap(_buffer, index, index - 1);
                Swap(_edits,  index, index - 1);
                _dirty = true;
            }
            GUI.enabled = (index < _buffer.Count - 1);
            if (GUILayout.Button(new GUIContent("▼"), _btn, GUILayout.Width(26), GUILayout.Height(24)))
            {
                Swap(_buffer, index, index + 1);
                Swap(_edits,  index, index + 1);
                _dirty = true;
            }
            GUI.enabled = true;

            if (GUILayout.Button("⧉", _btn, GUILayout.Width(26), GUILayout.Height(24))) // 复制
            {
                var ne = Clone(e);
                var nb = new RowEdit(ne);
                _buffer.Insert(index + 1, ne);
                _edits.Insert(index + 1, nb);
                _dirty = true;
            }
            if (GUILayout.Button("⊼", _btn, GUILayout.Width(26), GUILayout.Height(24))) // 插上
            {
                var ne = Clone(e);
                var nb = new RowEdit(ne);
                _buffer.Insert(index, ne);
                _edits.Insert(index, nb);
                _dirty = true;
            }
            if (GUILayout.Button("⊻", _btn, GUILayout.Width(26), GUILayout.Height(24))) // 插下
            {
                var ne = Clone(e);
                var nb = new RowEdit(ne);
                _buffer.Insert(index + 1, ne);
                _edits.Insert(index + 1, nb);
                _dirty = true;
            }

            bool canDelete = _buffer.Count > 1;
            GUI.enabled = canDelete;
            if (GUILayout.Button("✕", _btnWarn, GUILayout.Width(26), GUILayout.Height(24)))
            {
                if (canDelete)
                {
                    _buffer.RemoveAt(index);
                    _edits.RemoveAt(index);
                    _dirty = true;
                }
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.Space(2);
    }

    // —— 保存 —— 
    void SaveWithUndoAndBroadcast()
    {
        if (_gameData == null) { Debug.LogWarning("[ChartInfo/BPM] 无 GameData，保存跳过。"); return; }
        if (_gameData.content == null) _gameData.content = new Content();

        var rte  = Battlehub.RTCommon.IOC.IsRegistered<IRTE>() ? Battlehub.RTCommon.IOC.Resolve<IRTE>() : null;
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

        // 广播：重建谱面/网格等
        var cm = ChartManager.Instance;
        if (cm != null) cm.NotifyBpmListChanged();

        try { DechHub.Instance?.Save(); } catch {}

        _dirty = false;
        Debug.Log("[ChartInfo/BPM] 保存成功（含撤销/重做）。");
    }

    // —— 数据与工具 —— 
    void PullFromGameData()
    {
        _buffer.Clear();
        _edits.Clear();

        if (_gameData != null && _gameData.content != null && _gameData.content.bpmList != null && _gameData.content.bpmList.Length > 0)
        {
            foreach (var e in _gameData.content.bpmList)
            {
                var ne = Clone(e);
                _buffer.Add(ne);
                _edits.Add(new RowEdit(ne));
            }
        }
        else
        {
            var def = new BPMList { startBeat = new Vector3Int(0, 0, 0), bpm = 200f };
            _buffer.Add(def);
            _edits.Add(new RowEdit(def));
        }
    }

    void AddNew()
    {
        if (_buffer.Count == 0)
        {
            var def = new BPMList { startBeat = new Vector3Int(0,0,0), bpm = 200f };
            _buffer.Add(def);
            _edits.Add(new RowEdit(def));
        }
        else
        {
            var last = _buffer[_buffer.Count - 1];
            var ne   = new BPMList { startBeat = last.startBeat, bpm = last.bpm };
            _buffer.Add(ne);
            _edits.Add(new RowEdit(ne));
        }
        _dirty = true;
    }

    static BPMList Clone(BPMList src)
    {
        if (src == null) return new BPMList { startBeat = new Vector3Int(0,0,0), bpm = 200f };
        return new BPMList { startBeat = src.startBeat, bpm = src.bpm };
    }

    static BPMList[] CloneArray(List<BPMList> list)
    {
        var arr = new BPMList[list.Count];
        for (int i = 0; i < list.Count; i++) arr[i] = Clone(list[i]);
        return arr;
    }

    void SortByBeatSync()
    {
        var idx = new List<int>(_buffer.Count);
        for (int i = 0; i < _buffer.Count; i++) idx.Add(i);

        idx.Sort((ia, ib) =>
        {
            var a = _buffer[ia]; var b = _buffer[ib];
            int ax = a.startBeat.x.CompareTo(b.startBeat.x); if (ax != 0) return ax;
            int ay = a.startBeat.y.CompareTo(b.startBeat.y); if (ay != 0) return ay;
            return a.startBeat.z.CompareTo(b.startBeat.z);
        });

        var newBuf = new List<BPMList>(_buffer.Count);
        var newEdt = new List<RowEdit>(_edits.Count);
        for (int k = 0; k < idx.Count; k++)
        {
            newBuf.Add(_buffer[idx[k]]);
            newEdt.Add(_edits[idx[k]]);
        }
        _buffer = newBuf;
        _edits  = newEdt;
    }

    static void Swap<T>(List<T> list, int i, int j)
    {
        if (i < 0 || j < 0 || i >= list.Count || j >= list.Count) return;
        (list[i], list[j]) = (list[j], list[i]);
    }

    // —— 输入框：允许空 + 失焦回退 —— 
    void CommitIntIfValidAndUnfocused(string controlName, string buf, int oldValue, Action<int> onValid, Action onEmptyLoseFocus)
    {
        var focused = GUI.GetNameOfFocusedControl() == controlName;

        if (!focused)
        {
            if (string.IsNullOrEmpty(buf))
            {
                onEmptyLoseFocus?.Invoke(); // 回退为旧值文本
            }
            else if (int.TryParse(buf, out var nv))
            {
                onValid?.Invoke(nv);        // 成功解析 → 写入
            }
            // 解析失败：保持原样，不写入，也不覆盖缓冲，留给用户继续改
        }
    }

    void CommitFloatIfValidAndUnfocused(string controlName, string buf, float oldValue, Action<float> onValid, Action onEmptyLoseFocus)
    {
        var focused = GUI.GetNameOfFocusedControl() == controlName;

        if (!focused)
        {
            if (string.IsNullOrEmpty(buf))
            {
                onEmptyLoseFocus?.Invoke(); // 回退
            }
            else if (float.TryParse(buf, out var nv))
            {
                onValid?.Invoke(nv);
            }
        }
    }

    // —— 扁平化样式 —— 
    Texture2D Tex(Color c)
    {
        var t = new Texture2D(1,1); t.SetPixel(0,0,c); t.Apply(); return t;
    }

    void BuildStyles()
    {
        // 配色：近 Unity 深灰 UI
        var colPanel = new Color(0.18f,0.18f,0.18f,1);
        var colItem  = new Color(0.21f,0.21f,0.21f,1);
        var colBtn   = new Color(0.24f,0.24f,0.24f,1);
        var colHover = new Color(0.30f,0.30f,0.30f,1);
        var colText  = new Color(0.16f,0.16f,0.16f,1); // 输入框背景

        _bgPanel    = Tex(colPanel);
        _bgItem     = Tex(colItem);
        _bgBtn      = Tex(colBtn);
        _bgBtnHover = Tex(colHover);
        _bgText     = Tex(colText);

        _h1 = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } };

        _toolbar = new GUIStyle(GUI.skin.box)
        {
            normal = { background = _bgPanel },
            padding = new RectOffset(10,10,6,6),
            margin  = new RectOffset(4,4,4,6),
            border  = new RectOffset(0,0,0,0)
        };

        _elementBox = new GUIStyle(GUI.skin.box)
        {
            normal = { background = _bgItem },
            padding = new RectOffset(6,6,rowPadV,rowPadV),
            margin  = new RectOffset(6,6,3,3),
            border  = new RectOffset(0,0,0,0)
        };

        _fieldLabel = new GUIStyle(GUI.skin.label) { normal = { textColor = new Color(0.85f,0.85f,0.85f) }, alignment = TextAnchor.MiddleLeft };

        _btn = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { background = _bgBtn, textColor = Color.white },
            hover  = { background = _bgBtnHover, textColor = Color.white },
            active = { background = _bgBtnHover, textColor = Color.white },
            padding = new RectOffset(4,4,2,2),
            border  = new RectOffset(0,0,0,0),
            margin  = new RectOffset(3,3,2,2)
        };

        _btnWarn = new GUIStyle(_btn) { normal = { textColor = new Color(1f,0.55f,0.55f) }, hover = { textColor = Color.white } };
        _saveBtn = new GUIStyle(_btn) { fontStyle = FontStyle.Bold };

        _tagDirty = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f,0.45f,0.45f) } };

        // —— 扁平输入框 —— 
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
