using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LogRedirector : MonoBehaviour
{
    private static StreamWriter logWriter;
    private static string logPath;
    private static bool isInitialized = false;

    void Awake()
    {
        // 只初始化一次
        if (isInitialized) return;
        isInitialized = true;
        DontDestroyOnLoad(gameObject);

        InitLogWriter();
        Application.logMessageReceived += HandleLog;
    }

    private void InitLogWriter()
    {
        string filename = $"UnityLog_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

        /*
#if UNITY_EDITOR
        // 编辑器模式下输出到项目根目录的 Logs 目录
        string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#else
        // 打包后写入桌面（macOS）或 persistentDataPath
#if UNITY_STANDALONE_OSX
        string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#else
        string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif
#endif

*/

        Directory.CreateDirectory(folder); // 保证目录存在
        logPath = Path.Combine(folder, filename);

        try
        {
            logWriter = new StreamWriter(logPath, true);
            logWriter.AutoFlush = true;
            logWriter.WriteLine($"[日志开始] {System.DateTime.Now}\n--------------------");
            Debug.Log($"日志写入路径：{logPath}");
        }
        catch (IOException ex)
        {
            Debug.LogError("日志写入初始化失败: " + ex.Message);
        }
    }

    private static void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (logWriter == null) return;

        string time = System.DateTime.Now.ToString("HH:mm:ss.fff");
        string prefix = $"[{time}] [{type}] ";
        logWriter.WriteLine(prefix + condition);
        if (type == LogType.Error || type == LogType.Exception)
        {
            logWriter.WriteLine(stackTrace);
        }
    }

    void OnDestroy()
    {
        if (logWriter != null)
        {
            Application.logMessageReceived -= HandleLog;
            logWriter.Close();
            logWriter = null;
        }
    }
}
