using System;
using System.IO;
using UnityEngine;

public class DechSession
{
    public string DechPath { get; private set; }
    public GameData TargetSO { get; private set; }
    public AudioClip LoadedAudio { get; private set; }

    // 保留原始音频字节及扩展名，保存时不丢失
    byte[] _audioBytes;
    string _audioExt;

    FileStream _lockStream;            // 共享读写锁（避免自我冲突）
    FileSystemWatcher _watcher;
    public bool IsOpen { get; private set; }

    public event Action<GameData, AudioClip> OnLoaded;
    public event Action<string> OnExternalDeleteOrMove;

    /// <summary>
    /// 打开 .dech：读取 -> 获取共享锁 -> 若中间被改动则在锁下重读 -> 解码音频
    /// </summary>
    public void OpenAsync(MonoBehaviour runner, string path, GameData soToFill)
    {
        if (runner == null) throw new ArgumentNullException(nameof(runner));
        if (string.IsNullOrEmpty(path)) throw new ArgumentException("path is null/empty.");

        Close(); // 先清理旧会话

        // --- 第一次读取（未加锁），记录文件状态 ---
        var fiBefore = new FileInfo(path);
        if (!fiBefore.Exists) throw new FileNotFoundException("file not found", path);

        var (dto, ext, audio) = DechContainer.ReadAndUnpack(path);

        // --- 获取共享锁（避免自我冲突；带重试）---
        _lockStream = AcquireSharedLock(path);  // 关键：共享锁而不是独占锁

        // --- 校验文件在读取->加锁之间是否被改动；若改动则重读 ---
        var fiAfter = new FileInfo(path);
        if (fiAfter.Exists &&
            (fiAfter.Length != fiBefore.Length || fiAfter.LastWriteTimeUtc != fiBefore.LastWriteTimeUtc))
        {
            (dto, ext, audio) = DechContainer.ReadAndUnpack(path);
        }

        // --- 填充状态 ---
        DechPath  = path;
        TargetSO  = soToFill;
        GameDataMapper.FromDTO(dto, TargetSO);
        _audioBytes = audio;
        _audioExt   = ext;

        // --- 解码音频（协程）---
        runner.StartCoroutine(DechAudioLoader.LoadFromBytes(_audioBytes, _audioExt,
            clip =>
            {
                LoadedAudio = clip;
                BeginWatch();
                IsOpen = true;
                OnLoaded?.Invoke(TargetSO, LoadedAudio);
            },
            ex => { throw ex; }
        ));
    }

    /// <summary>
    /// 新建：写文件 -> 打开（内部会获取共享锁）
    /// </summary>
    public void NewAsync(MonoBehaviour runner, string savePath, string audioPath, GameData soToFill)
    {
        if (runner == null) throw new ArgumentNullException(nameof(runner));
        if (string.IsNullOrEmpty(savePath)) throw new ArgumentException("savePath null/empty");
        if (string.IsNullOrEmpty(audioPath)) throw new ArgumentException("audioPath null/empty");

        // 1) 读取音频字节 & 扩展名
        byte[] audioBytes = File.ReadAllBytes(audioPath);
        string audioExt = Path.GetExtension(audioPath)?.TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrEmpty(audioExt)) audioExt = "wav";

        // 2) 用工厂创建“默认 GameData”（含：bpmList[0]=200、1条判定线）
        GameData tmp = null;
        try
        {
            tmp = DefaultGameDataFactory.CreateSO();

            // 3) SO -> DTO，再写入容器
            var dto = GameDataMapper.ToDTO(tmp);
            DechContainer.PackAndWrite(savePath, dto, audioExt, audioBytes);
        }
        finally
        {
            // 清理临时 SO
            if (tmp != null) UnityEngine.Object.Destroy(tmp);
        }

        // 4) 立刻打开新建的 .dech（会填充到 soToFill、解码音频、加文件监控）
        OpenAsync(runner, savePath, soToFill);
    }


    /// <summary>
    /// 保存（原地覆盖，原子替换）
    /// </summary>
    public bool Save()
    {
        if (!IsOpen) return false;

        var dto = GameDataMapper.ToDTO(TargetSO);
        string tmp = DechPath + ".tmp";
        string bak = DechPath + ".bak";

        // 暂时释放锁（否则 Replace 在部分平台会冲突）
        _lockStream?.Dispose(); _lockStream = null;

        try
        {
            DechContainer.PackAndWrite(tmp, dto, _audioExt, _audioBytes);

            if (File.Exists(bak)) File.Delete(bak);
            File.Replace(tmp, DechPath, bak, ignoreMetadataErrors: true);

            if (File.Exists(tmp)) File.Delete(tmp);
            if (File.Exists(bak)) File.Delete(bak);
            return true;
        }
        finally
        {
            // 重新获取共享锁
            _lockStream = AcquireSharedLock(DechPath);
        }
    }

    /// <summary>
    /// 另存为（不改变当前会话路径/锁）
    /// </summary>
    public bool SaveAs(string newPath)
    {
        if (!IsOpen) return false;
        var dto = GameDataMapper.ToDTO(TargetSO);
        DechContainer.PackAndWrite(newPath, dto, _audioExt, _audioBytes);
        return true;
    }

    /// <summary>
    /// 从文件替换音频（不自动保存）
    /// </summary>
    public void SetAudioFromFileAsync(MonoBehaviour runner, string audioPath, Action onOk, Action<Exception> onErr)
    {
        if (runner == null) { onErr?.Invoke(new ArgumentNullException(nameof(runner))); return; }
        try
        {
            var bytes = File.ReadAllBytes(audioPath);
            var ext = Path.GetExtension(audioPath)?.TrimStart('.').ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)) ext = "wav";

            runner.StartCoroutine(DechAudioLoader.LoadFromBytes(bytes, ext,
                clip =>
                {
                    _audioBytes = bytes;
                    _audioExt   = ext;
                    if (LoadedAudio != null) UnityEngine.Object.Destroy(LoadedAudio);
                    LoadedAudio = clip;
                    onOk?.Invoke();
                },
                ex => onErr?.Invoke(ex)
            ));
        }
        catch (Exception ex) { onErr?.Invoke(ex); }
    }

    /// <summary>
    /// 用当前 AudioClip 覆盖音频（转 WAV 存入容器，不自动保存）
    /// </summary>
    public void UpdateAudioFromClip()
    {
        if (LoadedAudio == null) throw new Exception("No loaded audio");
        _audioBytes = WavWriter.FromAudioClip(LoadedAudio);
        _audioExt   = "wav";
    }

    /// <summary>
    /// 关闭会话（释放锁/监控/音频）
    /// </summary>
    public void Close()
    {
        IsOpen = false;
        try { _watcher?.Dispose(); } catch { }
        _watcher = null;
        try { _lockStream?.Dispose(); } catch { }
        _lockStream = null;

        if (LoadedAudio != null) { UnityEngine.Object.Destroy(LoadedAudio); LoadedAudio = null; }
        _audioBytes = null; _audioExt = null;
        DechPath = null; TargetSO = null;
    }

    void BeginWatch()
    {
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(DechPath));
        _watcher.Filter = Path.GetFileName(DechPath);
        _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.Attributes | NotifyFilters.Security | NotifyFilters.LastWrite;
        _watcher.Deleted += (_, __) => OnExternalDeleteOrMove?.Invoke("File deleted externally during edit.");
        _watcher.Renamed += (_, __) => OnExternalDeleteOrMove?.Invoke("File moved/renamed externally during edit.");
        _watcher.Changed += (_, __) => { /* 可选：外部写入时做提示 */ };
        _watcher.EnableRaisingEvents = true;
    }

    // ===== 共享锁获取（带重试；mac 更稳）=====
    FileStream AcquireSharedLock(string path)
    {
        const int maxTries = 15;   // ~1.2s
        const int delayMs  = 80;

        for (int i = 0; i < maxTries; i++)
        {
            try
            {
                // 共享读写；避免我们自己再次打开时产生冲突
                return new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            catch (IOException)
            {
                System.Threading.Thread.Sleep(delayMs);
            }
        }
        // 退一步：只读共享
        for (int i = 0; i < maxTries; i++)
        {
            try
            {
                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (IOException)
            {
                System.Threading.Thread.Sleep(delayMs);
            }
        }

        throw new IOException("无法获取文件共享锁（文件可能被系统或其他进程长期占用）。");
    }
}
