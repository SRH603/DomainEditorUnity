#if UNITY_STANDALONE_WIN
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SFB
{
    // 纯 Win32 版本的 Windows 文件对话框实现
    // - OpenFilePanel / SaveFilePanel: comdlg32.GetOpenFileNameW / GetSaveFileNameW
    // - OpenFolderPanel: shell32.SHBrowseForFolderW + SHGetPathFromIDListW
    // 兼容从 macOS Editor 构建 Windows Player（不依赖 System.Windows.Forms / Ookii.Dialogs）

    public class StandaloneFileBrowserWindows : IStandaloneFileBrowser
    {
        // === Win32 structs & imports ===
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct OPENFILENAME
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public StringBuilder lpstrFile;
            public int nMaxFile;
            public StringBuilder lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int FlagsEx;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName; // 接受显示名称的缓冲
            public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        private const int MAX_PATH = 260;
        private const int MAX_BUFFER = 65536;

        // OPENFILENAME Flags
        private const int OFN_READONLY            = 0x00000001;
        private const int OFN_OVERWRITEPROMPT     = 0x00000002;
        private const int OFN_HIDEREADONLY        = 0x00000004;
        private const int OFN_NOCHANGEDIR         = 0x00000008;
        private const int OFN_SHOWHELP            = 0x00000010;
        private const int OFN_ENABLEHOOK          = 0x00000020;
        private const int OFN_ENABLETEMPLATE      = 0x00000040;
        private const int OFN_ENABLETEMPLATEHANDLE= 0x00000080;
        private const int OFN_NOVALIDATE          = 0x00000100;
        private const int OFN_ALLOWMULTISELECT    = 0x00000200;
        private const int OFN_EXTENSIONDIFFERENT  = 0x00000400;
        private const int OFN_PATHMUSTEXIST       = 0x00000800;
        private const int OFN_FILEMUSTEXIST       = 0x00001000;
        private const int OFN_CREATEPROMPT        = 0x00002000;
        private const int OFN_SHAREAWARE          = 0x00004000;
        private const int OFN_NOREADONLYRETURN    = 0x00008000;
        private const int OFN_NOTESTFILECREATE    = 0x00010000;
        private const int OFN_NONETWORKBUTTON     = 0x00020000;
        private const int OFN_NOLONGNAMES         = 0x00040000;
        private const int OFN_EXPLORER            = 0x00080000;
        private const int OFN_NODEREFERENCELINKS  = 0x00100000;
        private const int OFN_LONGNAMES           = 0x00200000;

        // BROWSEINFO Flags
        private const uint BIF_RETURNONLYFSDIRS   = 0x00000001;
        private const uint BIF_NEWDIALOGSTYLE     = 0x00000040; // Vista 风格
        private const uint BIF_EDITBOX            = 0x00000010;

        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetOpenFileName(ref OPENFILENAME ofn);

        [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetSaveFileName(ref OPENFILENAME ofn);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO bi);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr pv);

        // ============ IStandaloneFileBrowser 实现 ============

        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
        {
            var ofn = new OPENFILENAME();
            InitOFN(ref ofn, title, directory, extensions, defExt: null, forSave: false, multiselect: multiselect);

            bool ok = GetOpenFileName(ref ofn);
            if (!ok) return new string[0];

            return ParseSelectedFiles(ofn.lpstrFile, multiselect);
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb)
        {
            // 简单同步→回调
            cb?.Invoke(OpenFilePanel(title, directory, extensions, multiselect));
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect)
        {
            // Windows 标准文件夹选择不支持多选；忽略 multiselect
            var bi = new BROWSEINFO();
            bi.hwndOwner = GetActiveWindow();
            bi.pidlRoot = IntPtr.Zero;
            bi.lpszTitle = string.IsNullOrEmpty(title) ? "Select Folder" : title;

            // 用一个显示名缓冲（虽然我们只取路径）
            var displayName = Marshal.AllocHGlobal(MAX_PATH * sizeof(char));
            bi.pszDisplayName = displayName;

            bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE | BIF_EDITBOX;

            // 调用
            IntPtr pidl = SHBrowseForFolder(ref bi);
            string result = string.Empty;

            if (pidl != IntPtr.Zero)
            {
                var sb = new StringBuilder(MAX_PATH);
                if (SHGetPathFromIDList(pidl, sb))
                {
                    result = sb.ToString();
                }
                CoTaskMemFree(pidl);
            }

            Marshal.FreeHGlobal(displayName);

            if (string.IsNullOrEmpty(result)) return new string[0];
            return new[] { result };
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb)
        {
            cb?.Invoke(OpenFolderPanel(title, directory, multiselect));
        }

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            string defExt = null;
            if (extensions != null && extensions.Length > 0 && extensions[0].Extensions != null && extensions[0].Extensions.Length > 0)
                defExt = extensions[0].Extensions[0];

            var ofn = new OPENFILENAME();
            InitOFN(ref ofn, title, directory, extensions, defExt, forSave: true, multiselect: false);

            // 设定默认文件名
            if (!string.IsNullOrEmpty(defaultName))
            {
                var initial = defaultName;
                if (initial.Length >= ofn.nMaxFile) initial = TruncateForBuffer(initial, ofn.nMaxFile);
                ofn.lpstrFile.Clear();
                ofn.lpstrFile.Append(initial);
            }

            bool ok = GetSaveFileName(ref ofn);
            if (!ok) return string.Empty;

            var path = ofn.lpstrFile.ToString();
            return path;
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb)
        {
            cb?.Invoke(SaveFilePanel(title, directory, defaultName, extensions));
        }

        // ============ Helpers ============

        private static void InitOFN(ref OPENFILENAME ofn, string title, string directory, ExtensionFilter[] exts, string defExt, bool forSave, bool multiselect)
        {
            ofn.lStructSize = Marshal.SizeOf(typeof(OPENFILENAME));
            ofn.hwndOwner = GetActiveWindow();
            ofn.hInstance = IntPtr.Zero;

            ofn.lpstrFilter = BuildFilter(exts);
            ofn.nFilterIndex = 1;

            ofn.lpstrFile = new StringBuilder(MAX_BUFFER);
            ofn.nMaxFile = MAX_BUFFER;

            ofn.lpstrFileTitle = new StringBuilder(MAX_PATH);
            ofn.nMaxFileTitle = MAX_PATH;

            ofn.lpstrInitialDir = SafeInitialDir(directory);
            ofn.lpstrTitle = string.IsNullOrEmpty(title) ? (forSave ? "Save As" : "Open") : title;

            int flags = OFN_EXPLORER | OFN_HIDEREADONLY | OFN_NOCHANGEDIR;
            if (forSave)
                flags |= OFN_OVERWRITEPROMPT | OFN_PATHMUSTEXIST;
            else
                flags |= OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST;

            if (multiselect && !forSave)
                flags |= OFN_ALLOWMULTISELECT;

            ofn.Flags = flags;
            ofn.lpstrDefExt = defExt; // null 也可
        }

        private static string SafeInitialDir(string dir)
        {
            if (string.IsNullOrEmpty(dir)) return null;
            try
            {
                var full = Path.GetFullPath(dir);
                if (Directory.Exists(full)) return full;
            }
            catch { }
            return null;
        }

        private static string BuildFilter(ExtensionFilter[] exts)
        {
            // Win32 需要： "Desc\0*.ext;*.ext\0Desc2\0*.ext\0\0"
            // 若无扩展，则给 All files
            if (exts == null || exts.Length == 0)
            {
                return "All files\0*.*\0\0";
            }

            var sb = new StringBuilder();
            foreach (var f in exts)
            {
                if (f.Extensions == null || f.Extensions.Length == 0) continue;
                string desc = string.IsNullOrEmpty(f.Name) ? "Files" : f.Name;
                sb.Append(desc);
                sb.Append("\0");

                // 多扩展用 ; 分隔
                for (int i = 0; i < f.Extensions.Length; i++)
                {
                    var ext = f.Extensions[i];
                    if (string.IsNullOrEmpty(ext)) continue;
                    if (i > 0) sb.Append(';');
                    sb.Append("*.");
                    sb.Append(ext);
                }
                sb.Append("\0");
            }
            sb.Append("\0"); // 结束双 \0
            return sb.ToString();
        }

        private static string[] ParseSelectedFiles(StringBuilder buffer, bool multiselect)
        {
            // OPENFILENAME 如果多选：缓冲内容为：
            // "<dir>\0<file1>\0<file2>\0...\0\0"
            // 单选：直接是 "C:\path\file.ext\0"
            string raw = buffer.ToString();

            // 查找第一个双零结束（保险）
            int doubleNull = raw.IndexOf("\0\0", StringComparison.Ordinal);
            if (doubleNull >= 0) raw = raw.Substring(0, doubleNull);

            var parts = raw.Split('\0');
            if (parts.Length == 0) return new string[0];

            if (parts.Length == 1 || !multiselect)
            {
                // 单文件
                return new[] { parts[0] };
            }
            else
            {
                // 多文件：第一个是目录，后面是文件名
                string dir = parts[0];
                var results = new string[parts.Length - 1];
                for (int i = 1; i < parts.Length; i++)
                {
                    results[i - 1] = Path.Combine(dir, parts[i]);
                }
                return results;
            }
        }

        private static string TruncateForBuffer(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length < max - 1) return s;
            return s.Substring(0, max - 1);
        }
    }
}
#endif
