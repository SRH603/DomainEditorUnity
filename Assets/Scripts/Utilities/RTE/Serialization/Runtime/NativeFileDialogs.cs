// ===== NativeFileDialogs.cs (fallback for .dech on macOS) =====
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
#define NFD_WIN
#endif
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
#define NFD_OSX
#endif
#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
#define NFD_LINUX
#endif

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

public static class NativeFileDialogs
{
    // —— 打开 .dech（macOS：不做过滤，业务层强校验）——
    public static string OpenDech(string title, string startDir)
    {
#if NFD_WIN
        return OpenWinOpen(title, startDir, "DECH file (*.dech)|*.dech");
#elif NFD_OSX
        return OpenMacDech_NoFilter(title, startDir); // 关键：不再用 of type 过滤
#elif NFD_LINUX
        return OpenLinuxOpen(title, startDir, "*.dech");
#else
        return null;
#endif
    }

    // —— 另存为 .dech（强制补后缀）——
    public static string SaveDech(string title, string startDir, string defaultName)
    {
#if NFD_WIN
        return SaveWin(title, startDir, defaultName, "dech");
#elif NFD_OSX
        return SaveMac(title, startDir, defaultName, "dech");
#elif NFD_LINUX
        return SaveLinux(title, startDir, defaultName, "dech");
#else
        return null;
#endif
    }

    // —— 打开音频（Win 按后缀过滤；mac 用 public.audio）——
    public static string OpenAudio(string title, string startDir)
    {
#if NFD_WIN
        string filter = "Audio files|*.wav;*.ogg;*.mp3;*.aiff;*.aif;*.flac;*.m4a;*.aac;*.oga;*.opus";
        return OpenWinOpen(title, startDir, filter);
#elif NFD_OSX
        return OpenMacAudio(title, startDir);
#elif NFD_LINUX
        return OpenLinuxOpen(title, startDir, "*.wav *.ogg *.mp3 *.aiff *.aif *.flac *.m4a *.aac *.oga *.opus");
#else
        return null;
#endif
    }

    // ===================== Windows =====================
#if NFD_WIN
    // ===== Win32 对话框（无 System.Windows.Forms 依赖）=====
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    struct OPENFILENAME
    {
        public int lStructSize;
        public System.IntPtr hwndOwner;
        public System.IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public System.Text.StringBuilder lpstrFile;
        public int nMaxFile;
        public System.Text.StringBuilder lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public System.IntPtr lCustData;
        public System.IntPtr lpfnHook;
        public string lpTemplateName;
        public System.IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }

    const int MAX_PATH   = 260;
    const int MAX_BUFFER = 65536;

    // Flags
    const int OFN_EXPLORER          = 0x00080000;
    const int OFN_HIDEREADONLY      = 0x00000004;
    const int OFN_NOCHANGEDIR       = 0x00000008;
    const int OFN_OVERWRITEPROMPT   = 0x00000002;
    const int OFN_PATHMUSTEXIST     = 0x00000800;
    const int OFN_FILEMUSTEXIST     = 0x00001000;
    const int OFN_ALLOWMULTISELECT  = 0x00000200;

    [System.Runtime.InteropServices.DllImport("comdlg32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    static extern bool GetOpenFileName(ref OPENFILENAME ofn);

    [System.Runtime.InteropServices.DllImport("comdlg32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    static extern bool GetSaveFileName(ref OPENFILENAME ofn);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern System.IntPtr GetActiveWindow();

    static string OpenWinOpen(string title, string startDir, string filter)
    {
        try
        {
            var ofn = new OPENFILENAME();
            InitOFN(ref ofn, title, startDir, ConvertWinFormsFilterToWin32(filter), defExt: null, forSave: false, multiselect: false);
            bool ok = GetOpenFileName(ref ofn);
            if (!ok) return null;

            // 单选：直接返回
            string raw = ofn.lpstrFile.ToString();
            int dbl = raw.IndexOf("\0\0", System.StringComparison.Ordinal);
            if (dbl >= 0) raw = raw.Substring(0, dbl);
            var parts = raw.Split('\0');
            if (parts.Length <= 1) return string.IsNullOrEmpty(parts[0]) ? null : parts[0];
            // 若返回了路径 + 文件名（多选格式），取第一项
            string dir = parts[0];
            return System.IO.Path.Combine(dir, parts[1]);
        }
        catch (System.Exception ex)
        {
            global::UnityEngine.Debug.LogError("[NFD][Win] Open dialog failed: " + ex.Message);
            return null;
        }
    }

    static string SaveWin(string title, string startDir, string defaultName, string extNoDot)
    {
        try
        {
            var ofn = new OPENFILENAME();
            // 构造一个简单过滤（*.ext）
            string filter = string.IsNullOrEmpty(extNoDot) ? "All files (*.*)|*.*" : $"DECH file (*.{extNoDot})|*.{extNoDot}";
            InitOFN(ref ofn, title, startDir, ConvertWinFormsFilterToWin32(filter), defExt: extNoDot, forSave: true, multiselect: false);

            // 预填默认文件名
            if (!string.IsNullOrEmpty(defaultName))
            {
                var initial = defaultName;
                if (initial.Length >= ofn.nMaxFile) initial = initial.Substring(0, ofn.nMaxFile - 1);
                ofn.lpstrFile.Clear();
                ofn.lpstrFile.Append(initial);
            }

            bool ok = GetSaveFileName(ref ofn);
            if (!ok) return null;

            return ofn.lpstrFile.ToString();
        }
        catch (System.Exception ex)
        {
            global::UnityEngine.Debug.LogError("[NFD][Win] Save dialog failed: " + ex.Message);
            return null;
        }
    }

    static void InitOFN(ref OPENFILENAME ofn, string title, string startDir, string win32Filter, string defExt, bool forSave, bool multiselect)
    {
        ofn.lStructSize   = System.Runtime.InteropServices.Marshal.SizeOf(typeof(OPENFILENAME));
        ofn.hwndOwner     = GetActiveWindow();
        ofn.hInstance     = System.IntPtr.Zero;
        ofn.lpstrFilter   = string.IsNullOrEmpty(win32Filter) ? "All files\0*.*\0\0" : win32Filter;
        ofn.nFilterIndex  = 1;
        ofn.lpstrFile     = new System.Text.StringBuilder(MAX_BUFFER);
        ofn.nMaxFile      = MAX_BUFFER;
        ofn.lpstrFileTitle= new System.Text.StringBuilder(MAX_PATH);
        ofn.nMaxFileTitle = MAX_PATH;
        ofn.lpstrInitialDir = SafeInitialDir(startDir);
        ofn.lpstrTitle    = string.IsNullOrEmpty(title) ? (forSave ? "Save As" : "Open") : title;
        ofn.lpstrDefExt   = defExt;

        int flags = OFN_EXPLORER | OFN_HIDEREADONLY | OFN_NOCHANGEDIR;
        if (forSave)
            flags |= OFN_OVERWRITEPROMPT | OFN_PATHMUSTEXIST;
        else
            flags |= OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST;
        if (multiselect && !forSave)
            flags |= OFN_ALLOWMULTISELECT;
        ofn.Flags = flags;
    }

    static string SafeInitialDir(string dir)
    {
        if (string.IsNullOrEmpty(dir)) return null;
        try
        {
            var full = System.IO.Path.GetFullPath(dir);
            if (System.IO.Directory.Exists(full)) return full;
        }
        catch {}
        return null;
    }

    // 把 WinForms 风格的 "Desc|*.ext;*.ext|Desc2|*.ext" 转为 Win32 需要的 "Desc\0*.ext;*.ext\0Desc2\0*.ext\0\0"
    static string ConvertWinFormsFilterToWin32(string winForms)
    {
        if (string.IsNullOrEmpty(winForms)) return "All files\0*.*\0\0";
        var parts = winForms.Split('|');
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i + 1 < parts.Length; i += 2)
        {
            sb.Append(parts[i]);          // 描述
            sb.Append('\0');
            sb.Append(parts[i + 1]);      // 模式（*.ext;*.ext）
            sb.Append('\0');
        }
        sb.Append('\0'); // 结束双零
        return sb.ToString();
    }
#endif


    // ===================== macOS =====================
#if NFD_OSX
    static string OpenMacDech_NoFilter(string title, string startDir)
    {
        // 不过滤，让用户能选任何文件；业务层做 .dech 校验
        string t = EscapeApple(title ?? "Open DECH");
        string d = EscapeApple(InitDir(startDir));
        string script =
$@"set _title to ""{t}""
set _defaultDir to POSIX file ""{d}""
try
    set _f to choose file with prompt _title default location _defaultDir
    set _p to POSIX path of _f
    return _p
on error number -128
    return """"
end try";
        return RunAppleScript(script);
    }

    static string OpenMacAudio(string title, string startDir)
    {
        string t = EscapeApple(title ?? "Open Audio");
        string d = EscapeApple(InitDir(startDir));
        string script =
$@"set _title to ""{t}""
set _defaultDir to POSIX file ""{d}""
try
    set _f to choose file with prompt _title of type {{""public.audio""}} default location _defaultDir
    set _p to POSIX path of _f
    return _p
on error number -128
    return """"
end try";
        return RunAppleScript(script);
    }

    static string SaveMac(string title, string startDir, string defaultName, string extNoDot)
    {
        string t = EscapeApple(title ?? "Save DECH");
        string d = EscapeApple(InitDir(startDir));
        string n = EscapeApple(string.IsNullOrEmpty(defaultName) ? "chart" : defaultName);
        string e = EscapeApple(string.IsNullOrEmpty(extNoDot) ? "dech" : extNoDot.Trim().TrimStart('.'));
        string script =
$@"set _title to ""{t}""
set _defaultDir to POSIX file ""{d}""
set _name to ""{n}""
set _ext to ""{e}""
try
    set _f to choose file name with prompt _title default location _defaultDir default name _name
    set _p to POSIX path of _f
    if _p does not end with ""."" & _ext then set _p to _p & ""."" & _ext
    return _p
on error number -128
    return """"
end try";
        return RunAppleScript(script);
    }

    static string RunAppleScript(string script)
    {
        string tmp = Path.Combine(Path.GetTempPath(), "nfd_" + Guid.NewGuid().ToString("N") + ".applescript");
        File.WriteAllText(tmp, script);
        try
        {
            var psi = new ProcessStartInfo{
                FileName = "/usr/bin/osascript",
                Arguments = "\"" + tmp.Replace("\"","\\\"") + "\"",
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            string stdout = p.StandardOutput.ReadToEnd();
            string stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                UnityEngine.Debug.LogError("[NFD] osascript failed: exit=" + p.ExitCode + " err=" + stderr);
                return null;
            }
            var path = (stdout ?? "").Trim();
            return string.IsNullOrEmpty(path) ? null : path;
        }
        catch (Exception ex) { UnityEngine.Debug.LogException(ex); return null; }
        finally { try { File.Delete(tmp); } catch {} }
    }

    static string InitDir(string dir)
    {
        return string.IsNullOrEmpty(dir)
            ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            : dir;
    }
    static string EscapeApple(string s) => (s ?? "").Replace("\\","\\\\").Replace("\"","\\\"");
#endif

    // ===================== Linux =====================
#if NFD_LINUX
    static string OpenLinuxOpen(string title, string startDir, string filterPattern)
    {
        string zenity = "/usr/bin/zenity"; if (!File.Exists(zenity)) return null;
        string initial = string.IsNullOrEmpty(startDir)
            ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
            : startDir.TrimEnd('/') + "/";

        var psi = new ProcessStartInfo{
            FileName = zenity,
            Arguments = $"--file-selection --title={Quote(title ?? "Open")} --filename={Quote(initial)} --file-filter={Quote(filterPattern)}",
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
        };
        using var p = Process.Start(psi);
        string outp = p.StandardOutput.ReadToEnd(); p.WaitForExit();
        var path = (outp ?? "").Trim();
        return string.IsNullOrEmpty(path) ? null : path;
    }

    static string SaveLinux(string title, string startDir, string defaultName, string extNoDot)
    {
        string zenity = "/usr/bin/zenity"; if (!File.Exists(zenity)) return null;
        string initial = string.IsNullOrEmpty(startDir)
            ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
            : startDir.TrimEnd('/') + "/";

        var psi = new ProcessStartInfo{
            FileName = zenity,
            Arguments = $"--file-selection --save --confirm-overwrite --title={Quote(title ?? "Save")} --filename={Quote(initial + (string.IsNullOrEmpty(defaultName) ? "chart" : defaultName) + "." + extNoDot)}",
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
        };
        using var p = Process.Start(psi);
        string outp = p.StandardOutput.ReadToEnd(); p.WaitForExit();
        var path = (outp ?? "").Trim();
        return string.IsNullOrEmpty(path) ? null : path;
    }

    static string Quote(string s) => "\"" + (s ?? "").Replace("\"", "\\\"") + "\"";
#endif
}
