using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ChartInfoBpmListPanel : MonoBehaviour
{
    [Header("目标 SO（可空：若开启自动获取，会尝试从 ChartManager.Instance.gameData 拿）")]
    public GameData targetGameData;

    [Header("尝试从 ChartManager.Instance.gameData 自动获取")]
    public bool autoGetFromChartManager = true;

    [Header("绘制宿主（推荐拖你窗口里一个 Panel/RectTransform）")]
    public RectTransform host;               // 在此区域内绘制并裁剪

    [Header("备用：未指定 Host 时用这个屏幕矩形")]
    public Rect fallbackScreenRect = new Rect(24, 24, 560, 520);

    [Header("外观")]
    public string title = "Content / BPM List";
    public bool startFoldoutOpen = true;

    bool _foldout;
    Vector2 _scroll;

    // 样式（延迟到 OnGUI 首帧初始化）
    GUIStyle _headerStyle, _elementBoxStyle, _miniBtn, _fieldLabel, _arrayHeaderBox, _helpBox;
    bool _stylesReady = false;

    void Awake()
    {
        _foldout = startFoldoutOpen;

        if (targetGameData == null && autoGetFromChartManager)
        {
            TryFetchGameDataFromChartManager();
        }
        EnsureListExists();

        // ！！！不要在这里 BuildStyles()，会触发 GUI 调用时机异常
    }

    void EnsureListExists()
    {
        if (targetGameData == null) return;
        if (targetGameData.content == null)
            targetGameData.content = new Content();

        if (targetGameData.content.bpmList == null || targetGameData.content.bpmList.Length == 0)
        {
            targetGameData.content.bpmList = new BPMList[1] {
                new BPMList { startBeat = new Vector3Int(0,0,0), bpm = 200f }
            };
            Debug.Log("[ChartInfo/BPM] 初始化：空列表 → 创建默认一条（bpm=200, startBeat=(0,0,0)）");
        }
    }

    void BuildStyles()
    {
        // 这里必然在 OnGUI 内执行，访问 GUI.skin 是安全的
        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };

        _arrayHeaderBox = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(8, 8, 6, 6)
        };

        _elementBoxStyle = new GUIStyle(GUI.skin.box)
        {
            margin = new RectOffset(2, 2, 4, 6),
            padding = new RectOffset(8, 8, 6, 6)
        };

        _miniBtn = new GUIStyle(GUI.skin.button)
        {
            fontSize = 11,
            padding = new RectOffset(6, 6, 3, 3)
        };

        _fieldLabel = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12
        };

        // 运行时模拟 HelpBox
        _helpBox = new GUIStyle(GUI.skin.box)
        {
            wordWrap = true,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(8, 8, 8, 8)
        };

        _stylesReady = true;
    }

    bool _warnedOnce = false;
    void OnGUI()
    {
        if (!_stylesReady) BuildStyles();

        Rect drawRect;
        if (host != null)
        {
            drawRect = GetScreenRect(host);
            if (drawRect.width < 8f || drawRect.height < 8f)
            {
                // 区域太小/计算有误：用备用矩形并给出一次性日志
                drawRect = fallbackScreenRect;
                if (!_warnedOnce)
                {
                    _warnedOnce = true;
                    Debug.LogWarning($"[ChartInfo/BPM] host 的屏幕区域过小或无效，已改用 fallback 区域：{drawRect}。请检查：Canvas渲染模式、host锚点/尺寸、Canvas是否绑定相机。");
                }
            }
        }
        else
        {
            drawRect = fallbackScreenRect;
        }

        GUI.BeginGroup(drawRect);
        {
            var localRect = new Rect(0, 0, drawRect.width, drawRect.height);

            GUI.Box(localRect, "");
            GUILayout.BeginArea(localRect);

            if (targetGameData == null)
            {
                GUILayout.Label("Chart Info – BPM List", _headerStyle);
                GUILayout.Space(6);
                GUILayout.Label("未绑定 GameData。\n请为 targetGameData 指定对象，或开启 autoGetFromChartManager。", _helpBox);
                GUILayout.EndArea();
                GUI.EndGroup();
                return;
            }

            var content = targetGameData.content ?? (targetGameData.content = new Content());
            if (content.bpmList == null) content.bpmList = Array.Empty<BPMList>();
            if (content.bpmList.Length == 0) EnsureListExists();

            GUILayout.BeginVertical(_arrayHeaderBox);
            GUILayout.BeginHorizontal();
            _foldout = EditorLikeFoldout(_foldout, $"{title} ({content.bpmList.Length})", true);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Add", _miniBtn, GUILayout.Width(64))) AddNew(content);
            if (GUILayout.Button("Sort by Beat", _miniBtn, GUILayout.Width(96))) SortByBeat(content);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6);

            if (_foldout)
            {
                _scroll = GUILayout.BeginScrollView(_scroll);

                var list = new List<BPMList>(content.bpmList);
                for (int i = 0; i < list.Count; i++)
                {
                    DrawElement(list, i);
                }
                content.bpmList = list.ToArray();

                GUILayout.EndScrollView();
            }

            GUILayout.EndArea();
        }
        GUI.EndGroup();
    }

    void DrawElement(List<BPMList> list, int index)
    {
        var elem = list[index];

        GUILayout.BeginVertical(_elementBoxStyle);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Element {index}", _fieldLabel, GUILayout.Width(120));
        GUILayout.FlexibleSpace();

        GUI.enabled = index > 0;
        if (GUILayout.Button("▲", _miniBtn, GUILayout.Width(28))) { Swap(list, index, index - 1); Log($"上移：#{index}→#{index-1}"); }
        GUI.enabled = index < list.Count - 1;
        if (GUILayout.Button("▼", _miniBtn, GUILayout.Width(28))) { Swap(list, index, index + 1); Log($"下移：#{index}→#{index+1}"); }
        GUI.enabled = true;

        if (GUILayout.Button("Duplicate", _miniBtn, GUILayout.Width(80)))
        {
            var copy = new BPMList { startBeat = elem.startBeat, bpm = elem.bpm };
            list.Insert(index + 1, copy);
            Log($"复制：在 #{index} 后插入一条");
        }

        bool canDelete = list.Count > 1;
        GUI.enabled = canDelete;
        if (GUILayout.Button("Delete", _miniBtn, GUILayout.Width(64)))
        {
            list.RemoveAt(index);
            Log(canDelete ? $"删除：#{index}" : "删除阻止：至少保留一条");
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            return;
        }
        GUI.enabled = true;

        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        GUILayout.Label("startBeat", _fieldLabel, GUILayout.Width(80));

        int x = elem.startBeat.x;
        int y = elem.startBeat.y;
        int z = elem.startBeat.z;

        x = IntFieldMini("X", x, 60);
        y = IntFieldMini("Y", y, 60);
        z = IntFieldMini("Z", z, 60);

        elem.startBeat = new Vector3Int(x, y, z);

        GUILayout.Space(16);

        GUILayout.Label("bpm", _fieldLabel, GUILayout.Width(36));
        FloatFieldMini(ref elem.bpm, 80);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        list[index] = elem;

        GUILayout.EndVertical();
        GUILayout.Space(4);
    }

    bool EditorLikeFoldout(bool state, string label, bool bold)
    {
        var bg = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
            normal = { textColor = GUI.skin.label.normal.textColor },
            active = { textColor = GUI.skin.label.normal.textColor }
        };
        var icon = state ? "▾" : "▸";
        if (GUILayout.Button($"{icon}  {label}", bg))
        {
            state = !state;
        }
        return state;
    }

    static int IntFieldMini(string label, int value, float width)
    {
        GUILayout.Label(label, GUILayout.Width(16));
        string s = GUILayout.TextField(value.ToString(), GUILayout.Width(width));
        if (int.TryParse(s, out int v)) return v;
        return value;
    }

    static void FloatFieldMini(ref float value, float width)
    {
        string s = GUILayout.TextField(value.ToString("0.###"), GUILayout.Width(width));
        if (float.TryParse(s, out float v) && Math.Abs(v - value) > 1e-6f) value = v;
    }

    void AddNew(Content c)
    {
        var list = new List<BPMList>(c.bpmList);
        BPMList last = list.Count > 0 ? list[list.Count - 1] : new BPMList { startBeat = new Vector3Int(0,0,0), bpm = 200f };
        var added = new BPMList { startBeat = last.startBeat, bpm = last.bpm };
        list.Add(added);
        c.bpmList = list.ToArray();
        Log($"新增：#{list.Count - 1}（bpm={added.bpm}, startBeat={added.startBeat})");
    }

    void SortByBeat(Content c)
    {
        Array.Sort(c.bpmList, (a, b) =>
        {
            int ax = a.startBeat.x.CompareTo(b.startBeat.x);
            if (ax != 0) return ax;
            int ay = a.startBeat.y.CompareTo(b.startBeat.y);
            if (ay != 0) return ay;
            return a.startBeat.z.CompareTo(b.startBeat.z);
        });
        Log("按 startBeat 排序（X→Y→Z）");
    }

    static void Swap(List<BPMList> list, int i, int j)
    {
        var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
    }

    static void Log(string msg) => Debug.Log("[ChartInfo/BPM] " + msg);

    void TryFetchGameDataFromChartManager()
    {
        try
        {
            Type cmType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try { cmType = asm.GetType("ChartManager"); if (cmType != null) break; } catch { }
            }
            if (cmType == null) return;

            var instProp = cmType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var cm = instProp != null ? instProp.GetValue(null, null) : null;
            if (cm == null) return;

            GameData gd = null;
            var field = cmType.GetField("gameData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) gd = field.GetValue(cm) as GameData;

            if (gd == null)
            {
                var prop = cmType.GetProperty("gameData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null && prop.CanRead) gd = prop.GetValue(cm, null) as GameData;
            }

            if (gd != null)
            {
                targetGameData = gd;
                Debug.Log("[ChartInfo/BPM] 已从 ChartManager.Instance.gameData 自动获取 GameData。");
            }
        }
        catch { }
    }

    static Rect GetScreenRect(RectTransform rt)
    {
        if (rt == null) return new Rect(0, 0, 0, 0);

        // 找到所属 Canvas 与相机
        var canvas = rt.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            // Screen Space - Camera / World Space 下需要用 canvas.worldCamera
            cam = canvas.worldCamera;
            // 若没绑定，则退回主摄像机
            if (cam == null) cam = Camera.main;
        }

        // 先拿世界四角，再投到屏幕坐标
        var world = new Vector3[4];
        rt.GetWorldCorners(world);

        Vector3 p0 = RectTransformUtility.WorldToScreenPoint(cam, world[0]); // 左下（屏幕坐标系：y向上）
        Vector3 p2 = RectTransformUtility.WorldToScreenPoint(cam, world[2]); // 右上

        float x      = Mathf.Min(p0.x, p2.x);
        float yMin   = Mathf.Min(p0.y, p2.y);
        float width  = Mathf.Abs(p2.x - p0.x);
        float height = Mathf.Abs(p2.y - p0.y);

        // IMGUI 的 (0,0) 在左上，需要把 y 翻转
        float yTop = yMin + height;
        float y    = Screen.height - yTop;

        // 防御：NaN/Inf 直接返回 0 矩形
        if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(width) || !float.IsFinite(height))
            return new Rect(0, 0, 0, 0);

        return new Rect(x, y, width, height);
    }

}
