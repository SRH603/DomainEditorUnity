using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 扁平风 IMGUI：居中卡片 + 大字号 + 扁平按钮
/// 依赖 DechHub：新建/打开/保存/另存为/替换音频/开始编辑（加载编辑器场景）
/// </summary>
public class DechRuntimeHubUI : MonoBehaviour
{
    [Header("可选：启动时给 Hub 指定一个 GameData（若 Hub 内未设置）")]
    public GameData initialGameData;

    [Header("是否显示内置 IMGUI 面板")]
    public bool showBuiltinPanel = true;

    [Header("编辑器场景加载设置")]
    public bool loadByBuildIndex = false;
    public string editorSceneName = "EditorScene";
    public int editorSceneBuildIndex = 0;
    public bool loadAdditive = false;

    [Header("外观（可在 Inspector 调整）")]
    public Font uiFont;
    [Range(12, 36)] public int baseFontSize = 20;
    public Color panelBg = new Color(0.12f, 0.12f, 0.12f, 0.96f);
    public Color textColor = Color.white;
    public Color buttonColor = new Color(0.22f, 0.55f, 0.98f, 1f);
    public Color buttonHover = new Color(0.26f, 0.60f, 1.00f, 1f);
    public Color buttonActive = new Color(0.16f, 0.45f, 0.88f, 1f);
    public Color buttonDisabled = new Color(0.35f, 0.35f, 0.35f, 1f);
    public Color buttonTextColor = Color.white;

    private string _status = "就绪。";

    // styles & textures
    Texture2D _texPanel, _texBtn, _texBtnHover, _texBtnActive, _texBtnDisabled;
    GUIStyle _panel, _header, _label, _button, _smallLabel;
    float _lastScale = -1f;

    void Awake()
    {
        // 确保 Hub 实例存在
        var hub = DechHub.Instance;

        // 如 Hub 里还没有 GameData，而你在此脚本上给了 initialGameData，则注入
        if (hub.GetGameData() == null && initialGameData != null)
            hub.AssignGameData(initialGameData);

        // 订阅事件
        hub.OnOpened += () =>
        {
            var p = hub.GetDechPath();
            var clip = hub.GetAudioClip();
            if (clip != null)
                _status = $"已打开：{p}\n音频：{clip.frequency}Hz / {clip.channels}ch / {clip.length:F1}s";
            else
                _status = $"已打开：{p}\n音频：<null>";
        };
        hub.OnSaved += () =>
        {
            _status = "保存成功。";
        };
        hub.OnError += (msg) =>
        {
            _status = msg ?? "发生错误。";
        };
    }

    // ====== 提供给外部（uGUI等）直接调用的公共方法 ======
    public void UI_NewDech() => DechHub.Instance.NewDech();
    public void UI_OpenDech() => DechHub.Instance.OpenDech();
    public void UI_Save() => DechHub.Instance.Save();
    public void UI_SaveAs() => DechHub.Instance.SaveAs();
    public void UI_SetAudioFromFile() => DechHub.Instance.SetAudioFromFile();

    public void UI_StartEditing()
    {
        var hub = DechHub.Instance;
        if (!hub.Session.IsOpen) { _status = "尚未打开任何 .dech，会话不存在，无法进入编辑器场景。"; return; }

        if (!loadByBuildIndex)
        {
            if (string.IsNullOrEmpty(editorSceneName)) { _status = "未设置编辑器场景名。"; return; }
            SceneManager.LoadScene(editorSceneName, loadAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        }
        else
        {
            if (editorSceneBuildIndex < 0) { _status = "编辑器场景 Build Index 非法。"; return; }
            SceneManager.LoadScene(editorSceneBuildIndex, loadAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        }
    }

    public GameData UI_GetGameData() => DechHub.Instance.GetGameData();
    public AudioClip UI_GetAudioClip() => DechHub.Instance.GetAudioClip();
    public string UI_GetDechPath() => DechHub.Instance.GetDechPath();

    // ====== 扁平风 IMGUI 面板 ======
    void OnGUI()
    {
        if (!showBuiltinPanel) return;

        float s = ComputeScale();       // 基于分辨率的缩放
        EnsureStyles(s);                // 初始化/更新样式

        // 居中卡片尺寸
        float w = Mathf.Min(880f * s, Screen.width - 40f);
        float h = Mathf.Min(420f * s, Screen.height - 40f);
        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;

        // 面板
        GUILayout.BeginArea(new Rect(x, y, w, h), _panel);
        GUILayout.Space(8f * s);
        GUILayout.Label("Domain Echoing — DECH Hub", _header);
        GUILayout.Space(8f * s);
        GUILayout.Label(_status, _label);
        GUILayout.Space(12f * s);

        // 第一行：新建 / 打开
        GUILayout.BeginHorizontal();
        DrawButton("新建 .dech（选择保存位置→选择音频）", _button, 44f * s, UI_NewDech, enabled: true);
        GUILayout.Space(10f * s);
        DrawButton("打开 .dech ...", _button, 44f * s, UI_OpenDech, enabled: true);
        GUILayout.EndHorizontal();

        GUILayout.Space(8f * s);

        // 第二行：保存 / 另存为 / 替换音频
        GUILayout.BeginHorizontal();
        DrawButton("保存（覆盖）", _button, 44f * s, UI_Save, enabled: true);
        GUILayout.Space(10f * s);
        DrawButton("另存为...", _button, 44f * s, UI_SaveAs, enabled: true);
        GUILayout.Space(10f * s);
        DrawButton("替换音频...", _button, 44f * s, UI_SetAudioFromFile, enabled: true);
        GUILayout.EndHorizontal();

        GUILayout.Space(10f * s);

        // 第三行：开始编辑（仅当 Session 打开时可用，禁用态通过 GUI.color 淡化）
        bool canEdit = DechHub.Instance.Session != null && DechHub.Instance.Session.IsOpen;
        DrawButton("开始编辑（加载编辑器场景）", _button, 48f * s, () =>
        {
            if (canEdit) UI_StartEditing();
        }, enabled: canEdit);

        GUILayout.Space(10f * s);

        // 底部信息
        var hub = DechHub.Instance;
        var path = hub.GetDechPath();
        var clip = hub.GetAudioClip();
        GUILayout.Label("当前文件：" + (string.IsNullOrEmpty(path) ? "(未打开)" : path), _smallLabel);
        if (clip != null)
            GUILayout.Label($"音频：{clip.frequency}Hz / {clip.channels}ch / {clip.length:F1}s", _smallLabel);

        GUILayout.EndArea();
    }

    // —— 按钮绘制（处理禁用态的淡化 & 不触发点击）——
    void DrawButton(string text, GUIStyle style, float height, System.Action onClick, bool enabled)
    {
        Color old = GUI.color;

        if (!enabled)
        {
            // 背景改成“禁用色”，前景整体降透明
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            // 用禁用背景
            var oldBg = style.normal.background;
            style.normal.background = _texBtnDisabled;
            bool clicked = GUILayout.Button(text, style, GUILayout.Height(height));
            style.normal.background = oldBg; // 还原
            GUI.color = old;
            return;
        }

        bool ok = GUILayout.Button(text, style, GUILayout.Height(height));
        GUI.color = old;
        if (ok) onClick?.Invoke();
    }

    // ====== 样式 & 帮助方法 ======
    float ComputeScale()
    {
        // 以 1080p 为基准，自适应缩放
        float s1 = Mathf.Clamp(Screen.width / 1280f, 0.85f, 1.6f);
        float s2 = Mathf.Clamp(Screen.height / 720f, 0.85f, 1.6f);
        return Mathf.Min(s1, s2);
    }

    void EnsureStyles(float s)
    {
        if (_panel != null && Mathf.Abs(_lastScale - s) < 0.001f) return;
        _lastScale = s;

        // 贴图
        _texPanel       = MakeTex(panelBg);
        _texBtn         = MakeTex(buttonColor);
        _texBtnHover    = MakeTex(buttonHover);
        _texBtnActive   = MakeTex(buttonActive);
        _texBtnDisabled = MakeTex(buttonDisabled);

        // 面板样式（扁平卡片）
        _panel = new GUIStyle(GUIStyle.none);
        _panel.normal.background = _texPanel;
        _panel.padding = new RectOffset((int)(18 * s), (int)(18 * s), (int)(18 * s), (int)(18 * s));
        _panel.margin  = new RectOffset(0, 0, 0, 0);

        // 标题
        _header = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = Mathf.RoundToInt(baseFontSize * 1.3f * s),
            fontStyle = FontStyle.Bold,
            wordWrap = false
        };
        _header.normal.textColor = textColor;
        if (uiFont != null) _header.font = uiFont;

        // 普通文本
        _label = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = Mathf.RoundToInt(baseFontSize * s),
            wordWrap = true
        };
        _label.normal.textColor = textColor;
        if (uiFont != null) _label.font = uiFont;

        // 较小文本（底部信息）
        _smallLabel = new GUIStyle(_label)
        {
            fontSize = Mathf.RoundToInt(baseFontSize * 0.9f * s),
        };
        _smallLabel.normal.textColor = new Color(textColor.r, textColor.g, textColor.b, 0.9f);
        if (uiFont != null) _smallLabel.font = uiFont;

        // 扁平按钮
        _button = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(baseFontSize * s),
            border = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset((int)(14 * s), (int)(14 * s), (int)(10 * s), (int)(10 * s)),
            margin  = new RectOffset(0, 0, (int)(4 * s), (int)(4 * s))
        };
        if (uiFont != null) _button.font = uiFont;

        _button.normal.background   = _texBtn;
        _button.hover.background    = _texBtnHover;
        _button.active.background   = _texBtnActive;
        _button.focused.background  = _texBtn;
        _button.onNormal.background = _texBtn;
        _button.onHover.background  = _texBtnHover;
        _button.onActive.background = _texBtnActive;

        _button.normal.textColor   = buttonTextColor;
        _button.hover.textColor    = buttonTextColor;
        _button.active.textColor   = buttonTextColor;
        _button.focused.textColor  = buttonTextColor;
        _button.onNormal.textColor = buttonTextColor;
    }

    Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        t.SetPixel(0, 0, c);
        t.Apply();
        t.hideFlags = HideFlags.HideAndDontSave;
        return t;
    }
}
