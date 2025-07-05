using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Battlehub.Storage
{
    public class FileSystemDataLayer : IDataLayer<string>
    {
        public Task<IList<TreeItem<string>>> GetTreeAsync(string rootID, bool recursive, string folderSearchPattern, string fileSearchPattern)
        {
            string[] folders = folderSearchPattern != null ?
                Directory.GetDirectories(rootID, folderSearchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly) :
                new string[0];

            string[] files = fileSearchPattern != null ?
                Directory.GetFiles(rootID, fileSearchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly) :
                new string[0];

            IList<TreeItem<string>> result = new List<TreeItem<string>>(1 + folders.Length + files.Length);
            if (folderSearchPattern != null && recursive)
            {
                result.Add(CreateTreeItem(rootID, isFolder: true));
            }

            for (int i = 0; i < folders.Length; ++i)
            {
                var folder = folders[i];
                result.Add(CreateTreeItem(folder, isFolder: true));
            }

            for (int i = 0; i < files.Length; ++i)
            {
                var file = files[i];
                result.Add(CreateTreeItem(file, isFolder: false));
            }

            return Task.FromResult(result);
        }

        private string NormalizePath(string path)
        {
            return path.Replace("\\", "/");
        }

        private TreeItem<string> CreateTreeItem(string path, bool isFolder)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(name))
            {
                name = Path.GetFileName(path);
            }

            return new TreeItem<string>(
                NormalizePath(Path.GetDirectoryName(path)),
                NormalizePath(path),
                name,
                isFolder: isFolder);
        }


        public Task<bool> ExistsAsync(string id)
        {
            return Task.FromResult(File.Exists(id) || Directory.Exists(id));
        }

        public Task CreateFolderAsync(string id)
        {
            Directory.CreateDirectory(id);
            return Task.CompletedTask;
        }

        public Task DeleteFolderAsync(string id)
        {
            if (Directory.Exists(id))
            {
                Directory.Delete(id, true);
            }
            return Task.CompletedTask;
        }

        public Task MoveFolderAsync(string folderID, string newFolderID)
        {
            Directory.Move(folderID, newFolderID);
            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(string fileID)
        {
            Stream stream = File.OpenRead(fileID);
            return Task.FromResult(stream);
        }

        public Task<Stream> OpenWriteAsync(string fileID)
        {
            Stream stream = File.Open(fileID, FileMode.Create);
            return Task.FromResult(stream);
        }

        public Task ReleaseAsync(Stream stream)
        {
            stream.Close();
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            if (File.Exists(id))
            {
                File.Delete(id);
            }

            if (Directory.Exists(id))
            {
                Directory.Delete(id);
            }

            return Task.CompletedTask;
        }

        public Task MoveAsync(string fileID, string newFileID)
        {
            File.Move(fileID, newFileID);
            return Task.CompletedTask;
        }

        public async Task CompressZipAsync(string folderID, string password, string outFileID)
        {
#if UNITY_SHARP_ZIP_LIB
            
            await TaskUtils.Run(() => ZipUtils.CompressFolderToZip(outFileID, password, folderID, null));
#else
            await Task.Yield();
            throw new System.NotSupportedException("Import com.unity.sharp-zip-lib https://docs.unity3d.com/Packages/com.unity.sharp-zip-lib@1.3/manual/index.html");
#endif
        }

        public async Task CompressZipAsync(string folderID, string[] files, string password, string outFileID)
        {
#if UNITY_SHARP_ZIP_LIB

            await TaskUtils.Run(() => ZipUtils.CompressFolderToZip(outFileID, password, folderID, files));
#else
            await Task.Yield();
            throw new System.NotSupportedException("Import com.unity.sharp-zip-lib https://docs.unity3d.com/Packages/com.unity.sharp-zip-lib@1.3/manual/index.html");
#endif
        }

        public async Task<string[]> UncompressZipAsync(string fileID, string password, string outFolderID)
        {
#if UNITY_SHARP_ZIP_LIB
            return await TaskUtils.Run(() => ZipUtils.UncompressFromZip(fileID, password, outFolderID));
#else
            await Task.Yield();
            throw new System.NotSupportedException("Import com.unity.sharp-zip-lib https://docs.unity3d.com/Packages/com.unity.sharp-zip-lib@1.3/manual/index.html");
#endif
        }
    }

#if UNITY_SHARP_ZIP_LIB
    internal class ZipUtils
    {
        private static string NormalizePath(string path)
        {
            return path.Replace("\\", "/");
        }

        internal static void CompressFolderToZip(string outPathname, string password, string folderName, string[] files)
        {
            using (FileStream fsOut = File.Create(outPathname))
            using (var zipStream = new Unity.SharpZipLib.Zip.ZipOutputStream(fsOut))
            {
                //0-9, 9 being the highest level of compression
                zipStream.SetLevel(3);

                // optional. Null is the same as not setting. Required if using AES.
                zipStream.Password = password;

                // This setting will strip the leading part of the folder path in the entries, 
                // to make the entries relative to the starting folder.
                // To include the full path for each entry up to the drive root, assign to 0.
                int folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);

                var includeFilesHs = files != null 
                    ? new HashSet<string>(
                        files.Select(file => Path.IsPathRooted(file) 
                            ? NormalizePath(file) 
                            : NormalizePath(Path.Combine(folderName, file)))) 
                    : null;

                CompressFolderToZipInternal(folderName, zipStream, folderOffset, includeFilesHs);
            }
        }

        // Recursively compresses a folder structure
        private static void CompressFolderToZipInternal(string path, Unity.SharpZipLib.Zip.ZipOutputStream zipStream, int folderOffset, HashSet<string> includeFilesHs)
        {
            var files = Directory.GetFiles(path);
            foreach (var filename in files)
            {
                var fi = new FileInfo(filename);

                if (includeFilesHs != null && !includeFilesHs.Contains(NormalizePath(fi.ToString())))
                {
                    continue;
                }

                // Make the name in zip based on the folder
                var entryName = filename.Substring(folderOffset);

                // Remove drive from name and fixe slash direction
                entryName = Unity.SharpZipLib.Zip.ZipEntry.CleanName(entryName);

                var newEntry = new Unity.SharpZipLib.Zip.ZipEntry(entryName);

                // Note the zip format stores 2 second granularity
                newEntry.DateTime = fi.LastWriteTime;

                // Specifying the AESKeySize triggers AES encryption. 
                // Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003,
                // WinZip 8, Java, and other older code, you need to do one of the following: 
                // Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, 
                // you do not need either, but the zip will be in Zip64 format which
                // not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                var buffer = new byte[4096];
                using (FileStream fsInput = File.OpenRead(filename))
                {
                    Unity.SharpZipLib.Core.StreamUtils.Copy(fsInput, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }

            // Recursively call CompressFolder on all folders in path
            var folders = Directory.GetDirectories(path);
            foreach (var folder in folders)
            {
                CompressFolderToZipInternal(folder, zipStream, folderOffset, includeFilesHs);
            }
        }

        /// <summary>
        /// Uncompress the contents of a zip file into the specified folder
        /// </summary>
        /// <param name="archivePath">The path to the zip file</param>
        /// <param name="password">The password required to open the zip file. Set to null if not required.</param>
        /// <param name="outFolder">The output folder</param>
        public static string[] UncompressFromZip(string archivePath, string password, string outFolder)
        {
            Directory.CreateDirectory(outFolder);

            var files = new List<string>();

            using (Stream fs = File.OpenRead(archivePath))
            using (var zf = new Unity.SharpZipLib.Zip.ZipFile(fs))
            {

                if (!string.IsNullOrEmpty(password))
                {
                    // AES encrypted entries are handled automatically
                    zf.Password = password;
                }

                foreach (Unity.SharpZipLib.Zip.ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        // Ignore directories
                        continue;
                    }
                    string entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:
                    //entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here
                    // to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    // Manipulate the output filename here as desired.
                    var fullZipToPath = Path.Combine(outFolder, entryFileName);
                    var directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                    {
                        if (!Directory.Exists(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                        }
                    }

                    if (File.Exists(fullZipToPath))
                    {
                        File.Delete(fullZipToPath);
                    }

                    // 4K is optimum
                    var buffer = new byte[4096];

                    // Unzip file in buffered chunks. This is just as fast as unpacking
                    // to a buffer the full size of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (Stream zipStream = zf.GetInputStream(zipEntry))
                    using (Stream fsOutput = File.Create(fullZipToPath))
                    {
                        files.Add(fullZipToPath);
                        Unity.SharpZipLib.Core.StreamUtils.Copy(zipStream, fsOutput, buffer);
                    }
                }
            }
            return files.ToArray();
        }
    }
#endif
}

