using Battlehub.RTEditor;
using System.Threading.Tasks;
using System;

using System.Threading;
using Battlehub.RTCommon;
using System.Collections.Generic;
using System.Linq;

#if UNITY_GLTFAST
using UnityEngine;
using Battlehub.RTEditor.Models;
using System.IO;
using GLTFast;
using GLTFast.Loading;
using GLTFast.Materials;
using GLTFast.Logging;
#endif

namespace Battlehub.RTImporter
{

#if UNITY_GLTFAST

    internal class GltfImportEx : GltfImport
    {
        private IDownloadProvider m_downloadProvider;
        private ICodeLogger m_logger;

        public GltfImportEx(
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null) : base(downloadProvider, deferAgent, materialGenerator, logger)
        {
            m_downloadProvider = downloadProvider ?? new DefaultDownloadProvider();
            m_logger = logger;
        }


        public async Task<string[]> GetDependencies(string url)
        {
            var download = await m_downloadProvider.Request(new Uri(url, UriKind.RelativeOrAbsolute));
            var success = download.Success;
            if (!success)
            {
                m_logger?.Error(LogCode.Download, download.Error, url.ToString());
                return null;
            }

            var deps = new HashSet<string>();
            var root = ParseJson(download.Text);
            if (root.Images != null)
            {
                foreach (var image in root.Images)
                {
                    if (!string.IsNullOrEmpty(image.uri) && !image.uri.StartsWith("data:"))
                    {
                        deps.Add(image.uri);
                    }
                }
            }
            
            if (root.Buffers != null)
            {
                foreach (var buffer in root.Buffers)
                {
                    if (!string.IsNullOrEmpty(buffer.uri) && !buffer.uri.StartsWith("data:"))
                    {
                        deps.Add(buffer.uri);
                    }
                }
            }
          
            return deps.ToArray();
        }
    }

    public class GLTFastLoaderModel : IExternalAssetLoaderModel
    {
        private ImportSettings m_settings = new ImportSettings
        {
            GenerateMipMaps = true,
            AnisotropicFilterLevel = 3,
            NodeNameMethod = NameImportMethod.OriginalUnique
        };

        public string LoaderID => nameof(GLTFastLoaderModel);

        public string LibraryFolder
        {
            get;
            set;
        }

        public Task<string[]> GetDependencies(string url)
        {
            var gltf = new GltfImportEx();
            return gltf.GetDependencies(url);
        }

        public async Task<object> LoadAsync(string key, object root, IProgress<float> progress = null)
        {
            string path;
            if (Uri.TryCreate(key, UriKind.Absolute, out _) || Path.IsPathRooted(key))
            {
                path = key;
            }
            else
            {
                path = !string.IsNullOrEmpty(LibraryFolder) ? $"{LibraryFolder}/{key}" : key;
            }

            var gltf = new GltfImportEx();
            var success = await gltf.Load(path, m_settings);
            if (!success)
            {
                throw new ArgumentException($"Loading glTF failed! {path}", "key");
            }

            var go = new GameObject(key);
            go.transform.SetParent(root as Transform);
            await gltf.InstantiateMainSceneAsync(go.transform);

            var parts = go.GetComponentsInChildren<Transform>(true);
            foreach (var part in parts)
            {
                part.gameObject.AddComponent<ExposeToEditor>();
            }

            return go;
        }

        public void Release(object obj)
        {
            GameObject go = obj as GameObject;
            if (go != null)
            {
                UnityEngine.Object.Destroy(go);
            }
        }
    }
#endif

    public class GlbImporter : GltfImporter
    {
        public override string FileExt
        {
            get { return ".glb"; }
        }
#if UNITY_GLTFAST
        protected override Task<string> AddFilesToLibraryAsync(string filePath, Guid assetID)
        {
            return AddFileToLibraryAsync(filePath, assetID);
        }
#endif
    }

    public class GltfImporter : AssetDatabaseFileImporter
    {
        public override int Priority
        {
            get { return int.MinValue; }
        }

        public override string FileExt
        {
            get { return ".gltf"; }
        }

        public override string IconPath
        {
            get { return "Importers/GLTF"; }
        }

#if UNITY_GLTFAST
        private GLTFastLoaderModel m_assetLoader;
        public override void Load()
        {
            base.Load();

            var gltFastLoader = new GLTFastLoaderModel();
            gltFastLoader.LibraryFolder = Editor.LibraryRootFolder;
            m_assetLoader = gltFastLoader;

            Editor.AddExternalAssetLoader(m_assetLoader.LoaderID, m_assetLoader);
        }

        public override void Unload()
        {
            Editor.RemoveExternalAssetLoader(m_assetLoader.LoaderID);

            m_assetLoader = null;

            base.Unload();
        }

        protected virtual async Task<string> AddFilesToLibraryAsync(string filePath, Guid assetID)
        {
            string parentPath = GetBasePath(filePath);
            string[] dependencies = await m_assetLoader.GetDependencies(filePath);
            if (dependencies != null)
            {
                foreach (string dependencyPath in dependencies)
                {
                    if (!IsUrl(dependencyPath))
                    {
                        await AddFileToLibraryAsync(parentPath, dependencyPath, assetID);
                    }
                }
            }

            return await AddFileToLibraryAsync(filePath, assetID);
        }

        public override async Task ImportAsync(string filePath, string targetPath, CancellationToken cancelToken)
        {
            try
            {
                Guid assetID = Guid.NewGuid();

                string externalAssetKey = await AddFilesToLibraryAsync(filePath, assetID);

                await ImportExternalAsset(targetPath, assetID, externalAssetKey, m_assetLoader);
            }
            catch (Exception e)
            {
                throw new FileImporterException(e.Message, e);
            }
        }

#else
        public override Task ImportAsync(string filePath, string targetPath, CancellationToken cancelToken)
        {
            throw new InvalidOperationException("Install com.unity.cloud.gltfast using the package manager");
        }
#endif
    }
}
