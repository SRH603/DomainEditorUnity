using System;
using System.IO;
using UnityEngine;

public class DechHub : MonoBehaviour
{
    static DechHub _inst;
    public static DechHub Instance {
        get {
            if (_inst == null) {
                var go = new GameObject("DechHub");
                _inst = go.AddComponent<DechHub>();
                DontDestroyOnLoad(go);
            }
            return _inst;
        }
    }

    public DechSession Session { get; private set; } = new DechSession();

    [SerializeField] GameData initialGameData;
    public GameData TargetGameData { get; private set; }

    string _lastDir; const string LastDirKey = "DECH_LAST_DIR";

    public event Action OnOpened;
    public event Action OnSaved;
    public event Action<string> OnError;

    void Awake()
    {
        if (_inst != null && _inst != this) { Destroy(gameObject); return; }
        _inst = this; DontDestroyOnLoad(gameObject);

        _lastDir = PlayerPrefs.GetString(LastDirKey, "");
        TargetGameData = initialGameData ?? ScriptableObject.CreateInstance<GameData>();

        Session.OnLoaded += (so, clip) => { OnOpened?.Invoke(); };
        Session.OnExternalDeleteOrMove += (msg) => { Debug.LogWarning("[DECH] " + msg); };
    }

    void OnDestroy()
    {
        PlayerPrefs.SetString(LastDirKey, _lastDir ?? "");
        PlayerPrefs.Save();
        Session.Close();
        if (TargetGameData != null && initialGameData == null) Destroy(TargetGameData);
    }

    // —— 打开（mac 上对话框不筛选，这里强制校验 .dech）——
    public void OpenDech()
    {
        var path = NativeFileDialogs.OpenDech("Open DECH", _lastDir);
        if (string.IsNullOrEmpty(path)) { OnError?.Invoke("已取消。"); return; }

        if (!path.EndsWith(".dech", StringComparison.OrdinalIgnoreCase))
        {
            OnError?.Invoke("导入失败：仅支持 .dech 文件。");
            return;
        }

        _lastDir = Path.GetDirectoryName(path);
        try { Session.OpenAsync(this, path, TargetGameData); }
        catch (IOException ioex) { OnError?.Invoke("打开失败（文件被占用或权限不足）： " + ioex.Message); }
        catch (Exception ex)     { OnError?.Invoke("打开失败：" + ex.Message); }
    }

    public void NewDech()
    {
        var savePath = NativeFileDialogs.SaveDech("New DECH", _lastDir, "chart");
        if (string.IsNullOrEmpty(savePath)) { OnError?.Invoke("已取消。"); return; }

        var audioPath = NativeFileDialogs.OpenAudio("Pick Audio", Path.GetDirectoryName(savePath));
        if (string.IsNullOrEmpty(audioPath)) { OnError?.Invoke("导入失败：未选择音频。"); return; }

        _lastDir = Path.GetDirectoryName(savePath);
        try { Session.NewAsync(this, savePath, audioPath, TargetGameData); }
        catch (IOException ioex) { OnError?.Invoke("新建失败（文件被占用或权限受限）： " + ioex.Message); }
        catch (Exception ex)     { OnError?.Invoke("新建失败：" + ex.Message); }
    }

    public void Save()
    {
        if (!Session.IsOpen) { OnError?.Invoke("未打开会话。"); return; }
        try { if (Session.Save()) OnSaved?.Invoke(); }
        catch (Exception ex) { OnError?.Invoke("保存失败：" + ex.Message); }
    }

    public void SaveAs()
    {
        if (!Session.IsOpen) { OnError?.Invoke("未打开会话。"); return; }
        var newPath = NativeFileDialogs.SaveDech("Save As", Path.GetDirectoryName(Session.DechPath), Path.GetFileNameWithoutExtension(Session.DechPath));
        if (string.IsNullOrEmpty(newPath)) { OnError?.Invoke("已取消。"); return; }
        try { if (Session.SaveAs(newPath)) OnSaved?.Invoke(); }
        catch (Exception ex) { OnError?.Invoke("另存失败：" + ex.Message); }
    }

    public void SetAudioFromFile()
    {
        if (!Session.IsOpen) { OnError?.Invoke("未打开会话。"); return; }
        var audioPath = NativeFileDialogs.OpenAudio("Pick New Audio", Path.GetDirectoryName(Session.DechPath));
        if (string.IsNullOrEmpty(audioPath)) { OnError?.Invoke("已取消。"); return; }

        Session.SetAudioFromFileAsync(this, audioPath,
            onOk: () => { Debug.Log("[DECH] 新音频载入成功（尚未保存）。"); },
            onErr: ex => { OnError?.Invoke("设置音频失败：" + ex.Message); }
        );
    }

    public GameData GetGameData()  => TargetGameData;
    public AudioClip GetAudioClip() => Session.LoadedAudio;
    public string GetDechPath()     => Session.DechPath;

    public void AssignGameData(GameData so) { TargetGameData = so; }
}
