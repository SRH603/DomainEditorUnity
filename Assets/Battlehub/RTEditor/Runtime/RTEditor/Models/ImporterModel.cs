using UnityEngine;
using System.Collections;
using Battlehub.RTCommon;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Networking;
using System.IO;
using Battlehub.RTEditor.Models;

namespace Battlehub.RTEditor
{
    [Serializable]
    public class FileImporterException : Exception
    {
        public FileImporterException() { }
        public FileImporterException(string message) : base(message) { }
        public FileImporterException(string message, Exception inner) : base(message, inner) { }
        protected FileImporterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public abstract class FileImporterAsync : IFileImporterAsync
    {
        public abstract string FileExt { get; }

        public abstract string IconPath { get; }

        public virtual int Priority
        {
            get { return 0; }
        }

        public abstract Task ImportAsync(string filePath, string targetPath, CancellationToken cancelToken);
    }


    [Serializable]
    public class UnityWebRequestException : Exception
    {
        public UnityWebRequestException() { }
        public UnityWebRequestException(string message) : base(message) { }
        public UnityWebRequestException(string message, Exception inner) : base(message, inner) { }
        protected UnityWebRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public abstract class AssetDatabaseFileImporter : FileImporterAsync, IAssetDatabaseFileImporter
    {
        private IRuntimeEditor m_editor;

        protected IRuntimeEditor Editor
        {
            get { return m_editor; }
        }

        public virtual void Load()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();
        }

        public virtual void Unload()
        {
            m_editor = null;
        }

        protected Task ImportExternalAsset(string targetPath, Guid assetID, string externalAssetKey, IExternalAssetLoaderModel loader)
        {
            string folderPath = Path.GetDirectoryName(targetPath);
            string desiredName = Path.GetFileNameWithoutExtension(targetPath);
            var folderID = Editor.GetAssetID(folderPath);
            if (folderID == ID.Empty)
            {
                throw new ArgumentException($"Folder {folderPath} not found", "targetPath");
            }

            return Editor.ImportExternalAssetAsync(folderID, assetID, externalAssetKey, loader.LoaderID, desiredName);
        }

        protected static bool IsUrl(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult))
            {
                return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
            }
            return false;
        }

        protected static string GetBasePath(string filePath)
        {
            string parentPath;
            if (IsUrl(filePath))
            {
                var uri = new Uri(filePath, UriKind.Absolute);
                parentPath = uri.GetLeftPart(UriPartial.Authority) + uri.AbsolutePath.Substring(0, uri.AbsolutePath.LastIndexOf('/'));
            }
            else
            {
                parentPath = Path.GetDirectoryName(filePath);
            }
            return parentPath;
        }

        protected async Task<string> AddFileToLibraryAsync(string basePath, string relativePath, Guid desiredAssetID)
        {
            string filePath = $"{basePath}/{relativePath}";
            string libraryFolder = Editor.GetFolderInLibrary(desiredAssetID);
            string libraryFolderFullPath = $"{Editor.LibraryRootFolder}/{libraryFolder}";
            if (!Directory.Exists(libraryFolderFullPath))
            {
                Directory.CreateDirectory(libraryFolderFullPath);
            }

            string targetPath = $"{libraryFolderFullPath}/{relativePath}";
            string targetDir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            if (IsUrl(filePath))
            {
                var bytes = await DownloadBytesAsync(filePath);
                File.WriteAllBytes(targetPath, bytes);
            }
            else
            {
                File.Copy(filePath, targetPath, true);
            }

            string externalAssetKey = $"{libraryFolder}/{relativePath}";
            return externalAssetKey;

        }

        protected Task<string> AddFileToLibraryAsync(string filePath, Guid desiredAssetID)
        {
            string basePath = GetBasePath(filePath);
            string relativePath = Path.GetFileName(filePath);
            return AddFileToLibraryAsync(basePath, relativePath, desiredAssetID);
        }

        protected string GetUniquePath(string targetPath, string ext = null)
        {
            if (string.IsNullOrEmpty(ext))
            {
                ext = ".asset";// IAssetDatabaseModel.k_assetExt;
            }

            string folderPath = Path.GetDirectoryName(targetPath);
            string desiredName = Path.GetFileNameWithoutExtension(targetPath);

            Guid folderID = Editor.GetAssetID(folderPath);
            if (folderID == Guid.Empty)
            {
                throw new ArgumentException($"Folder {folderPath} not found", "targetPath");
            }

            return Editor.GetUniquePath(folderID, $"{desiredName}{ext}");
        }

        protected Task<byte[]> DownloadBytesAsync(string filePath)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
            IOC.Resolve<IRTE>().StartCoroutine(CoDownloadBytes(filePath, tcs));
            return tcs.Task;
        }

        private IEnumerator CoDownloadBytes(string filePath, TaskCompletionSource<byte[]> tcs)
        {
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                tcs.SetException(new UnityWebRequestException(www.error));
            }
            else
            {
                tcs.SetResult(www.downloadHandler.data);
            }
        }

        [Obsolete("Use AddFileToLibraryAsync")]
        protected string AddFileToLibrary(string filePath, Guid desiredAssetID)
        {
            string fileName = Path.GetFileName(filePath);
            string libraryFolder = Editor.GetFolderInLibrary(desiredAssetID);
            string libraryFolderFullPath = $"{Editor.LibraryRootFolder}/{libraryFolder}";
            if (!Directory.Exists(libraryFolderFullPath))
            {
                Directory.CreateDirectory(libraryFolderFullPath);
            }
            File.Copy(filePath, $"{libraryFolderFullPath}/{fileName}", true);
            string externalAssetKey = $"{libraryFolder}/{fileName}";
            return externalAssetKey;
        }

    }
}

namespace Battlehub.RTEditor.Models
{
    public class ImporterModel : MonoBehaviour, IImporterModel
    {
        private readonly Dictionary<string, IFileImporterDescription> m_extToFileImporter = new Dictionary<string, IFileImporterDescription>();

        public string[] Extensions
        {
            get;
            private set;
        }

        public Sprite[] Icons
        {
            get;
            private set;
        }

        private IRuntimeEditor m_editor;
        private bool m_importersLoaded;
        
        protected virtual void Awake()
        {
            if (!IOC.IsFallbackRegistered<IImporterModel>())
            {
                IOC.RegisterFallback<IImporterModel>(this);
            }

            m_editor = IOC.Resolve<IRuntimeEditor>();
            if (m_editor != null)
            {
                m_editor.BeforeLoadProject += OnBeforeLoadProject;
                m_editor.UnloadProject += OnUnloadProject;
            }

            if (m_editor.IsProjectLoaded)
            {
                LoadImporters();
            }
            
            Dictionary<string, Sprite> extToIcon = new Dictionary<string, Sprite>();
            List<Assembly> assemblies = new List<Assembly>();
            foreach (string assemblyName in KnownAssemblies.Names)
            {
                var asName = new AssemblyName();
                asName.Name = assemblyName;

                try
                {
                    Assembly asm = Assembly.Load(asName);
                    assemblies.Add(asm);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }

            Type[] importerTypes = assemblies.SelectMany(asm => asm.GetTypes().Where(t => t != null && t.IsClass && typeof(IFileImporterDescription).IsAssignableFrom(t))).ToArray();
            foreach (Type importerType in importerTypes)
            {
                if (importerType.IsAbstract)
                {
                    continue;
                }

                try
                {
                    IFileImporterDescription fileImporter = (IFileImporterDescription)Activator.CreateInstance(importerType);

                    string ext = fileImporter.FileExt;
                    ext = ext.ToLower();

                    if (!ext.StartsWith("."))
                    {
                        ext = "." + ext;
                    }

                    if (m_extToFileImporter.ContainsKey(ext))
                    {
                        int priority = fileImporter.Priority;
                        if (fileImporter is IFileImporterAsync)
                        {
                            priority++;
                        }

                        if (m_extToFileImporter[ext].Priority > priority)
                        {
                            continue;
                        }
                    }
                    m_extToFileImporter[ext] = fileImporter;
                    extToIcon[ext] = Resources.Load<Sprite>(fileImporter.IconPath);
                }
                catch (Exception e)
                {
                    Debug.LogError("Unable to instantiate and initialize File Importer " + e.ToString());
                }
            }

            Extensions = extToIcon.Keys.ToArray();
            Icons = extToIcon.Values.ToArray();
        }
   
        protected virtual void OnDestroy()
        {
            IOC.UnregisterFallback<IImporterModel>(this);

            if (m_editor != null)
            {
                m_editor.BeforeLoadProject -= OnBeforeLoadProject;
                m_editor.UnloadProject -= OnUnloadProject;
            }

            foreach (var fileImporter in m_extToFileImporter.Values)
            {
                IDisposable disposable = fileImporter as IDisposable;
                if (disposable != null)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            m_extToFileImporter.Clear();
            Extensions = null;
            Icons = null;
        }

        private void OnBeforeLoadProject(object sender, EventArgs e)
        {
            LoadImporters();
        }

        private void LoadImporters()
        {
            if (m_importersLoaded)
            {
                return;
            }

            foreach (var fileImporter in m_extToFileImporter.Values)
            {
                var externalAssertFileImporter = fileImporter as IAssetDatabaseFileImporter;
                if (externalAssertFileImporter != null)
                {
                    externalAssertFileImporter.Load();
                }
            }

            m_importersLoaded = true;
        }

        private void OnUnloadProject(object sender, EventArgs e)
        {
            UnloadImporters();
        }

        private void UnloadImporters()
        {
            if (!m_importersLoaded)
            {
                return;
            }

            foreach (var fileImporter in m_extToFileImporter.Values)
            {
                var assertDatabaseFileImporter = fileImporter as IAssetDatabaseFileImporter;
                if (assertDatabaseFileImporter != null)
                {
                    assertDatabaseFileImporter.Unload();
                }
            }

            m_importersLoaded = false;
        }

        public IFileImporterAsync GetImporter(string ext)
        {
            if (!m_extToFileImporter.TryGetValue(ext.ToLower(), out IFileImporterDescription importer))
            {
                return null;
            }

            if (importer is IFileImporterAsync)
            {
                return (IFileImporterAsync)importer;
            }

            return null;
        }

        public IFileImporterDescription GetImporterDescription(string ext)
        {
            if (!m_extToFileImporter.TryGetValue(ext.ToLower(), out IFileImporterDescription importer))
            {
                return null;
            }

            return importer;
        }

        public Task ImportAsync(string path, string ext, CancellationToken cancelToken)
        {
            IFileImporterDescription importer = GetImporterDescription(ext);
            if (importer is IFileImporterAsync)
            {
                return ImportAsync(path, (IFileImporterAsync)importer, cancelToken);
            }

            throw new ArgumentException($"Importer for {path} not found");
        }

        public Task ImportAsync(string path, IFileImporterAsync importer, CancellationToken cancelToken)
        {
            string targetPath = GetTargetPath(path);
            return importer.ImportAsync(path, targetPath, cancelToken);
        }

        private static string GetTargetPath(string path)
        {
            var editor = IOC.Resolve<IRuntimeEditor>();
            var currentFolder = editor.CurrentFolderID;
            return editor.GetUniquePath(currentFolder, Path.GetFileNameWithoutExtension(path));
        }
    }

    [Obsolete("Use IFileImporterAsync")]
    public interface IFileImporter : IFileImporterDescription
    {
        [Obsolete("No longer called")]
        IEnumerator Import(string filePath, string targetPath);
    }

    [Obsolete("Use FileImporterAsync")]
    public abstract class FileImporter : IFileImporter
    {
        public abstract string FileExt { get; }

        public string TargetExt { get; }

        public abstract string IconPath { get; }

        public virtual int Priority
        {
            get { return 0; }
        }

        [Obsolete("No longer called")]
        public abstract IEnumerator Import(string filePath, string targetPath);
    }
}

namespace Battlehub.RTEditor
{
    using RTSL.Interface;

    [Obsolete("User Asset Database File Importer")]
    public abstract class ProjectFileImporterAsync : FileImporterAsync
    {
        public abstract Type TargetType
        {
            get;
        }

        public override Task ImportAsync(string filePath, string targetPath, CancellationToken cancelToken)
        {
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            ProjectItem target = project.Utils.Get(targetPath, TargetType);
            if (target != null)
            {
                targetPath = project.Utils.GetUniquePath(targetPath, TargetType, target.Parent);
            }

            return ImportAsync(filePath, targetPath, project, cancelToken);
        }

        public abstract Task ImportAsync(string filePath, string targetPath, IProjectAsync project, CancellationToken cancelToken);

        protected Task<byte[]> DownloadBytesAsync(string filePath)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
            IOC.Resolve<IRTE>().StartCoroutine(CoDownloadBytes(filePath, tcs));
            return tcs.Task;
        }

        private IEnumerator CoDownloadBytes(string filePath, TaskCompletionSource<byte[]> tcs)
        {
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                tcs.SetException(new UnityWebRequestException(www.error));
            }
            else
            {
                tcs.SetResult(www.downloadHandler.data);
            }
        }
    }
}
