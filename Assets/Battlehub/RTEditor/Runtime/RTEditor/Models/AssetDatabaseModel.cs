using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.Storage;
using Battlehub.Utils;
using TMPro;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public struct LoadExternalAssetArgs
    {
        public object Asset { get; set; }
        public string Key { get; set; }

        public LoadExternalAssetArgs(object asset, string key)
        {
            Asset = asset;
            Key = key;
        }
    }

    public struct ReleaseExternalAssetArgs
    {
        public object Asset { get; set; }

        public ReleaseExternalAssetArgs(object asset)
        {
            Asset = asset;
        }
    }

    public class AssetDatabaseModelImpl : IAssetDatabaseModel
    {
        private const string k_sceneExt = ".scene";
        private const string k_prefabExt = ".prefab";
        private const string k_assetExt = ".asset";
        private const string k_valueExt = ".value";

        private bool k_unloadAssetsBeforeInitializingNewScene = true;
        private bool k_unloadAssetsBeforeLoadingScene = false;

        public event EventHandler BeforeLoadProject;

        public event EventHandler LoadProject;

        public event EventHandler BeforeUnloadProject;

        public event EventHandler UnloadProject;

        public event EventHandler BeforeReloadProject;

        public event EventHandler ReloadProject;

        public event EventHandler ChangeCurrentFolder;

        public event EventHandler<AssetSelectionEventArgs> ChangeAssetSelection;

        public event EventHandler<CreateFolderEventArgs> CreateFolder;

        public event EventHandler<BeforeCreateAssetEventArgs> BeforeCreateAsset;

        public event EventHandler<CreateAssetEventArgs> CreateAsset;

        public event EventHandler<SaveAssetEventArgs> SaveAsset;

        public event EventHandler InitializeNewScene;

        public event EventHandler<SaveAssetEventArgs> UpdateAssetThumbnail;

        public event EventHandler<MoveAssetsEventArgs> BeforeMoveAssets;

        public event EventHandler<MoveAssetsEventArgs> MoveAssets;

        public event EventHandler<DuplicateAssetsEventArgs> DuplicateAssets;

        public event EventHandler<DeleteAssetsEventArgs> BeforeDeleteAssets;

        public event EventHandler<DeleteAssetsEventArgs> DeleteAssets;

        public event EventHandler<AssetEventArgs> BeforeOpenAsset;

        public event EventHandler<AssetEventArgs> OpenAsset;

        public event EventHandler<AssetEventArgs> BeforeOpenScene;

        public event EventHandler<AssetEventArgs> OpenScene;

        public event EventHandler<AssetEventArgs> BeforeOpenPrefab;

        public event EventHandler<AssetEventArgs> OpenPrefab;

        public event EventHandler<AssetEventArgs> BeforeClosePrefab;

        public event EventHandler<AssetEventArgs> ClosePrefab;

        public event EventHandler<InstancesEventArgs> InstantiateAssets;

        public event EventHandler<InstancesEventArgs> Detach;

        public event EventHandler<InstanceEventArgs> SetDirty;

        public event EventHandler<InstancesEventArgs> Duplicate;

        public event EventHandler<InstancesEventArgs> Release;

        public event EventHandler<InstanceEventArgs> BeforeApplyChanges;

        public event EventHandler<InstanceEventArgs> ApplyChanges;

        public event EventHandler<InstanceEventArgs> BeforeApplyChangesToBase;

        public event EventHandler<InstanceEventArgs> ApplyChangesToBase;

        public event EventHandler<InstanceEventArgs> BeforeRevertChangesToBase;

        public event EventHandler<InstanceEventArgs> RevertChangesToBase;

        public event Action<LoadExternalAssetArgs> LoadExternalAsset;
        public event Action<ReleaseExternalAssetArgs> ReleaseExternalAsset;

        public bool CanInitializeNewScene
        {
            get { return CurrentPrefab == null; }
        }

        public bool CanSaveScene
        {
            get { return CurrentPrefab == null; }
        }

        [SerializeField]
        private GameObject m_currentScene;

        public GameObject CurrentScene
        {
            get { return m_currentScene; }
            set { m_currentScene = value; }
        }

        public ID CurrentSceneID
        {
            get;
            private set;
        }

        public bool CanCreatePrefab(object obj)
        {
            return IsPrefabOperationAllowed(obj) && m_assetDatabase.CanCreateAsset(obj);
        }

        public bool CanCreatePrefabVariant(object obj)
        {
            return IsPrefabOperationAllowed(obj) && m_assetDatabase.CanCreateAssetVariant(obj);
        }

        public bool CanCreatePrefabVariant(ID id)
        {
            return IsPrefab(id) && m_assetDatabase.CanInstantiateAsset(id);
        }

        public bool CanSelectPrefab(object instance)
        {
            if (instance == null)
            {
                return false;
            }

            var assetID = GetAssetIDByInstance(instance);
            if (m_assetDatabase.Exists(assetID))
            {
                return true;
            }

            object asset = GetAsset(assetID);
            if (asset == null)
            {
                return false;
            }

            assetID = GetAssetIDByInstance(asset);
            return m_assetDatabase.Exists(assetID);
        }

        public bool CanOpenPrefab(object instance)
        {
            if (m_assetDatabase.IsExternalAssetInstance(instance))
            {
                return false;
            }

            if (ReferenceEquals(instance, CurrentPrefab))
            {
                object asset = m_assetDatabase.GetAssetByInstance(instance);
                return asset != null && m_assetDatabase.IsInstanceRoot(asset);
            }

            if (m_assetDatabase.IsInstanceRoot(instance))
            {
                return true;
            }

            if (m_assetDatabase.IsInstanceRootRef(instance))
            {
                return true;
            }

            return false;
        }

        public bool CanClosePrefab
        {
            get { return CurrentPrefab != null; }
        }

        private readonly HashSet<string> m_openableAssetExt = new HashSet<string>();
        public void AddOpenableAssetExt(string ext)
        {
            m_openableAssetExt.Add(ext.ToLower());
        }

        public void RemoveOpenableAssetExt(string ext)
        {
            m_openableAssetExt.Remove(ext.ToLower());
        }

        public bool CanOpenAsset(ID assetID)
        {
            if (m_assetDatabase.IsExternalAsset(assetID))
            {
                return false;
            }

            if (IsPrefab(assetID) || IsScene(assetID))
            {
                return true;
            }

            string ext = Path.GetExtension(GetPath(assetID));
            return !string.IsNullOrEmpty(ext) && m_openableAssetExt.Contains(ext.ToLower());
        }

        public bool CanEditAsset(ID assertID)
        {
            return IsLoaded(assertID) && !CanInstantiateAsset(assertID) && !IsExternalAsset(assertID);
        }

        public bool CanInstantiateAsset(ID assetID)
        {
            return m_assetDatabase.CanInstantiateAsset(assetID);
        }

        public bool CanDuplicateAsset(ID assetID)
        {
            return true;
        }

        public bool CanDetach(object[] instances)
        {
            for (int i = 0; i < instances.Length; ++i)
            {
                var instance = instances[i];
                bool canDetach = IsPrefabOperationAllowed(instance) && m_assetDatabase.CanDetach(instance) && !ReferenceEquals(instance, CurrentPrefab);
                if (!canDetach)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanDuplicate(object[] instances)
        {
            for (int i = 0; i < instances.Length; ++i)
            {
                var instance = instances[i];
                bool canDuplicate = instance != null && !ReferenceEquals(instance, CurrentPrefab) && !ReferenceEquals(instance, CurrentScene);
                if (!canDuplicate)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanRelease(object[] instances)
        {
            for (int i = 0; i < instances.Length; ++i)
            {
                var instance = instances[i];
                bool canRelease = instance != null && !ReferenceEquals(instance, CurrentPrefab) && !ReferenceEquals(instance, CurrentScene);
                if (!canRelease)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanApplyChanges(object instance)
        {
            return instance != null && m_assetDatabase.CanApplyChangesAndSaveAsync(instance, CurrentPrefab);
        }

        public bool CanApplyToBase(object instance)
        {
            return instance != null && m_assetDatabase.CanApplyChangesToBaseAndSaveAsync(instance, CurrentPrefab);
        }

        public bool CanRevertToBase(object instance)
        {
            return instance != null && m_assetDatabase.CanRevertChangesToBaseAndSaveAsync(instance, CurrentPrefab);
        }

        private GameObject m_currentHierarchyParent;
        public GameObject CurrentHierarchyParent
        {
            get
            {
                if (m_currentHierarchyParent != null)
                {
                    return m_currentHierarchyParent;
                }

                return CurrentPrefab != null ? CurrentPrefab : CurrentScene;
            }
            set
            {
                m_currentHierarchyParent = value;
            }
        }

        private readonly Stack<GameObject> m_openedPrefabs = new Stack<GameObject>();
        public GameObject CurrentPrefab
        {
            get { return m_openedPrefabs.Count > 0 ? m_openedPrefabs.Peek() : null; }
        }

        private readonly List<IAssetDatabaseProjectExtension> m_extensions = new List<IAssetDatabaseProjectExtension>();

        private readonly Dictionary<string, IExternalAssetLoaderModel> m_externalAssetLoaders = new Dictionary<string, IExternalAssetLoaderModel>();

        private readonly List<IImportSourceModel> m_importSources = new List<IImportSourceModel>();
        public IReadOnlyList<IImportSourceModel> ImportSources
        {
            get { return m_importSources; }
        }

        private string m_projectID;

        public string ProjectID
        {
            get { return m_projectID; }
        }

        private bool m_isProjectLoaded;
        public bool IsProjectLoaded
        {
            get { return m_assetDatabase != null && m_assetDatabase.IsProjectLoaded && m_isProjectLoaded; }
        }

        public ID RootFolderID
        {
            get { return (m_assetDatabase != null && m_assetDatabase.IsProjectLoaded) ? m_assetDatabase.RootID : ID.Empty; }
        }

        private const string k_libraryRootFolder = ".Library";
        public string LibraryRootFolder
        {
            get { return $"{ProjectID}/{k_libraryRootFolder}"; }
        }

        public string GetFolderInLibrary(ID assetID)
        {
            return assetID.ToString();
        }

        private const string k_valuesFolder = "Values";

        private ID m_currentFolder;
        public ID CurrentFolderID
        {
            get { return m_currentFolder; }
            set
            {
                if (m_currentFolder != value)
                {
                    m_currentFolder = value;
                    ChangeCurrentFolder?.InvokeSafe(this, EventArgs.Empty);
                }
            }
        }

        private ID[] m_selectedAssets = new ID[0];

        public ID[] SelectedAssets
        {
            get { return m_selectedAssets; }
            set
            {
                if (m_selectedAssets != value && !m_selectedAssets.SequenceEqual(value))
                {
                    var unselectedAssets = m_selectedAssets;

                    m_selectedAssets = value;

                    ChangeAssetSelection?.InvokeSafe(this, new AssetSelectionEventArgs(m_selectedAssets, unselectedAssets));
                }
            }
        }

        private IAssetDatabase m_assetDatabase;
        protected IAssetDatabase AssetDatabase
        {
            get { return m_assetDatabase; }
        }

        private GameObject m_runtimeAssetDatabaseHost;

        private ThumbnailUtil m_thumbnailUtil;
        public IThumbnailUtil ThumbnailUtil
        {
            get { return m_thumbnailUtil; }
        }

        private IShaderUtil m_shaderUtil;

        private Transform m_tempRoot;

        public void Init(Transform host)
        {
            m_tempRoot = new GameObject("TempRoot").transform;
            m_tempRoot.transform.SetParent(host);
            m_tempRoot.gameObject.SetActive(false);

            m_assetDatabase = RuntimeAssetDatabase.Instance;

            if (m_assetDatabase == null)
            {
                m_runtimeAssetDatabaseHost = new GameObject("AssetDatabase");
                m_runtimeAssetDatabaseHost.transform.SetParent(host, false);

                var assetDatabaseHostType = Type.GetType(RuntimeAssetDatabase.AssetDatabaseHostTypeName);
                if (assetDatabaseHostType == null)
                {
                    Debug.LogError("Cannot find script Battlehub.Storage.RuntimeAssetDatabaseHost. Click Tools->Runtime Asset Database->Build All");
                    return;
                }
                else
                {
                    m_runtimeAssetDatabaseHost.AddComponent(assetDatabaseHostType);
                    m_assetDatabase = RuntimeAssetDatabase.Instance;
                }
            }

            m_shaderUtil = m_assetDatabase.ShaderUtil;
            if (m_shaderUtil != null)
            {
                IOC.RegisterFallback(m_shaderUtil);
            }


            m_thumbnailUtil = host.GetComponentInChildren<ThumbnailUtil>();
            if (m_thumbnailUtil == null)
            {
                GameObject thumbnailUtilGo = new GameObject("ThumbnailUtil");
                thumbnailUtilGo.transform.SetParent(host, true);

                m_thumbnailUtil = thumbnailUtilGo.AddComponent<ThumbnailUtil>();
            }

            m_thumbnailUtil.AllowNullTexture = true;
        }

        public void Dispose()
        {
            if (m_shaderUtil != null)
            {
                IOC.UnregisterFallback(m_shaderUtil);
            }

            m_assetDatabase.ClearExternalAssetsAsync();
            m_assetDatabase = null;
            m_thumbnailUtil = null;
            m_openedPrefabs.Clear();
            CurrentScene = null;

            if (m_runtimeAssetDatabaseHost != null)
            {
                UnityObject.Destroy(m_runtimeAssetDatabaseHost);
                m_runtimeAssetDatabaseHost = null;
            }

            if (m_tempRoot != null)
            {
                UnityObject.Destroy(m_tempRoot.gameObject);
                m_tempRoot = null;
            }

            m_externalAssetLoaders.Clear();
            m_importSources.Clear();
            m_extensions.Clear();
            m_openableAssetExt.Clear();
            SerializerExtensionUtil.Reset();
        }

        public void AddRuntimeSerializableTypes(Type[] types, Guid[] typeIDs)
        {
            SerializerExtensionUtil.RegisterDynamicTypes(types);
        }

        public void AddRuntimeSerializableTypes(params Type[] types)
        {
            SerializerExtensionUtil.RegisterDynamicTypes(types);
        }

        public void RemoveRuntimeSerializableTypes(params Type[] types)
        {
            SerializerExtensionUtil.UnregisterDynamicTypes(types);
        }

        [Obsolete("Use AddRuntimeSerializableTypes")]
        public void AddRuntimeSerializableType(Type type, Guid typeID)
        {
            SerializerExtensionUtil.RegisterDynamicType(type);
        }

        [Obsolete("Use AddRuntimeSerializableTypes")]
        public void RemoveRuntimeSerializableType(Type type)
        {
            SerializerExtensionUtil.UnregisterDynamicType(type);
        }

        public void SetRuntimeTypeResolver(Func<string, Type> resolveType)
        {
            DynamicSurrogateUtils.ResolveType = resolveType;
        }

        public void AddExtension(IAssetDatabaseProjectExtension extension)
        {
            m_extensions.Add(extension);
        }

        public void RemoveExtension(IAssetDatabaseProjectExtension extension)
        {
            m_extensions?.Remove(extension);
        }

        private async Task ForEachExtension(Func<IAssetDatabaseProjectExtension, Task> func)
        {
            foreach (var extension in m_extensions)
            {
                try
                {
                    await func(extension);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private class ExternalAssetLoaderAdapter : IExternalAssetLoader
        {
            private IExternalAssetLoaderModel m_assetLoaderModel;
            private Action<LoadExternalAssetArgs> m_onLoadExternalAsset;
            private Action<ReleaseExternalAssetArgs> m_onReleaseExternalAsset;
            public ExternalAssetLoaderAdapter(IExternalAssetLoaderModel assetLoaderModel, Action<LoadExternalAssetArgs> onLoadExternalAsset, Action<ReleaseExternalAssetArgs> onReleaseExternalAsset)
            {
                m_assetLoaderModel = assetLoaderModel;
                m_onLoadExternalAsset = onLoadExternalAsset;
                m_onReleaseExternalAsset = onReleaseExternalAsset;
            }

            public async Task<object> LoadAsync(string key, object root, IProgress<float> progress = null)
            {
                var asset = await m_assetLoaderModel.LoadAsync(key, root, progress);
                m_onLoadExternalAsset?.Invoke(new LoadExternalAssetArgs(asset, key));
                return asset;
            }

            public void Release(object obj)
            {
                m_onReleaseExternalAsset?.Invoke(new ReleaseExternalAssetArgs(obj));
                m_assetLoaderModel.Release(obj);
            }
        }

        public bool HasExternalAssetLoader(string loaderID)
        {
            return m_externalAssetLoaders.ContainsKey(loaderID);
        }

        public void AddExternalAssetLoader(string loaderID, IExternalAssetLoaderModel loader)
        {
            m_externalAssetLoaders[loaderID] = loader;
        }

        public void RemoveExternalAssetLoader(string loaderID)
        {
            m_externalAssetLoaders?.Remove(loaderID);
        }

        public void AddImportSource(IImportSourceModel importSource)
        {
            m_importSources.Add(importSource);
        }

        public void RemoveImportSource(IImportSourceModel importSource)
        {
            m_importSources?.Remove(importSource);
        }

        private async Task InitLibraryFolderAsync()
        {
            if (!m_assetDatabase.Exists(k_libraryRootFolder))
            {
                await m_assetDatabase.CreateFolderAsync(k_libraryRootFolder);
            }

            if (!m_assetDatabase.Exists($"{k_libraryRootFolder}/{k_valuesFolder}"))
            {
                await m_assetDatabase.CreateFolderAsync($"{k_libraryRootFolder}/{k_valuesFolder}");
            }
        }

        private readonly Dictionary<Guid, object> m_externalAssets = new Dictionary<Guid, object>();

        private async Task RegisterExternalAssets()
        {
            await UnregisterExternalAssets();

            TMP_Settings.LoadDefaultSettings();

            var tmpFontAsset = TMP_Settings.defaultFontAsset;
            if (tmpFontAsset != null)
            {
                m_externalAssets.Add(new Guid("53d733a0-05cd-4134-a383-c3a447ebec40"), tmpFontAsset);
            }

            await m_assetDatabase.RegisterExternalAssetsAsync(m_externalAssets);
        }

        private async Task UnregisterExternalAssets()
        {
            await m_assetDatabase.UnregisterExternalAssetAsync(m_externalAssets.Keys.ToArray());
            m_externalAssets.Clear();
        }

        public async Task LoadProjectAsync(string projectID, string version)
        {
            if (IsProjectLoaded)
            {
                throw new InvalidOperationException($"First unload {m_projectID}");
            }

            m_isProjectLoaded = false;
            m_projectID = projectID;

            await RegisterExternalAssets();

            if (m_currentScene != null)
            {
                // this is to prevent 'Destroying assets is not permitted to avoid data loss.' message
                // The runtime resource database will not attempt to destroy assets with the Don't Destroy flag.
                await DontDestroySubAssetsAsync(m_currentScene);
            }

            BeforeLoadProject?.Invoke(this, EventArgs.Empty);
            foreach (var loader in m_externalAssetLoaders.Values)
            {
                var adapter = new ExternalAssetLoaderAdapter(loader, LoadExternalAsset, ReleaseExternalAsset);
                await m_assetDatabase.RegisterExternalAssetLoaderAsync(loader.LoaderID, adapter);
            }
            await m_assetDatabase.LoadProjectAsync(projectID);

            CurrentScene = GameObject.FindGameObjectWithTag(ExposeToEditor.HierarchyRootTag);

            await InitLibraryFolderAsync();
            await ForEachExtension(extension => extension.OnProjectLoadAsync(this));
            await Task.Yield();

            m_isProjectLoaded = true;
            LoadProject?.InvokeSafe(this, EventArgs.Empty);
        }

        public async Task UnloadProjectAsync()
        {
            if (!IsProjectLoaded)
            {
                return;
            }

            m_isProjectLoaded = false;

            BeforeUnloadProject?.Invoke(this, EventArgs.Empty);

            while (CurrentPrefab != null)
            {
                await CloseCurrentPrefabAsync(false);
            }

            if (m_assetDatabase != null)
            {
                await m_assetDatabase.ClearExternalAssetLoadersAsync();
                await m_assetDatabase.UnloadProjectAsync(true);
                await UnregisterExternalAssets();
                await m_assetDatabase.ClearDontDestroyFlagsAsync();
            }

            await ForEachExtension(extension => extension.OnProjectUnloadAsync());

            if (CurrentScene != null)
            {
                CurrentScene.tag = "Untagged";
                UnityObject.Destroy(CurrentScene);
                m_currentScene = null;
            }

            m_currentFolder = ID.Empty;
            m_selectedAssets = new ID[0];

            UnloadProject?.InvokeSafe(this, EventArgs.Empty);
            m_projectID = null;
        }

        public async Task ReloadProjectAsync()
        {
            BeforeReloadProject?.InvokeSafe(this, EventArgs.Empty);

            var projectID = ProjectID;
            var currentSceneID = CurrentSceneID;
            await UnloadProjectAsync();
            await LoadProjectAsync(projectID, null);

            if (Exists(CurrentSceneID))
            {
                await OpenAssetAsync(CurrentSceneID);
            }
            else
            {
                await InitializeNewSceneAsync();
            }

            ReloadProject?.InvokeSafe(this, EventArgs.Empty);
        }

        public bool IsPrefabOperationAllowed(object instance)
        {
            if (instance == null)
            {
                return false;
            }

            if (ReferenceEquals(instance, CurrentScene))
            {
                return false;
            }

            object instanceRoot = m_assetDatabase.GetInstanceRoot(instance);
            return instanceRoot == null ||
                ReferenceEquals(instanceRoot, CurrentPrefab) ||
                m_assetDatabase.IsAddedObject(instance);
        }

        public bool IsAssetRoot(object obj)
        {
            return m_assetDatabase.IsAssetRoot(obj);
        }

        public bool IsAsset(object obj)
        {
            return m_assetDatabase.IsAsset(obj);
        }

        public bool IsInstanceOfAssetVariant(object obj)
        {
            return m_assetDatabase.IsInstanceOfAssetVariant(obj);
        }

        public bool IsInstanceOfAssetVariantRef(object obj)
        {
            return m_assetDatabase.IsInstanceOfAssetVariantRef(obj);
        }

        public bool IsInstanceRoot(object obj)
        {
            return m_assetDatabase.IsInstanceRoot(obj);
        }

        public bool IsInstanceRootRef(object obj)
        {
            return m_assetDatabase.IsInstanceRootRef(obj);
        }

        public bool IsInstance(object obj)
        {
            return m_assetDatabase.IsInstance(obj);
        }

        public bool IsDirtyObject(object obj)
        {
            return m_assetDatabase.IsDirty(obj);
        }

        public bool IsAddedObject(object obj)
        {
            return m_assetDatabase.IsAddedObject(obj);
        }

        public bool HasChanges(object instance, object instanceRootOpenedForEditing)
        {
            if (instance == null)
            {
                return false;
            }

            return m_assetDatabase.HasChanges(instance, instanceRootOpenedForEditing);
        }

        private bool IsScene(in IMeta<Guid, string> meta)
        {
            return meta.FileID != null && meta.FileID.EndsWith(k_sceneExt);
        }

        public bool IsScene(ID id)
        {
            return m_assetDatabase.TryGetMeta(id, out var meta) && IsScene(meta);
        }

        private bool IsPrefab(in IMeta<Guid, string> meta)
        {
            return meta.FileID != null && meta.FileID.EndsWith(k_prefabExt);
        }

        public bool IsPrefab(ID id)
        {
            return m_assetDatabase.TryGetMeta(id, out var meta) && IsPrefab(meta);
        }

        public bool IsPrefabVariant(ID id)
        {
            return IsPrefab(id) && m_assetDatabase.TryGetAsset(id, out var asset) && m_assetDatabase.IsAssetVariant(asset);
        }

        public bool IsExternalAsset(ID id)
        {
            return m_assetDatabase.IsExternalAsset(id);
        }

        public bool IsExternalAsset(object obj)
        {
            return m_assetDatabase.IsExternalAsset(obj);
        }

        private bool IsFolder(in IMeta<Guid, string> meta)
        {
            return m_assetDatabase.IsFolder(meta.ID);
        }

        public bool IsFolder(ID id)
        {
            return m_assetDatabase.IsFolder(id);
        }

        public bool Exists(ID id)
        {
            return m_assetDatabase.Exists(id);
        }

        public Type GetType(ID id)
        {
            return m_assetDatabase.GetAssetType(id);
        }

        public ID GetTypeID(Type type)
        {
            return m_assetDatabase.GetAssetTypeIDByType(type);
        }

        public object CreateObjectOfType(Type type)
        {
            var internalUtils = m_assetDatabase as IAssetDatabaseInternalUtils<Guid, string>;
            var deps = internalUtils.Deps;
            var serializer = deps.Serializer as ISurrogatesSerializer<Guid>;
            var surrogate = serializer.CreateSurrogate(type);
            if (surrogate == null)
            {
                return null;
            }

            using var idMapRef = deps.AcquireIDMapRef();
            using var contextRef = deps.AcquireContextRef();
            var context = contextRef.Get();
            context.IDMap = idMapRef.Get();
            context.ShaderUtil = deps.ShaderUtil;

            var task = surrogate.Deserialize(context);
            while (!task.IsCompleted) { continue; }

            return task.Result;
        }


        public string GetName(ID id)
        {
            if (id == ID.Empty)
            {
                return null;
            }

            var meta = m_assetDatabase.GetMeta(id);
            return meta.Name;
        }

        public string GetDisplayName(ID id)
        {
            if (id == ID.Empty)
            {
                return null;
            }

            return Path.GetFileNameWithoutExtension(GetName(id));
        }

        public bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            return Path.GetInvalidFileNameChars().All(c => !name.Contains(c));
        }

        public string GetDisplayName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public string GetName(string path)
        {
            return Path.GetFileName(path);
        }

        private string GetName(object obj)
        {
            if (obj is UnityObject)
            {
                UnityObject uo = (UnityObject)obj;
                return uo.name;
            }

            if (obj is byte[])
            {
                return "BinaryData";
            }

            string typeName = obj.GetType().Name;
            if (IsValidName(typeName))
            {
                return typeName;
            }

            return Guid.NewGuid().ToString();
        }

        private void SetName(object obj, string name)
        {
            if (obj is UnityObject)
            {
                UnityObject uo = (UnityObject)obj;
                uo.name = name;
            }
        }

        public ID GetAssetID(string path)
        {
            return m_assetDatabase.TryGetMeta(path, out var meta) ? meta.ID : ID.Empty;
        }

        public ID GetAssetID(object asset)
        {
            return m_assetDatabase.TryGetMeta(asset, out var meta) ? meta.ID : ID.Empty;
        }

        public ID GetSubAssetID(object subAsset)
        {
            return m_assetDatabase.TryGetAssetID(subAsset, out var id) ? id :  ID.Empty;
        }

        public ID GetAssetIDByInstance(object instance)
        {
            object asset = m_assetDatabase.GetAssetByInstance(instance);
            if (asset == null)
            {
                return ID.Empty;
            }

            return m_assetDatabase.GetAssetID(asset);
        }

        public IEnumerable<GameObject> GetInstancesByAssetID(ID assetID)
        {
            return m_assetDatabase.GetInstances(assetID).OfType<GameObject>();
        }

        public bool IsLoaded(ID id)
        {
            return m_assetDatabase.GetAsset(id) != null;
        }

        public object GetAsset(ID id)
        {
            return m_assetDatabase.GetAsset(id);
        }

        public object GetAssetByInstance(object instance)
        {
            return m_assetDatabase.GetAssetByInstance(instance);
        }

        public bool IsRawData(ID id)
        {
            var assetType = GetType(id);
            return assetType == typeof(BinaryData);
        }

        public T GetRawData<T>(ID id)
        {
            var asset = GetAsset(id);
            var binaryData = asset as BinaryData;
            if (binaryData != null)
            {
                var desiredType = typeof(T);
                if (desiredType == typeof(byte[]))
                {
                    return (T)(object)binaryData.Bytes;
                }
                else if (desiredType == typeof(string))
                {
                    return (T)(object)binaryData.GetString();
                }

            }

            if (asset is T)
            {
                return (T)asset;
            }

            return default;
        }

        public void SetRawData<T>(ID id, T data)
        {
            var asset = GetAsset(id);
            var binaryData = asset as BinaryData;
            if (binaryData != null)
            {
                var targetType = typeof(T);
                if (targetType == typeof(byte[]))
                {
                    binaryData.Bytes = (byte[])(object)data;
                }
                else if (targetType == typeof(string))
                {
                    binaryData.SetString((string)(object)data);
                }
            }
        }

        public async Task<object> LoadAssetAsync(ID id)
        {
            await m_assetDatabase.LoadAssetAsync(id);
            return GetAsset(id);
        }

        public string GetPath(ID id)
        {
            if (m_assetDatabase.TryGetMeta(id, out var meta))
            {
                return meta.FileID;
            }
            return string.Empty;
        }

        public string GetExt(object obj)
        {
            if (ReferenceEquals(obj, CurrentScene))
            {
                return k_sceneExt;
            }

            if (obj is GameObject)
            {
                return k_prefabExt;
            }

            return k_assetExt;
        }

        public string GetUniquePath(string path)
        {
            return m_assetDatabase.GetUniqueFileID(path);
        }

        public string GetUniquePath(ID folderId, string desiredName)
        {
            var folderMeta = m_assetDatabase.GetMeta(folderId);
            string path = $"{folderMeta.FileID}/{desiredName}";
            return m_assetDatabase.GetUniqueFileID(path);
        }

        public byte[] GetThumbnailData(ID id)
        {
            return m_assetDatabase.GetThumbnail(id);
        }

        public async Task<byte[]> LoadThumbnailDataAsync(ID id)
        {
            await m_assetDatabase.LoadThumbnailAsync(id);
            return GetThumbnailData(id);
        }

        public ID GetParent(ID id)
        {
            return m_assetDatabase.GetParent(id);
        }

        public bool HasChildren(ID id)
        {
            return m_assetDatabase.GetChildren(id).Count > 0;
        }

        private static readonly AssetNamesComparer s_assetNameWithNumberComparer = new AssetNamesComparer();
        public IEnumerable<ID> GetChildren(ID id, bool sortByName, bool recursive, string searchPattern)
        {
            IEnumerable<Guid> children;
            if (recursive)
            {
                children = m_assetDatabase.GetChildren(id, recursive: recursive);
                if (sortByName)
                {
                    var folders = children.Where(childID => m_assetDatabase.IsFolder(childID)).OrderBy(childID => m_assetDatabase.GetMeta(childID).Name, s_assetNameWithNumberComparer);
                    var assets = children.Where(childID => !m_assetDatabase.IsFolder(childID)).OrderBy(childID => m_assetDatabase.GetMeta(childID).Name, s_assetNameWithNumberComparer);
                    children = folders.Union(assets);
                }
            }
            else
            {
                children = m_assetDatabase.GetChildren(id, sortByName: sortByName);
            }

            if (!string.IsNullOrEmpty(searchPattern))
            {
                children = children.Where(childID => GetDisplayName(childID).Contains(searchPattern, StringComparison.CurrentCultureIgnoreCase));
            }

            return children.Select(id => new ID(id)).ToArray();
        }

        public Task<IEnumerable<object>> ExtractSubAssetsAsync(object obj, ExtractSubAssetOptions options)
        {
            return Task.FromResult(ExtractSubAssets(obj, options));
        }

        private IEnumerable<object> ExtractSubAssets(object obj, ExtractSubAssetOptions options = default)
        {
            var decomposition = new RuntimeAssetEnumerable(obj);
            foreach (object subAsset in decomposition)
            {
                if (subAsset is Component)
                {
                    continue;
                }

                if (subAsset is not UnityObject)
                {
                    continue;
                }

                if (!options.IncludeExisting && m_assetDatabase.Exists(subAsset))
                {
                    continue;
                }

                if (!options.IncludeExternal && m_assetDatabase.IsExternalAsset(subAsset))
                {
                    continue;
                }

                if (!options.IncludeNonSerializable && m_assetDatabase.GetAssetTypeIDByType(subAsset.GetType()) == Guid.Empty)
                {
                    continue;
                }

                yield return subAsset;
            }
        }

        public async Task DontDestroySubAssetsAsync(object obj)
        {
            foreach (object subAsset in ExtractSubAssets(obj))
            {
                await m_assetDatabase.SetDontDestroyFlagAsync(subAsset);
            }
        }

        public Task<ID> CreateFolderAsync(string path)
        {
            return CreateFolderAsync(path, raiseEvent: true);
        }

        public async Task<ID> CreateFolderAsync(string path, bool raiseEvent)
        {
            var normalizedPath = m_assetDatabase.NormalizePath(path);
            if (m_assetDatabase.Exists(normalizedPath))
            {
                throw new ArgumentException($"Folder {path} already exists");
            }

            var parent = Path.GetDirectoryName(normalizedPath);
            var parentPath = m_assetDatabase.NormalizePath(parent);

            if (!m_assetDatabase.TryGetMeta(parentPath, out IMeta<Guid, string> parentMeta))
            {
                await CreateFolderAsync(parentPath, raiseEvent: false);
                if (!m_assetDatabase.TryGetMeta(parentPath, out parentMeta))
                {
                    throw new ArgumentException("Parent folder not found", parentPath);
                }
            }

            await m_assetDatabase.CreateFolderAsync(path);
            var meta = m_assetDatabase.GetMeta(path);
            if (raiseEvent)
            {
                var args = new CreateFolderEventArgs(meta.ID);
                CreateFolder?.InvokeSafe(this, args);
            }
            return meta.ID;
        }

        public async Task<ID> CreateAssetAsync(object obj, string path, bool variant, bool extractSubassets)
        {
            if (obj is byte[])
            {
                var binData = ScriptableObject.CreateInstance<BinaryData>();
                binData.Bytes = obj as byte[];
                obj = binData;
            }
            else if (obj is string)
            {
                var binData = ScriptableObject.CreateInstance<BinaryData>();
                binData.SetString((string)obj);
                obj = binData;
            }

            if (extractSubassets)
            {
                string folder = path;
                if (Path.HasExtension(path))
                {
                    folder = Path.GetDirectoryName(path);
                }

                foreach (var subAsset in ExtractSubAssets(obj))
                {
                    await CreateAssetAsync(subAsset, folder, false);
                }
            }

            return await CreateAssetAsync(obj, path, variant);
        }

        private async Task<ID> CreateAssetAsync(object obj, string path, bool variant)
        {
            if (!Path.IsPathRooted(path))
            {
                path = $"{GetPath(RootFolderID)}/{path}";
            }

            string ext = Path.GetExtension(path);
            string folderPath;
            if (string.IsNullOrEmpty(ext))
            {
                IAssetDatabaseModel assetDatabase = this;
                ext = assetDatabase.GetExt(obj);

                folderPath = path;
            }
            else
            {
                folderPath = Path.GetDirectoryName(path);
            }

            if (!m_assetDatabase.Exists(folderPath))
            {
                throw new ArgumentException($"Folder {folderPath} does not exist", "path");
            }

            ID overwrittenAssetID = ID.Empty;

            if (folderPath != path)
            {
                if (m_assetDatabase.Exists(path))
                {
                    overwrittenAssetID = m_assetDatabase.GetAssetIDByFileID(path);
                }
            }

            BeforeCreateAsset?.InvokeSafe(this, new BeforeCreateAssetEventArgs(obj, path));

            if (folderPath != path)
            {
                SetName(obj, Path.GetFileNameWithoutExtension(path));
            }

            Guid folderID = GetAssetID(folderPath);
            Guid assetID;
            if (ReferenceEquals(obj, CurrentScene) || string.Compare(ext, k_sceneExt, ignoreCase: true) == 0)
            {
                if (m_assetDatabase.IsAsset(obj))
                {
                    throw new ArgumentException("Cannot create a scene from an existing asset", "obj");
                }

                assetID = await CreateSceneAsync(obj, overwrittenAssetID, folderID);
            }
            else
            {
                if (overwrittenAssetID != ID.Empty)
                {
                    await m_assetDatabase.DeleteAssetAsync(overwrittenAssetID);
                }

                if (m_assetDatabase.IsAsset(obj))
                {
                    if (variant)
                    {
                        var sourceAssetID = GetAssetID(obj);

                        var copy = await m_assetDatabase.InstantiateAssetAsync(sourceAssetID, m_tempRoot);
                        SetName(obj, GetDisplayName(sourceAssetID));

                        assetID = await CreateAssetAsync(copy, overwrittenAssetID, folderID, variant, ext);
                        await m_assetDatabase.ReleaseAsync(copy);
                    }
                    else
                    {
                        Debug.LogWarning($"Use {nameof(DuplicateAssetsAsync)} method");
                        assetID = await CreateAssetAsync(obj, overwrittenAssetID, folderID, variant, ext);
                    }
                }
                else
                {
                    assetID = await CreateAssetAsync(obj, overwrittenAssetID, folderID, variant, ext);
                }
            }

            return assetID;
        }


        private async Task<ID> CreateAssetAsync(object obj, ID overwrittenAssetID, ID folderID, bool variant, string ext)
        {
            if (obj is GameObject)
            {
                if (!variant)
                {
                    if (!CanCreatePrefab(obj))
                    {
                        throw new InvalidOperationException("Can't create prefab");
                    }

                    await m_assetDatabase.DetachAsync(obj, completely: false, cloneSubAssets: true);
                }
                else
                {
                    if (!CanCreatePrefabVariant(obj))
                    {
                        throw new InvalidOperationException("Can't create prefab variant");
                    }
                }
            }

            string name = GetName(obj);
            name = name.Replace("/", "_").Replace("\\", "_"); //Replace / \\ with _ (example Universal Renderer Pipeline/Lit)

            var fileID = m_assetDatabase.GetUniqueFileID(folderID, $"{name}{ext}");
            var texture = await m_thumbnailUtil.CreateThumbnailAsync(obj);
            var thumbnail = await m_thumbnailUtil.EncodeToPngAsync(texture);

            await m_assetDatabase.CreateAssetAsync(obj, fileID, thumbnail);

            if (CurrentPrefab != null && obj is GameObject && !ReferenceEquals(obj, CurrentPrefab))
            {
                // Required when prefab is created using a child instance
                // probably needs to be done in CreateAssetAsync
                await m_assetDatabase.ApplyChangesAndSaveAsync(CurrentPrefab);
            }

            SetName(obj, Path.GetFileNameWithoutExtension(fileID));

            var assetID = m_assetDatabase.GetMeta(fileID).ID;

            var args = new CreateAssetEventArgs(assetID, overwrittenAssetID, texture);
            if (CreateAsset != null)
            {
                CreateAsset?.InvokeSafe(this, args);
            }

            UnityObject.Destroy(texture);
            return assetID;
        }

        private async Task<ID> CreateSceneAsync(object obj, ID overwrittenAssetID, ID folderID)
        {
            if (!CanSaveScene)
            {
                throw new InvalidOperationException("Can't create scene");
            }

            string fileID = m_assetDatabase.GetUniqueFileID(folderID, $"{GetName(obj)}{k_sceneExt}");
            string overwrittenFileID = null;
            if (overwrittenAssetID != ID.Empty && m_assetDatabase.TryGetMeta(overwrittenAssetID, out var meta))
            {
                overwrittenFileID = meta.FileID;
            }
         
            await m_assetDatabase.CreateAssetAsync(obj, fileID);
            await m_assetDatabase.DetachAsync(obj, completely: false, cloneSubAssets: true);
         
            SetName(obj, Path.GetFileNameWithoutExtension(fileID));

            var sceneID = m_assetDatabase.GetMeta(fileID).ID;
            CurrentSceneID = sceneID;

            await m_assetDatabase.UnloadAssetAsync(sceneID);

            for (int i = 0; i < m_selectedAssets.Length; ++i)
            {
                var seletedAssetID = m_selectedAssets[i];
                if (seletedAssetID == overwrittenAssetID)
                {
                    m_selectedAssets[i] = sceneID;
                }
            }

            if (overwrittenAssetID != ID.Empty)
            {
                await m_assetDatabase.DeleteAssetAsync(overwrittenAssetID);
                await m_assetDatabase.MoveAssetAsync(sceneID, overwrittenFileID); 
            }

            var args = new CreateAssetEventArgs(sceneID, overwrittenAssetID, null);
            CreateAsset?.InvokeSafe(this, args);
            return sceneID;
        }

        public Task<ID> ImportExternalAssetAsync(ID folderID, object key, string loaderID, string desiredName)
        {
            return ImportExternalAssetAsync(folderID, ID.Empty, key, loaderID, desiredName);
        }

        public async Task<ID> ImportExternalAssetAsync(ID folderID, ID assetID, object key, string loaderID, string desiredName)
        {
            var loader = m_assetDatabase.GetExternalAssetLoader(loaderID);
            var tempRoot = m_assetDatabase.AssetsRoot;
            var externalAsset = await loader.LoadAsync(key.ToString(), tempRoot, null);
            if (externalAsset == null)
            {
                throw new ArgumentException($"Can't load external asset with key {key}");
            }

            desiredName = (externalAsset is GameObject) ?
                    $"{desiredName}{k_prefabExt}" :
                    $"{desiredName}{k_assetExt}";

            string fileID = m_assetDatabase.GetUniqueFileID(folderID, desiredName);

            BeforeCreateAsset?.InvokeSafe(this, new BeforeCreateAssetEventArgs(externalAsset, fileID));

            await m_assetDatabase.ImportExternalAssetAsync(assetID, externalAsset, key.ToString(), loaderID, fileID);

            var obj = m_assetDatabase.GetAsset<object>(fileID);
            var texture = await m_thumbnailUtil.CreateThumbnailAsync(obj);
            var thumbnail = await m_thumbnailUtil.EncodeToPngAsync(texture);

            await m_assetDatabase.SaveThumbnailAsync(fileID, thumbnail);

            assetID = m_assetDatabase.GetMeta(fileID).ID;

            var args = new CreateAssetEventArgs(assetID, ID.Empty, texture);
            if (CreateAsset != null)
            {
                CreateAsset?.InvokeSafe(this, args);
            }

            UnityObject.Destroy(texture);
            return assetID;
        }

        public async Task ExportAssetsAsync(ID[] assets, Stream ostream, bool includeDependencies)
        {
            string projectPath = ProjectID;
            Guid[] guids = assets.Select(id => id.Guid).ToArray();

            string zipPath = $"{projectPath}.zip";
            var dataLayer = m_assetDatabase.DataLayer;
            try
            {
                await m_assetDatabase.ExportAssetsAsync(guids, zipPath);
                var zipStream = await dataLayer.OpenReadAsync(zipPath);
                try
                {
                    await TaskUtils.Run(() => zipStream.CopyTo(ostream));
                }
                finally
                {
                    await dataLayer.ReleaseAsync(zipStream);
                }
            }
            finally
            {
                await dataLayer.DeleteAsync(zipPath);
            }
        }

        public async Task ImportAssetsAsync(Stream istream)
        {
            string projectPath = ProjectID;
            string zipPath = $"{projectPath}.zip";
            var dataLayer = m_assetDatabase.DataLayer;
            try
            {
                var zipStream = await dataLayer.OpenWriteAsync(zipPath);
                try
                {
                    await TaskUtils.Run(() => istream.CopyTo(zipStream));
                }
                finally
                {
                    await dataLayer.ReleaseAsync(zipStream);
                }

                await m_assetDatabase.ImportAssetsAsync(zipPath, reloadProject:false);
                await ReloadProjectAsync();
            }
            finally
            {
                await dataLayer.DeleteAsync(zipPath);
            }
        }

        public async Task InitializeNewSceneAsync()
        {
            while (CurrentPrefab != null)
            {
                await CloseCurrentPrefabAsync(false);
            }

            if (!CanInitializeNewScene)
            {
                throw new InvalidOperationException("Can't create new scene");
            }

            if (CurrentScene != null)
            {
                if (k_unloadAssetsBeforeInitializingNewScene)
                {
                    await UnloadAllAndClearSceneAsync();
                }

                UnityObject.Destroy(CurrentScene);
            }

            await Task.Yield();

            CurrentScene = GameObject.FindGameObjectWithTag(ExposeToEditor.HierarchyRootTag);
            if (CurrentScene == null)
            {
                CurrentScene = new GameObject("Scene");
                try
                {
                    CurrentScene.tag = ExposeToEditor.HierarchyRootTag;
                }
                catch (Exception)
                {
                    Debug.LogWarning($"Add {ExposeToEditor.HierarchyRootTag} tag in Tags & Layers Window");
                    ExposeToEditor.HierarchyRootTag = "Respawn";
                    CurrentScene.tag = ExposeToEditor.HierarchyRootTag;
                }
            }

            CurrentSceneID = default;
            InitializeNewScene?.InvokeSafe(this, EventArgs.Empty);
        }

        public async Task UnloadAllAndClearSceneAsync()
        {
            if (CurrentScene != null)
            {
                await m_assetDatabase.UnloadAllAssetsAsync(destroy: true);
                await m_assetDatabase.ClearDontDestroyFlagsAsync();

                foreach (Transform child in CurrentScene.transform)
                {
                    UnityObject.Destroy(child.gameObject);
                }
            }
        }

        public async Task SaveAssetAsync(ID assetID)
        {
            SaveAssetEventArgs eventArgs = null;
            var ctx = new ThumbnailCreatorContext(m_thumbnailUtil,
                args =>
                {
                    eventArgs = new SaveAssetEventArgs(args.AssetID, args.Thumbnail != null ? UnityObject.Instantiate(args.Thumbnail) : null);
                },
                OnUpdateThumbnail,
                noThumbnailExtensions: k_sceneExt);

            await m_assetDatabase.SaveAssetAndUpdateThumbnailsAsync(assetID, ctx);

            if (eventArgs != null)
            {
                SaveAsset?.Invoke(this, eventArgs);
                if (eventArgs.Thumbnail != null)
                {
                    UnityObject.Destroy(eventArgs.Thumbnail);
                }
            }
        }

        public async Task UpdateThumbnailAsync(ID assetID)
        {
            SaveAssetEventArgs eventArgs = null;
            var ctx = new ThumbnailCreatorContext(m_thumbnailUtil, args => { },
                args =>
                {
                    eventArgs = new SaveAssetEventArgs(args.AssetID, args.Thumbnail);
                },
                noThumbnailExtensions: k_sceneExt);

            await m_assetDatabase.UpdateThumbnailAsync(assetID, ctx);

            if (eventArgs != null)
            {
                UpdateAssetThumbnail?.Invoke(this, eventArgs);
            }
        }

        public async Task MoveAssetsAsync(IReadOnlyList<ID> assetIDs, IReadOnlyList<string> toPaths)
        {
            if (this.ExcludeDescendants(assetIDs).Count() != assetIDs.Count)
            {
                throw new ArgumentException($"There are descendant IDs in the list of identifiers, exclude the descendants using the ExcludeDescendants method.", "assetIDs");
            }

            string currentFolderPath = GetPath(CurrentFolderID);
            var parentIDs = new ID[assetIDs.Count];
            var childrenIDs = new IReadOnlyList<ID>[assetIDs.Count];
            var idToPath = new Dictionary<ID, string>();
            for (int i = 0; i < assetIDs.Count; ++i)
            {
                var parent = m_assetDatabase.GetParent(assetIDs[i]);
                var children = GetChildren(assetIDs[i], sortByName: false, recursive: true, searchPattern: null).ToArray();
                var assetPath = GetPath(assetIDs[i]);

                if (currentFolderPath.StartsWith(assetPath))
                {
                    currentFolderPath = toPaths[i] + currentFolderPath.Remove(0, assetPath.Length);
                }

                idToPath.Add(assetIDs[i], toPaths[i]);

                foreach (var childID in children)
                {
                    var path = GetPath(childID);
                    path = toPaths[i] + path.Remove(0, assetPath.Length);
                    idToPath.Add(childID, path);
                }

                parentIDs[i] = parent;
                childrenIDs[i] = children;
            }

            var args = new MoveAssetsEventArgs(assetIDs, parentIDs, childrenIDs, toPaths);
            BeforeMoveAssets?.InvokeSafe(this, args);

            for (int i = 0; i < assetIDs.Count; ++i)
            {
                string fromPath = GetPath(assetIDs[i]);
                string toPath = toPaths[i];

                fromPath = m_assetDatabase.NormalizePath(fromPath);
                toPath = m_assetDatabase.NormalizePath(toPath);

                if (IsFolder(assetIDs[i]))
                {
                    await m_assetDatabase.MoveFolderAsync(fromPath, toPath);
                }
                else
                {
                    await m_assetDatabase.MoveAssetAsync(fromPath, toPath);
                }
            }

            m_currentFolder = GetAssetID(currentFolderPath);

            for (int i = 0; i < m_selectedAssets.Length; ++i)
            {
                var seletedAssetID = m_selectedAssets[i];
                if (idToPath.TryGetValue(seletedAssetID, out var selectedAssetPath))
                {
                    m_selectedAssets[i] = GetAssetID(selectedAssetPath);
                }
            }

            MoveAssets?.InvokeSafe(this, args);
        }

        public async Task DuplicateAssetsAsync(IReadOnlyList<ID> assetIDs, IReadOnlyList<string> toPaths)
        {
            if (this.ExcludeDescendants(assetIDs).Count() != assetIDs.Count)
            {
                throw new ArgumentException($"There are descendant IDs in the list of identifiers, exclude the descendants using the ExcludeDescendants method.", "assetIDs");
            }

            if (assetIDs.Any(assetID => !CanDuplicateAsset(assetID)))
            {
                throw new ArgumentException("Can't duplicate Assets", "assetIDs");
            }

            var newIDs = new ID[assetIDs.Count];
            for (int i = 0; i < assetIDs.Count; ++i)
            {
                var id = assetIDs[i];
                var toPath = toPaths[i];
                if (IsFolder(id))
                {
                    await m_assetDatabase.DuplicateFolderAsync(id, toPath);
                }
                else
                {
                    await m_assetDatabase.DuplicateAssetAsync(id, toPath);
                }

                var newID = GetAssetID(toPath);
                newIDs[i] = newID;
            }

            var args = new DuplicateAssetsEventArgs(assetIDs, newIDs);
            DuplicateAssets?.InvokeSafe(this, args);
        }

        public async Task DeleteAssetsAsync(IReadOnlyList<ID> assetIDs)
        {
            if (assetIDs.Count == 0)
            {
                return;
            }

            assetIDs = this.ExcludeDescendants(assetIDs).ToArray();

            var assetParentIDs = new ID[assetIDs.Count];
            var assetChildrenIDs = new IReadOnlyList<ID>[assetIDs.Count];
            for (int i = 0; i < assetIDs.Count; ++i)
            {
                var id = assetIDs[i];
                var children = GetChildren(id, sortByName: false, recursive: true, null).ToArray();

                assetParentIDs[i] = m_assetDatabase.GetParent(id);
                assetChildrenIDs[i] = children;

                DeleteCacheFolder(id);
                for (int j = 0; j < children.Length; ++j)
                {
                    DeleteCacheFolder(children[j]);
                }
            }

            var args = new DeleteAssetsEventArgs(assetIDs, assetParentIDs, assetChildrenIDs);
            BeforeDeleteAssets?.InvokeSafe(this, args);

            var affected = new HashSet<ID>();
            for (int i = 0; i < assetIDs.Count; ++i)
            {
                var id = assetIDs[i];
                if (IsFolder(id))
                {
                    await m_assetDatabase.DeleteFolderAsync(id);
                }
                else
                {
                    if (CurrentPrefab != null && m_assetDatabase.GetAssetIDByInstance(CurrentPrefab) == (Guid)id)
                    {
                        await ClosePrefabAsync();
                    }

                    foreach (var affectedID in m_assetDatabase.GetAssetsAffectedBy(id))
                    {
                        affected.Add(affectedID);
                    }
                    await m_assetDatabase.DeleteAssetAsync(id);

                    if (id == CurrentSceneID)
                    {
                        CurrentSceneID = default;
                    }
                }
            }

            DeleteAssets?.InvokeSafe(this, args);

            await Task.Yield();

            foreach (var id in affected)
            {
                await m_assetDatabase.UpdateThumbnailAsync(id, GetThumbnailCreatorContext());
            }

            if (CurrentScene == null)
            {
                await InitializeNewSceneAsync();
            }
        }

        private async void DeleteCacheFolder(ID assetID)
        {
            string cacheFolder = GetFolderInLibrary(assetID);
            string cacheFolderFullPath = $"{LibraryRootFolder}/{cacheFolder}";
            try
            {
                if (await m_assetDatabase.DataLayer.ExistsAsync(cacheFolderFullPath))
                {
                    await m_assetDatabase.DeleteFolderAsync(cacheFolderFullPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public Task SelectPrefabAsync(GameObject instance)
        {
            var assetID = GetAssetIDByInstance(instance);
            if (!Exists(assetID))
            {
                object asset = GetAsset(assetID);
                assetID = asset != null ? GetAssetIDByInstance(asset) : ID.Empty;
            }
            SelectedAssets = new[] { assetID };
            return Task.CompletedTask;
        }

        public async Task OpenPrefabAsync(GameObject instance)
        {
            if (!CanOpenPrefab(instance))
            {
                throw new InvalidOperationException("Can't open prefab");
            }

            Guid assetID;
            if (instance == CurrentPrefab)
            {
                object asset = m_assetDatabase.GetAssetByInstance(instance);
                if (!m_assetDatabase.IsInstanceRoot(asset))
                {
                    throw new InvalidOperationException();
                }

                assetID = m_assetDatabase.GetAssetIDByInstance(asset);
            }
            else
            {
                if (CurrentPrefab != null)
                {
                    assetID = m_assetDatabase.GetAssetIDByInstance(m_assetDatabase.GetAssetByInstance(instance));
                }
                else
                {
                    object asset = m_assetDatabase.GetAssetByInstance(instance);
                    if (m_assetDatabase.IsAssetRoot(asset))
                    {
                        assetID = m_assetDatabase.GetAssetID(asset);
                    }
                    else
                    {
                        assetID = m_assetDatabase.GetAssetIDByInstance(asset);
                    }
                }
            }

            await OpenPrefabAsync(assetID);
        }

        private async Task OpenPrefabAsync(ID assetID)
        {
            var args = new AssetEventArgs(assetID);
            BeforeOpenPrefab?.Invoke(this, args);

            if (!m_assetDatabase.IsLoaded(assetID))
            {
                await m_assetDatabase.LoadAssetAsync(assetID);
            }

            if (CurrentPrefab != null)
            {
                CurrentPrefab.transform.parent.SetParent(m_assetDatabase.AssetsRoot, true);
            }
            else
            {
                CurrentScene.SetActive(false);
            }

            GameObject prefabRoot = new GameObject("PrefabRoot");
            prefabRoot.tag = ExposeToEditor.HierarchyRootTag;
            prefabRoot.gameObject.SetActive(false);

            var instance = await m_assetDatabase.InstantiateAssetAsync<GameObject>(assetID, prefabRoot.transform);

            m_openedPrefabs.Push(instance);

            prefabRoot.gameObject.SetActive(true);

            OpenPrefab?.InvokeSafe(this, args);
        }

        public async Task ClosePrefabAsync()
        {
            if (!CanClosePrefab)
            {
                throw new InvalidOperationException("Can't close prefab");
            }

            await CloseCurrentPrefabAsync(true);
        }

        private async Task CloseCurrentPrefabAsync(bool raiseEvent)
        {
            Guid assetID = m_assetDatabase.IsInstance(CurrentPrefab) ?
                m_assetDatabase.GetAssetIDByInstance(CurrentPrefab) :
                Guid.Empty;

            if (raiseEvent)
            {
                BeforeClosePrefab?.Invoke(this, new AssetEventArgs(assetID));
            }

            if (CurrentPrefab != null)
            {
                GameObject prefabRoot = CurrentPrefab.transform.parent.gameObject;

                await m_assetDatabase.ReleaseAsync(CurrentPrefab);

                UnityObject.Destroy(prefabRoot);

                m_openedPrefabs.Pop();
            }

            if (CurrentPrefab != null)
            {
                CurrentPrefab.transform.parent.SetParent(null, true);
            }
            else
            {
                CurrentScene.SetActive(true);
            }

            if (raiseEvent)
            {
                ClosePrefab?.InvokeSafe(this, new AssetEventArgs(assetID));
            }
        }

        private async Task OpenSceneAsync(ID assetID)
        {
            var args = new AssetEventArgs(assetID);
            BeforeOpenScene?.Invoke(this, args);
            while (CurrentPrefab != null)
            {
                await CloseCurrentPrefabAsync(false);
            }

            if (k_unloadAssetsBeforeLoadingScene)
            {
                await m_assetDatabase.UnloadAllAssetsAsync(destroy: true);
            }

            if (!m_assetDatabase.IsLoaded(assetID))
            {
                await m_assetDatabase.LoadAssetAsync(assetID);
            }

            var scene = await m_assetDatabase.InstantiateAssetAsync<GameObject>(assetID, m_tempRoot);
            await m_assetDatabase.DetachAsync(scene, completely: false, cloneSubAssets: true);
            await m_assetDatabase.UnloadAssetAsync(assetID);

            if (scene != CurrentScene)
            {
                if (CurrentScene != null)
                {
                    await m_assetDatabase.ReleaseAsync(CurrentScene);
                }

                CurrentScene = scene;
                CurrentSceneID = assetID;
            }

            scene.tag = ExposeToEditor.HierarchyRootTag;
            scene.transform.hasChanged = false;
            scene.transform.SetParent(null);
            OpenScene?.InvokeSafe(this, args);
        }

        public async Task OpenAssetAsync(ID assetID)
        {
            if (!CanOpenAsset(assetID))
            {
                throw new InvalidOperationException("Can't open asset");
            }

            if (IsPrefab(assetID))
            {
                while (CurrentPrefab != null)
                {
                    await CloseCurrentPrefabAsync(false);
                }

                await OpenPrefabAsync(assetID);
            }
            else if (IsScene(assetID))
            {
                await OpenSceneAsync(assetID);
            }
            else
            {
                BeforeOpenAsset?.Invoke(this, new AssetEventArgs(assetID));
                if (!m_assetDatabase.IsLoaded(assetID))
                {
                    await m_assetDatabase.LoadAssetAsync(assetID);
                }
                OpenAsset?.InvokeSafe(this, new AssetEventArgs(assetID));
            }
        }

        public bool IsCyclicNesting(GameObject obj, Transform parent)
        {
            if (parent != null)
            {
                object asset = m_assetDatabase.GetAssetByInstance(obj);
                if (asset != null && m_assetDatabase.IsCyclicNesting(asset, parent))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<InstantiateAssetsResult> InstantiateAssetsAsync(ID[] assetIDs, Transform parent = null)
        {
            Guid sceneID = assetIDs.Where(assetID => IsScene(assetID)).FirstOrDefault();
            if (sceneID != default)
            {
                throw new ArgumentException("Can't instantiate scene. Use OpenAssetAsync method instead");
            }

            if (parent == null)
            {
                parent = CurrentHierarchyParent.transform;
            }

            assetIDs = assetIDs.Where(assetID => GetType(assetID) == typeof(GameObject)).ToArray();

            for (int i = 0; i < assetIDs.Length; ++i)
            {
                var assetID = assetIDs[i];

                await m_assetDatabase.LoadAssetAsync(assetID);
                object asset = m_assetDatabase.GetAsset(assetID);
                if (asset != null && m_assetDatabase.IsCyclicNesting(asset, parent))
                {
                    return new InstantiateAssetsResult(new GameObject[0], true);
                }
            }

            var instances = new GameObject[assetIDs.Length];
            for (int i = 0; i < assetIDs.Length; ++i)
            {
                var assetID = assetIDs[i];
                var instance = await m_assetDatabase.InstantiateAssetAsync<GameObject>(assetID, m_tempRoot);
                if (instance == null)
                {
                    instance = new GameObject(assetID.ToString());
                }
                instances[i] = instance;
            }

            for (int i = 0; i < instances.Length; ++i)
            {
                var instance = instances[i];
                instance.transform.SetParent(parent, false);
            }

            InstantiateAssets?.InvokeSafe(this, new InstancesEventArgs(instances));
            return new InstantiateAssetsResult(instances, false);
        }

        public async Task DetachAsync(GameObject[] instances, bool completely, bool cloneSubAssets)
        {
            if (!CanDetach(instances))
            {
                throw new InvalidOperationException("Can't detach");
            }

            for (int i = 0; i < instances.Length; i++)
            {
                await m_assetDatabase.DetachAsync(instances[i], completely: completely, cloneSubAssets: cloneSubAssets);
            }

            Detach?.InvokeSafe(this, new InstancesEventArgs(instances));
        }

        public async Task SetDirtyAsync(object instance)
        {
            if (!m_assetDatabase.IsInstance(instance))
            {
                return;
            }

            if (m_assetDatabase.IsDirty(instance))
            {
                return;
            }

            if (GetTypeID(instance.GetType()) == ID.Empty)
            {
                return;
            }

            await m_assetDatabase.SetDirtyAsync(instance);

            if (instance is Component component)
            {
                SetDirty?.InvokeSafe(this, new InstanceEventArgs(component.gameObject));
            }
        }

        public async Task DuplicateAsync(GameObject[] instances)
        {
            if (!CanDuplicate(instances))
            {
                throw new InvalidOperationException("Can't duplicate");
            }

            var duplicates = new GameObject[instances.Length];
            for (int i = 0; i < instances.Length; i++)
            {
                var duplicate = await m_assetDatabase.InstantiateAsync(instances[i], m_tempRoot);
                duplicates[i] = (GameObject)duplicate;
            }

            for (int i = 0; i < duplicates.Length; i++)
            {
                duplicates[i].transform.SetParent(instances[i].transform.parent, worldPositionStays: true);
            }

            Duplicate?.InvokeSafe(this, new InstancesEventArgs(duplicates));
        }

        public async Task ReleaseAsync(GameObject[] instances)
        {
            if (!CanRelease(instances))
            {
                return;
            }

            for (int i = 0; i < instances.Length; i++)
            {
                await m_assetDatabase.ReleaseAsync(instances[i]);
            }

            Release?.InvokeSafe(this, new InstancesEventArgs(instances));
        }

        public bool IsCyclicNestingAfterApplyingChanges(GameObject instance, bool toBase)
        {
            if (CurrentPrefab != null && m_assetDatabase.IsCyclicNestingAfterApplyingChanges(CurrentPrefab, false))
            {
                return true;
            }

            return instance != null && m_assetDatabase.IsCyclicNestingAfterApplyingChanges(instance, toBase);
        }

        public async Task ApplyChangesAsync(GameObject instance)
        {
            if (!CanApplyChanges(instance))
            {
                throw new InvalidOperationException("Can't apply changes");
            }

            var args = new InstanceEventArgs(instance);

            BeforeApplyChanges?.InvokeSafe(this, args);

            await m_assetDatabase.ApplyChangesAndSaveAsync(instance, CurrentPrefab, GetThumbnailCreatorContext());

            ApplyChanges?.InvokeSafe(this, args);
        }

        public async Task ApplyToBaseAsync(GameObject instance)
        {
            if (!CanApplyToBase(instance))
            {
                throw new InvalidOperationException("Can't apply to base");
            }

            var args = new InstanceEventArgs(instance);

            BeforeApplyChangesToBase?.InvokeSafe(this, args);

            await m_assetDatabase.ApplyChangesToBaseAndSaveAsync(instance, CurrentPrefab, GetThumbnailCreatorContext());

            ApplyChangesToBase?.InvokeSafe(this, args);
        }

        public async Task RevertToBaseAsync(GameObject instance)
        {
            if (!CanRevertToBase(instance))
            {
                throw new InvalidOperationException("Can't reset to base");
            }

            var args = new InstanceEventArgs(instance);

            BeforeRevertChangesToBase?.InvokeSafe(this, args);

            await m_assetDatabase.RevertChangesToBaseAndSaveAsync(instance, CurrentPrefab, GetThumbnailCreatorContext());

            RevertChangesToBase?.InvokeSafe(this, args);
        }

        public async Task<byte[]> SerializeAsync(object asset)
        {
            var serializer = m_assetDatabase as IAssetDatabaseSerializer<Guid>;
            if (serializer != null)
            {
                using (var ms = new MemoryStream())
                {
                    var id = await serializer.SerializeAsync(asset, ms);
                    return id.ToByteArray().Union(ms.ToArray()).ToArray();
                }
            }
            return new byte[0];
        }

        public async Task<object> DeserializeAsync(byte[] data, object target)
        {
            var serializer = m_assetDatabase as IAssetDatabaseSerializer<Guid>;
            if (serializer != null)
            {
                using (var ms = new MemoryStream(data))
                {
                    var id = Guid.Empty.ToByteArray();
                    ms.Read(id);
                    return await serializer.DeserializeAsync(new Guid(id), ms);
                }
            }
            return null;
        }

        public async Task<T> GetValueAsync<T>(string key)
        {
            if (!key.EndsWith(k_valueExt))
            {
                key += k_valueExt;
            }

            string path = $"{LibraryRootFolder}/{k_valuesFolder}/{key}";
            object asset = null;
            if (m_assetDatabase.Exists(path))
            {
                await m_assetDatabase.LoadAssetAsync(path);
                asset = m_assetDatabase.GetAsset(path);
            }

            if (asset == null)
            {
                return default;
            }

            var type = asset.GetType();
            if (type == typeof(BinaryData))
            {
                var binaryData = (BinaryData)asset;
                var requestedType = typeof(T);
                if (requestedType == typeof(byte[]))
                {
                    return (T)(object)binaryData.Bytes;
                }
                else if (requestedType == typeof(string))
                {
                    return (T)(object)binaryData.GetString();
                }
            }

            return (T)asset;
        }

        public async Task SetValueAsync<T>(string key, T value)
        {
            if (!key.EndsWith(k_valueExt))
            {
                key += k_valueExt;
            }

            string path = $"{LibraryRootFolder}/{k_valuesFolder}/{key}";
            if (m_assetDatabase.Exists(path))
            {
                await m_assetDatabase.DeleteAssetAsync(path);
            }

            object asset = value;
            if (asset is byte[])
            {
                var binData = ScriptableObject.CreateInstance<BinaryData>();
                binData.Bytes = asset as byte[];
                asset = binData;
            }
            else if (asset is string)
            {
                var binData = ScriptableObject.CreateInstance<BinaryData>();
                binData.SetString(asset as string);
                asset = binData;
            }

            await m_assetDatabase.CreateAssetAsync(asset, path);
        }

        public async Task DeleteValueAsync<T>(string key)
        {
            if (!key.EndsWith(k_valueExt))
            {
                key += k_valueExt;
            }

            string path = $"{LibraryRootFolder}/{k_valuesFolder}/{key}";
            if (m_assetDatabase.Exists(path))
            {
                await m_assetDatabase.DeleteAssetAsync(path);
            }
        }

        private IThumbnailCreatorContext GetThumbnailCreatorContext()
        {
            return new ThumbnailCreatorContext(m_thumbnailUtil,
                OnSaveAsset,
                OnUpdateThumbnail,
                noThumbnailExtensions: k_sceneExt);
        }

        private void OnSaveAsset(AssetThumbnailEventArgs args)
        {
            var e = new SaveAssetEventArgs(args.AssetID, args.Thumbnail);
            SaveAsset?.InvokeSafe(this, e);
        }

        private void OnUpdateThumbnail(AssetThumbnailEventArgs args)
        {
            UpdateAssetThumbnail?.InvokeSafe(this, new SaveAssetEventArgs(args.AssetID, args.Thumbnail));
        }
    }

    [DefaultExecutionOrder(-99)]
    public class AssetDatabaseModel : MonoBehaviour
    {
        private AssetsObjectModel m_objectModel;
        private AssetDatabaseModelImpl m_impl;
        private IAssetThumbnailUtil m_thumbnailUtil;

        [Serializable]
        public class Settings
        {
            public bool ExposeExternalAssetsToEditor = true;
        }

        [SerializeField]
        private Settings m_settings = new Settings();


        private void Awake()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            UnityObjectUtils.IsHiddenFunc = IsHiddedOverride;

            m_impl = new AssetDatabaseModelImpl();
            m_impl.LoadExternalAsset += OnLoadExternalAsset;
            m_impl.ReleaseExternalAsset += OnReleaseExternalAsset;
            m_impl.Init(transform);

            var extensions = GetComponentsInChildren<IAssetDatabaseProjectExtension>();
            foreach (var extension in extensions)
            {
                m_impl.AddExtension(extension);
            }

            var loaders = GetComponentsInChildren<IExternalAssetLoaderModel>();
            foreach (IExternalAssetLoaderModel loader in loaders)
            {
                m_impl.AddExternalAssetLoader(loader.LoaderID, loader);
            }

            var importSources = GetComponentsInChildren<IImportSourceModel>();
            foreach (IImportSourceModel importSource in importSources)
            {
                m_impl.AddImportSource(importSource);
            }

            m_objectModel = new AssetsObjectModel(m_impl);
            m_thumbnailUtil = GetComponent<IAssetThumbnailUtil>();
            if (m_thumbnailUtil == null)
            {
                m_thumbnailUtil = gameObject.AddComponent<AssetThumbnailUtil>();
            }
            m_thumbnailUtil.AssetDatabaseModel = m_impl;

            name = nameof(AssetDatabaseModel);

            IOC.Register<IAssetDatabaseModel>(name, m_impl);
            IOC.Register<IAssetObjectModel>(name, m_objectModel);
            IOC.Register<IAssetThumbnailUtil>(name, m_thumbnailUtil);
        }

        private void OnDestroy()
        {
            if (m_thumbnailUtil != null)
            {
                IOC.Unregister<IAssetThumbnailUtil>(name, m_thumbnailUtil);
                m_thumbnailUtil = null;
            }

            if (m_objectModel != null)
            {
                IOC.Unregister<IAssetObjectModel>(name, m_objectModel);
                m_objectModel.Dispose();
                m_objectModel = null;
            }

            if (m_impl != null)
            {
                IOC.Unregister<IAssetDatabaseModel>(name, m_impl);
                m_impl.LoadExternalAsset -= OnLoadExternalAsset;
                m_impl.ReleaseExternalAsset -= OnReleaseExternalAsset;
                m_impl.Dispose();
                m_impl = null;
            }
        }

        protected virtual void OnLoadExternalAsset(LoadExternalAssetArgs args)
        {
            if (m_settings.ExposeExternalAssetsToEditor)
            {
                GameObject go = args.Asset as GameObject;
                if (go != null)
                {
                    var exposeToEditor = go.GetComponent<ExposeToEditor>();
                    if (exposeToEditor == null)
                    {
                        go.AddComponent<ExposeToEditor>();
                    }
                }
            }
        }

        protected virtual void OnReleaseExternalAsset(ReleaseExternalAssetArgs args)
        {
        }

        private static bool IsHiddedOverride(object obj)
        {
            if (obj is Component || obj is GameObject)
            {
                var uo = (UnityObject)obj;
                return uo.hideFlags.HasFlag(HideFlags.DontSave) || uo.hideFlags == HideFlags.HideInInspector;
            }
            else if (obj is UnityObject)
            {
                var uo = (UnityObject)obj;
                return uo.hideFlags == HideFlags.DontSave || uo.hideFlags == HideFlags.HideInInspector;
            }
            return false;
        }
    }
}
