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
    static string OpenWinOpen(string title, string startDir, string filter)
    {
        string result = null;
        var t = new Thread(() =>
        {
            try
            {
                using (var dlg = new System.Windows.Forms.OpenFileDialog())
                {
                    dlg.Title = string.IsNullOrEmpty(title) ? "Open" : title;
                    dlg.Filter = filter;
                    dlg.FilterIndex = 1;
                    dlg.AddExtension = true;
                    dlg.CheckFileExists = true;
                    dlg.DereferenceLinks = true;
                    dlg.RestoreDirectory = true;
                    if (!string.IsNullOrEmpty(startDir) && Directory.Exists(startDir))
                        dlg.InitialDirectory = startDir;

                    var r = dlg.ShowDialog();
                    if (r == System.Windows.Forms.DialogResult.OK)
                        result = dlg.FileName;
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();
        return result;
    }

    static string SaveWin(string title, string startDir, string defaultName, string extNoDot)
    {
        string result = null;
        var t = new Thread(() =>
        {
            try
            {
                using (var dlg = new System.Windows.Forms.SaveFileDialog())
                {
                    dlg.Title = string.IsNullOrEmpty(title) ? "Save" : title;
                    dlg.Filter = $"DECH file (*.{extNoDot})|*.{extNoDot}";
                    dlg.AddExtension = true;
                    dlg.DefaultExt = extNoDot;
                    dlg.OverwritePrompt = true;
                    if (!string.IsNullOrEmpty(defaultName)) dlg.FileName = defaultName;
                    if (!string.IsNullOrEmpty(startDir) && Directory.Exists(startDir))
                        dlg.InitialDirectory = startDir;

                    var r = dlg.ShowDialog();
                    if (r == System.Windows.Forms.DialogResult.OK)
                        result = dlg.FileName;
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();
        return result;
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
