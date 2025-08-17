using System;
using System.IO;
using UnityEngine;

/// <summary>
/// DECH 中枢：统一处理 New / Open / Save / SaveAs / SetAudio。
/// - 事件：OnOpened / OnSaved / OnError
/// - 日志：所有操作（包含成功）都会输出 Debug 日志
/// </summary>
public class DechHub : MonoBehaviour
{
    // ====== 单例 ======
    static DechHub _inst;
    public static DechHub Instance
    {
        get
        {
            if (_inst == null)
            {
                var go = new GameObject("DechHub");
                _inst = go.AddComponent<DechHub>();
                DontDestroyOnLoad(go);
                _inst.LogInfo("实例已创建并常驻（DontDestroyOnLoad）。");
            }
            return _inst;
        }
    }

    // ====== 会话与数据 ======
    public DechSession Session { get; private set; } = new DechSession();

    [SerializeField] GameData initialGameData;
    public GameData TargetGameData { get; private set; }

    string _lastDir;
    const string LastDirKey = "DECH_LAST_DIR";

    // ====== 事件 ======
    public event Action OnOpened;
    public event Action OnSaved;
    public event Action<string> OnError;
    
    // ====== 生命周期 ======
    void Awake()
    {
        if (_inst != null && _inst != this) { Destroy(gameObject); return; }
        _inst = this;
        DontDestroyOnLoad(gameObject);

        _lastDir = PlayerPrefs.GetString(LastDirKey, "");
        TargetGameData = initialGameData ?? ScriptableObject.CreateInstance<GameData>();

        LogInfo($"Awake。恢复上次目录：{(_lastDir ?? "(null)")}");
        LogInfo($"初始 GameData：{(TargetGameData != null ? TargetGameData.name : "(null)")}");

        // Session 事件桥接 + 日志
        Session.OnLoaded += (so, clip) =>
        {
            var p = Session.DechPath;
            var clipInfo = clip != null ? $"{clip.frequency}Hz/{clip.channels}ch/{clip.length:F1}s" : "<null>";
            LogInfo($"打开成功：{p} | 音频：{clipInfo}");
            SafeInvokeOpened();
        };
        Session.OnExternalDeleteOrMove += (msg) =>
        {
            LogWarn($"外部文件变动：{msg}");
        };
    }

    void OnDestroy()
    {
        LogInfo("OnDestroy：保存首选项并关闭会话。");
        PlayerPrefs.SetString(LastDirKey, _lastDir ?? "");
        PlayerPrefs.Save();
        Session.Close();

        if (TargetGameData != null && initialGameData == null)
        {
            Destroy(TargetGameData);
            LogInfo("销毁运行时创建的临时 GameData。");
        }
    }

    // ====== 对外操作 ======

    /// <summary>打开 .dech（mac 上对话框不筛选，这里强制校验 .dech）</summary>
    public void OpenDech()
    {
        LogInfo($"准备打开 .dech（起始目录：{_lastDir}）");
        var path = NativeFileDialogs.OpenDech("Open DECH", _lastDir);

        if (string.IsNullOrEmpty(path))
        {
            RaiseError("打开已取消。");
            return;
        }
        if (!path.EndsWith(".dech", StringComparison.OrdinalIgnoreCase))
        {
            RaiseError("导入失败：仅支持 .dech 文件。");
            return;
        }

        _lastDir = Path.GetDirectoryName(path);
        LogInfo($"开始载入：{path}");
        try
        {
            Session.OpenAsync(this, path, TargetGameData);
        }
        catch (IOException ioex)
        {
            RaiseError("打开失败（文件被占用或权限不足）： " + ioex.Message);
        }
        catch (Exception ex)
        {
            RaiseError("打开失败：" + ex.Message);
        }
    }

    /// <summary>新建：选择保存位置→选择音频→写入默认谱面→打开</summary>
    public void NewDech()
    {
        LogInfo($"准备新建 .dech（起始目录：{_lastDir}）");
        var savePath = NativeFileDialogs.SaveDech("New DECH", _lastDir, "chart");
        if (string.IsNullOrEmpty(savePath))
        {
            RaiseError("新建已取消（未选择保存位置）。");
            return;
        }

        var audioPath = NativeFileDialogs.OpenAudio("Pick Audio", Path.GetDirectoryName(savePath));
        if (string.IsNullOrEmpty(audioPath))
        {
            RaiseError("导入失败：未选择音频。");
            return;
        }

        _lastDir = Path.GetDirectoryName(savePath);
        LogInfo($"开始新建：{savePath}（音频：{audioPath}）");

        try
        {
            Session.NewAsync(this, savePath, audioPath, TargetGameData);
            LogInfo("新建流程已提交（写入并打开）。");
        }
        catch (IOException ioex)
        {
            RaiseError("新建失败（文件被占用或权限受限）： " + ioex.Message);
        }
        catch (Exception ex)
        {
            RaiseError("新建失败：" + ex.Message);
        }
    }

    /// <summary>保存（覆盖当前 .dech）</summary>
    public void Save()
    {
        if (!Session.IsOpen)
        {
            RaiseError("未打开会话，无法保存。");
            return;
        }

        try
        {
            if (Session.Save())
            {
                LogInfo($"保存成功（覆盖）：{Session.DechPath}");
                SafeInvokeSaved();
            }
            else
            {
                RaiseError("保存失败：会话未打开。");
            }
        }
        catch (Exception ex)
        {
            RaiseError("保存失败：" + ex.Message);
        }
    }

    /// <summary>另存为（选择新的 .dech 路径；不改变当前会话路径）</summary>
    public void SaveAs()
    {
        if (!Session.IsOpen)
        {
            RaiseError("未打开会话，无法另存为。");
            return;
        }

        var newPath = NativeFileDialogs.SaveDech(
            "Save As",
            Path.GetDirectoryName(Session.DechPath),
            Path.GetFileNameWithoutExtension(Session.DechPath)
        );

        if (string.IsNullOrEmpty(newPath))
        {
            RaiseError("另存为已取消。");
            return;
        }

        try
        {
            if (Session.SaveAs(newPath))
            {
                LogInfo($"另存为成功：{newPath}（当前会话路径未改变：{Session.DechPath}）");
                SafeInvokeSaved();
            }
            else
            {
                RaiseError("另存为失败：会话未打开。");
            }
        }
        catch (Exception ex)
        {
            RaiseError("另存为失败：" + ex.Message);
        }
    }

    /// <summary>从文件替换音频（立即生效但不自动保存）</summary>
    public void SetAudioFromFile()
    {
        if (!Session.IsOpen)
        {
            RaiseError("未打开会话，无法替换音频。");
            return;
        }

        var audioPath = NativeFileDialogs.OpenAudio("Pick New Audio", Path.GetDirectoryName(Session.DechPath));
        if (string.IsNullOrEmpty(audioPath))
        {
            RaiseError("替换音频已取消。");
            return;
        }

        LogInfo($"准备载入新音频：{audioPath}");
        Session.SetAudioFromFileAsync(
            this,
            audioPath,
            onOk: () =>
            {
                LogInfo("新音频载入成功（尚未保存到 .dech）。");
            },
            onErr: ex =>
            {
                RaiseError("设置音频失败：" + ex.Message);
            }
        );
    }

    // ====== 查询 ======
    public GameData  GetGameData()  => TargetGameData;
    public AudioClip GetAudioClip() => Session.LoadedAudio;
    public string    GetDechPath()  => Session.DechPath;
    public void AssignGameData(GameData so)
    {
        TargetGameData = so;
        LogInfo($"AssignGameData：{(so != null ? so.name : "(null)")} 已注入到 Hub。");
    }

    // ====== 事件安全触发（同时写日志） ======
    void SafeInvokeOpened()
    {
        try { OnOpened?.Invoke(); }
        catch (Exception cbEx) { LogError("OnOpened 回调异常：" + cbEx.Message); }
    }

    void SafeInvokeSaved()
    {
        try { OnSaved?.Invoke(); }
        catch (Exception cbEx) { LogError("OnSaved 回调异常：" + cbEx.Message); }
    }

    void RaiseError(string msg)
    {
        LogError(msg);
        try { OnError?.Invoke(msg); }
        catch (Exception cbEx) { LogError("OnError 回调异常：" + cbEx.Message); }
    }

    // ====== 日志工具（统一前缀） ======
    void LogInfo(string msg)  => Debug.Log($"[DECH Hub] {msg}");
    void LogWarn(string msg)  => Debug.LogWarning($"[DECH Hub] {msg}");
    void LogError(string msg) => Debug.LogError($"[DECH Hub] {msg}");
}
