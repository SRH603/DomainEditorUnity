// Assets/Scripts/DomainEchoing/Runtime/FilePickerUtil.cs
using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public static class FilePickerUtil
{
    // 调用方式：
    // FilePickerUtil.OpenDechFile((ok, path) => { if(ok){ ... }});
    public static void OpenDechFile(Action<bool, string> onDone)
    {
        // 反射尝试调用 SFB.StandaloneFileBrowser.OpenFilePanel
        try
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                      .FirstOrDefault(a => a.GetTypes().Any(t => t.FullName == "SFB.StandaloneFileBrowser"));
            if (asm != null)
            {
                var t = asm.GetType("SFB.StandaloneFileBrowser");
                var mi = t.GetMethod("OpenFilePanel", new Type[] {
                    typeof(string), typeof(string), typeof(string[]), typeof(bool)
                });
                if (mi != null)
                {
                    var res = (string[])mi.Invoke(null, new object[] {
                        "Open DECH", "", new string[]{ "dech" }, false
                    });
                    var path = (res != null && res.Length > 0) ? res[0] : null;
                    onDone?.Invoke(!string.IsNullOrEmpty(path), path);
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[DECH] SFB reflection failed: " + e.Message);
        }

        // 退化：弹一个简单的对话（运行时）让用户粘贴或拖入路径
        DegradedPathPrompt.Show(".dech 文件路径：", ".dech", onDone);
    }
}

// 简易退化 UI（IMGUI），允许拖拽/粘贴路径
public class DegradedPathPrompt : MonoBehaviour
{
    static DegradedPathPrompt _inst;
    static string _hint, _ext;
    static Action<bool, string> _cb;
    string path = "";

    public static void Show(string hint, string ext, Action<bool, string> cb)
    {
        if (_inst != null) Destroy(_inst.gameObject);
        var go = new GameObject("DegradedPathPrompt");
        _inst = go.AddComponent<DegradedPathPrompt>();
        _hint = hint; _ext = ext; _cb = cb;
        DontDestroyOnLoad(go);
    }

    void OnGUI()
    {
        GUILayout.Window(12345, new Rect(40,40,600,140), DrawWin, "Select File (Fallback)");
    }

    void DrawWin(int id)
    {
        GUILayout.Label(_hint);
        GUI.SetNextControlName("path");
        path = GUILayout.TextField(path);
#if UNITY_EDITOR
        if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    path = DragAndDrop.paths[0];
            }
            Event.current.Use();
        }
#endif
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("OK", GUILayout.Width(120)))
        {
            bool ok = !string.IsNullOrEmpty(path) && path.EndsWith(_ext, StringComparison.OrdinalIgnoreCase);
            _cb?.Invoke(ok, path);
            Close();
        }
        if (GUILayout.Button("Cancel", GUILayout.Width(120)))
        {
            _cb?.Invoke(false, null);
            Close();
        }
        GUILayout.EndHorizontal();
        GUI.FocusControl("path");
    }

    void Close()
    {
        Destroy(gameObject);
        _inst = null; _cb = null;
    }
}
