// Assets/Scripts/Utilities/RTE/Serialization/Runtime/DechRuntimeUI.cs
using System;
using System.IO;
using UnityEngine;

public class DechRuntimeUI : MonoBehaviour
{
    [Header("把你要填充的 ScriptableObject 指定进来")]
    public GameData targetGameData;

    private DechSession _session = new DechSession();
    private string _status = "就绪。";
    private string _lastDir;
    private const string LastDirKey = "DECH_LAST_DIR";

    void OnEnable()
    {
        _lastDir = PlayerPrefs.GetString(LastDirKey, "");
        _session.OnLoaded += (so, clip) =>
        {
            _status = $"已加载：{_session.DechPath}\n音频：{clip.frequency}Hz / {clip.channels}ch / {clip.length:F1}s";
        };
        _session.OnExternalDeleteOrMove += (msg) => _status = "[警告] " + msg;
    }

    void OnDisable()
    {
        PlayerPrefs.SetString(LastDirKey, _lastDir ?? "");
        PlayerPrefs.Save();
        _session.Close();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 700, 260), "Domain Echoing – DECH Editor (Runtime)", GUI.skin.window);
        GUILayout.Label(_status);

        GUILayout.Space(8);
        if (GUILayout.Button("打开 .dech ...（系统原生文件对话框）", GUILayout.Height(32)))
        {
            if (targetGameData == null)
            {
                _status = "请先在 Inspector 里指定 targetGameData。";
            }
            else
            {
                try
                {
                    var path = NativeFileDialogs.OpenDech("Open DECH", string.IsNullOrEmpty(_lastDir) ? "" : _lastDir);
                    if (string.IsNullOrEmpty(path))
                    {
                        _status = "已取消。";
                    }
                    else if (!path.EndsWith(".dech", StringComparison.OrdinalIgnoreCase))
                    {
                        _status = "导入失败：仅支持 .dech 文件。";
                    }
                    else
                    {
                        _lastDir = System.IO.Path.GetDirectoryName(path);
                        // 关键：这里用 OpenAsync，并把 this 传进去作为协程宿主
                        _session.OpenAsync(this, path, targetGameData);
                        _status = $"载入成功：{path}";
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    _status = "载入失败：" + ex.Message;
                }
            }
        }

        GUI.enabled = _session.IsOpen;
        if (GUILayout.Button("保存（覆盖 .dech）", GUILayout.Height(32)))
        {
            try
            {
                if (_session.Save()) _status = "保存成功（已原地覆盖）。";
                else _status = "保存失败：未打开会话。";
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _status = "保存异常：" + ex.Message;
            }
        }
        GUI.enabled = true;

        GUILayout.EndArea();
    }
}
