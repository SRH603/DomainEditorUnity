using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTSL;
using Battlehub.RTSL.Interface;
using Battlehub.Storage;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public class AssetDatabaseOverRTSLModelImpl : IAssetDatabaseModel
    {
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

        #pragma warning disable CS0067
        public event EventHandler<DuplicateAssetsEventArgs> DuplicateAssets;
        #pragma warning restore CS0067

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

        public bool ExposeInstancesToEditor
        {
            get;
            set;
        }

        public bool CanInitializeNewScene
        {
            get { return CurrentPrefab == null; }
        }

        public bool CanSaveScene
        {
            get { return CurrentPrefab == null; }
        }

        public bool CanCreatePrefab(object obj)
        {
            return obj is GameObject;
        }

        public bool CanCreatePrefabVariant(object obj)
        {
            return false;
        }

        public bool CanCreatePrefabVariant(ID id)
        {
            return false;
        }

        public bool CanSelectPrefab(object instance)
        {
            return false;
        }

        public bool CanOpenPrefab(object instance)
        {
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
            if (IsScene(assetID))
            {
                return true;
            }

            string ext = Path.GetExtension(GetPath(assetID));
            return !string.IsNullOrEmpty(ext) && m_openableAssetExt.Contains(ext.ToLower());
        }

        public bool CanEditAsset(ID assertID)
        {
            return true;
        }

        public bool CanInstantiateAsset(ID assetID)
        {
            return true;
        }

        public bool CanDuplicateAsset(ID assetID)
        {
            return false;
        }

        public bool CanDetach(object[] instances)
        {
            return false;
        }

        public bool CanDuplicate(object[] instances)
        {
            return instances != null && instances.Length > 0;
        }

        public bool CanRelease(object[] instances)
        {
            return true;
        }

        public bool CanApplyChanges(object instance)
        {
            return false;
        }

        public bool CanApplyToBase(object instance)
        {
            return false;
        }

        public bool CanRevertToBase(object instance)
        {
            return false;
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

                return CurrentScene;
            }
            set
            {
                m_currentHierarchyParent = value;
            }
        }
        public GameObject CurrentPrefab
        {
            get { return null; }
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

        private const string m_rootLibraryFolderName = ".Library";
        public string LibraryRootFolder
        {
            get { return $"{ProjectID}/{m_rootLibraryFolderName}"; }
        }

        public string GetFolderInLibrary(ID assetID)
        {
            return assetID.ToString();
        }

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

        private bool m_isProjectLoaded;
        public bool IsProjectLoaded
        {
            get { return m_isProjectLoaded; }
        }


        private ID m_rootFolder;
        public ID RootFolderID
        {
            get { return m_rootFolder; }
        }

        private IProjectAsync m_project;
        private RTSL.Interface.ITypeMap m_typeMap;

        
        private ThumbnailUtil m_thumbnailUtil;

        public IThumbnailUtil ThumbnailUtil
        {
            get { return m_thumbnailUtil; }
        }

        private GameObject m_tempRoot;

        public void Init(Transform host)
        {
            m_tempRoot = new GameObject("TempRoot");
            m_tempRoot.transform.SetParent(host, false);
            m_tempRoot.gameObject.SetActive(false);

            m_project = IOC.Resolve<IProjectAsync>();
            m_typeMap = IOC.Resolve<RTSL.Interface.ITypeMap>();

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
            m_project = null;
            m_typeMap = null;
            m_externalAssetLoaders.Clear();
            m_importSources.Clear();
            m_openableAssetExt.Clear();
            m_extensions.Clear();
            m_thumbnailUtil = null;
        }

        public void AddRuntimeSerializableTypes(Type[] types, Guid[] typeIDs)
        {
            for (int i = 0; i < types.Length; ++i)
            {
                m_typeMap.RegisterRuntimeSerializableType(types[i], typeIDs[i]);
            }
        }

        public void AddRuntimeSerializableTypes(params Type[] types)
        {
            throw new NotSupportedException();
        }

        public void RemoveRuntimeSerializableTypes(params Type[] types)
        {
            for (int i = 0; i < types.Length; ++i)
            {
                m_typeMap?.UnregisterRuntimeSerialzableType(types[i]);
            }
        }

        public void AddRuntimeSerializableType(Type type, Guid typeID)
        {
            m_typeMap.RegisterRuntimeSerializableType(type, typeID);
        }

        public void RemoveRuntimeSerializableType(Type type)
        {
            m_typeMap?.UnregisterRuntimeSerialzableType(type);
        }

        public void SetRuntimeTypeResolver(Func<string, Type> resolveType)
        {
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

        public void AddExternalAssetLoader(string loaderID, IExternalAssetLoaderModel loader)
        {
            m_externalAssetLoaders[loaderID] = loader;
        }

        public void RemoveExternalAssetLoader(string loaderID)
        {
            m_externalAssetLoaders.Remove(loaderID);
        }

        public void AddImportSource(IImportSourceModel importSource)
        {
            m_importSources.Add(importSource);
        }

        public void RemoveImportSource(IImportSourceModel importSource)
        {
            m_importSources?.Remove(importSource);
        }

        public async Task LoadProjectAsync(string projectID, string version)
        {
            if (IsProjectLoaded)
            {
                throw new InvalidOperationException($"First unload {m_projectID}");
            }

            if (m_project == null)
            {
                throw new InvalidOperationException("Please extract Assets/Battlehub/LegacyRTSL.unitpackage");
            }

            m_isProjectLoaded = false;
            m_projectID = NormalizePath(projectID);

            BeforeLoadProject?.Invoke(this, EventArgs.Empty);

            await m_project.Safe.OpenProjectAsync(Path.GetFileName(projectID), OpenProjectFlags.DestroyObjects | OpenProjectFlags.ClearScene);
            CurrentScene = GameObject.FindGameObjectWithTag(ExposeToEditor.HierarchyRootTag);

            m_rootFolder = new ID(m_project.State.RootFolder);
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

            await m_project.Safe.CloseProjectAsync();

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
            m_rootFolder = ID.Empty;
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
            return true;
        }

        public bool IsAssetRoot(object obj)
        {   
            if (obj is UnityObject)
            {
                return m_project.Utils.ToProjectItem((UnityObject)obj) != null;
            }
            return false;
        }

        public bool IsAsset(object obj)
        {
            if (obj is UnityObject)
            {
                return m_project.Utils.ToProjectItem((UnityObject)obj) != null;
            }
            return false;
        }

        public bool IsInstanceOfAssetVariant(object obj)
        {
            return false;
        }

        public bool IsInstanceOfAssetVariantRef(object obj)
        {
            return false;
        }

        public bool IsInstanceRoot(object obj)
        {
            return false;
        }

        public bool IsInstanceRootRef(object obj)
        {
            return false;
        }

        public bool IsInstance(object obj)
        {
            return false;
        }

        public bool IsDirtyObject(object obj)
        {
            return false;
        }

        public bool IsAddedObject(object obj)
        {
            return false;
        }

        public bool HasChanges(object instance, object instanceRootOpenedForEditing)
        {
            return false;
        }

        public bool IsScene(ID id)
        {
            var projectItem = GetRef(id);

            return projectItem != null && m_project.Utils.IsScene(projectItem);
        }

        public bool IsPrefab(ID id)
        {
            return GetType(id) == typeof(GameObject);
        }

        public bool IsPrefabVariant(ID id)
        {
            return false;
        }

        public bool IsExternalAsset(ID id)
        {
            var projectItem = GetRef(id);
            if (projectItem == null)
            {
                return false;
            }

            return m_project.Utils.IsStatic(projectItem);
        }

        public bool IsExternalAsset(object obj)
        {
            return m_project.Utils.IsStatic(obj as UnityObject);
        }

        public bool IsFolder(ID id)
        {
            var projectItem = GetRef(id);
            if (projectItem == null)
            {
                return false;
            }

            return projectItem.IsFolder;
        }

        public bool Exists(ID id)
        {
            var projectItem = GetRef(id);
            if (projectItem == null)
            {
                return false;
            }
            return projectItem.Parent != null || id == m_rootFolder;
        }

        public Type GetType(ID id)
        {
            var projectItem = GetRef(id);
            if (projectItem == null)
            {
                return null;
            }

            return m_project.Utils.ToType(projectItem);
        }

        public ID GetTypeID(Type type)
        {
            return m_project.Utils.ToGuid(type);
        }

        public object CreateObjectOfType(Type type)
        {
            var objectFactory = IOC.Resolve<IUnityObjectFactory>();
            var asset = objectFactory.CreateInstance(type, null);
            return asset;
        }

        public string GetName(ID id)
        {
            return GetRef(id)?.NameExt;
        }

        public string GetDisplayName(ID id)
        {
            return GetRef(id)?.Name;
        }

        public bool IsValidName(string name)
        {
            return ProjectItem.IsValidName(name);
        }

        public string GetDisplayName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
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
                return "Binary";
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

        public string GetName(string path)
        {
            return Path.GetFileName(path);
        }

        public ID GetAssetID(string path)
        {
            path = NormalizePath(path);
            var item = GetRef(m_rootFolder).Get(path);
            return item != null ?
                new ID(item) :
                ID.Empty;
        }

        public ID GetAssetID(object asset)
        {
            var item = m_project.Utils.ToProjectItem(asset as UnityObject);
            return item != null ?
                new ID(item) :
                ID.Empty;
        }

        public ID GetSubAssetID(object subAsset)
        {
            // Not implemented
            return GetAssetID(subAsset);
        }

        public ID GetAssetIDByInstance(object instance)
        {
            return ID.Empty;
        }

        public IEnumerable<GameObject> GetInstancesByAssetID(ID assetID)
        {
            return new GameObject[0];
        }

        public bool IsLoaded(ID id)
        {
            var item = GetRef(id);
            if (item.IsFolder)
            {
                return true;
            }

            return m_project.Utils.FromProjectItem<UnityObject>(item) != null; 
            
        }

        public object GetAsset(ID id)
        {
            var item = GetRef(id);
            if (item == null)
            {
                return null;
            }

            if (item.IsFolder)
            {
                return null;
            }

            return m_project.Utils.FromProjectItem<UnityObject>(item);
        }

        public object GetAssetByInstance(object instance)
        {
            return null;
        }

        public bool IsRawData(ID id)
        {
            var asset = GetType(id);
            return asset == typeof(RuntimeBinaryAsset) ||
                   asset == typeof(RuntimeTextAsset);
        }

        public T GetRawData<T>(ID id)
        {
            var asset = GetAsset(id);
            if (asset is RuntimeBinaryAsset)
            {
                var binaryAsset = (RuntimeBinaryAsset)asset;
                if (typeof(T) == typeof(byte[]))
                {
                    return (T)(object)binaryAsset.Data;
                }
            }
            else if (asset is RuntimeTextAsset)
            {
                var textAsset = (RuntimeTextAsset)asset;
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)textAsset.Text;
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
            if (asset is RuntimeBinaryAsset)
            {
                var binaryAsset = (RuntimeBinaryAsset)asset;
                if (typeof(T) == typeof(byte[]))
                {
                    binaryAsset.Data = (byte[])(object)data;
                }
            }
            else if (asset is RuntimeTextAsset)
            {
                var textAsset = (RuntimeTextAsset)asset;
                if (typeof(T) == typeof(string))
                {
                    textAsset.Text = (string)(object)data;
                }
            }
        }

        public async Task<object> LoadAssetAsync(ID id)
        {
            var result = await m_project.Safe.LoadAsync(new[] { GetRef(id) });
            return result?.FirstOrDefault();
        }

        public string GetPath(ID id)
        {
            var item = GetRef(id);
            if (item == null)
            {
                return string.Empty;
            }
            return item.ToString();
        }


        public string GetExt(object obj)
        {
            if (ReferenceEquals(obj, CurrentScene))
            {
                return ".rtscene";
            }

            return m_project.Utils.GetExt(obj);
        }

        public string GetUniquePath(string path)
        {
            path = NormalizePath(path);
            return m_project.Utils.GetUniquePath(path, null);
        }

        private bool ExtEqual(string ext1, string ext2)
        {
            bool isNullOrEmpty1 = string.IsNullOrEmpty(ext1);
            bool isNullOrEmpty2 = string.IsNullOrEmpty(ext2);

            if (isNullOrEmpty1 && isNullOrEmpty2)
            {
                return true;
            }

            if (isNullOrEmpty1 || isNullOrEmpty2)
            {
                return false;
            }

            return ext1.Equals(ext2, StringComparison.InvariantCultureIgnoreCase);
        }

        public string GetUniquePath(ID folderId, string desiredName)
        {
            var name = Path.GetFileNameWithoutExtension(desiredName);
            var ext = Path.GetExtension(desiredName);
            
            var folder = GetRef(folderId);
            string[] existingNames = folder.Children != null ?
                folder.Children.Where(c => ExtEqual(c.Ext, ext)).Select(c => c.Name).ToArray() :
                new string[0];
            name = m_project.Utils.GetUniqueName(name, existingNames);
            return $"{folder}/{name}{ext}";
        }

        public byte[] GetThumbnailData(ID id)
        {
            var item = GetRef(id);
            if (item != null)
            {
                return item.GetPreview();
            }
            return null;
        }

        public async Task<byte[]> LoadThumbnailDataAsync(ID id)
        {
            var item = GetRef(id);
            if (item != null)
            {
                var previews = await m_project.Safe.GetPreviewsAsync(new[] { item });
                return previews[0];
            }
            return null;
        }

        public ID GetParent(ID id)
        {
            return new ID(GetRef(id).Parent);
        }

        public bool HasChildren(ID id)
        {
            var item = GetRef(id);
            return item?.Children != null && item.Children.Count > 0;    
        }

        public IEnumerable<ID> GetChildren(ID id, bool sortByName = true, bool recursive = false, string searchPattern = null)
        {
            IEnumerable<ProjectItem> items;
            if(recursive)
            {
                var item = GetRef(id);
                if (item != null)
                {
                    items = item.Flatten(false, false);
                    items = items.Where(child => child != item && (searchPattern == null || child.Name.Contains(searchPattern))).ToArray();
                }
                else
                {
                    items = new ProjectItem[0];
                }
            }
            else
            {
                var item = GetRef(id);
                items = item?.Children != null ? 
                    item.Children.Where(child => searchPattern == null || child.Name.Contains(searchPattern)).ToArray() :
                    new ProjectItem[0];
            }

            if (sortByName)
            {
                items = items.OrderBy(item => item.Name);
            }

            return items.Select(item => new ID(item));
        }


        public async Task InitializeNewSceneAsync()
        {
            if (!CanInitializeNewScene)
            {
                throw new InvalidOperationException("Can't create new scene");
            }

            if (CurrentScene != null)
            {
                //if (k_unloadAssetsBeforeInitializingNewScene)
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
        
        public Task UnloadAllAndClearSceneAsync()
        {
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; ++i)
            {
                GameObject rootGO = rootGameObjects[i];
                if (rootGO.GetComponent<RTSLIgnore>() || (rootGO.hideFlags & HideFlags.DontSave) != 0)
                {
                    continue;
                }

                if (RTSLSettings.SaveIncludedObjectsOnly)
                {
                    if (rootGO.GetComponent<RTSLInclude>() == null)
                    {
                        continue;
                    }
                }

                UnityObject.Destroy(rootGO);
            }

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<object>> ExtractSubAssetsAsync(object asset, ExtractSubAssetOptions options = default)
        {
            var deps = await m_project.GetDependenciesAsync(asset, !options.IncludeExisting);
            return deps != null ? deps.OfType<UnityObject>() : new UnityObject[0];
        }

        public Task DontDestroySubAssetsAsync(object obj)
        {
            return Task.CompletedTask;
        }

        public async Task<ID> CreateFolderAsync(string path)
        {
            path = NormalizePath(path);

            var result = await m_project.CreateFolderAsync(path);
            var id = result != null && result.Length > 0 ? new ID(result[0]) : ID.Empty;
            var args = new CreateFolderEventArgs(id);

            CreateFolder?.InvokeSafe(this, args);

            return id;
        }

        public async Task<ID> CreateAssetAsync(object obj, string path, bool variant, bool extractSubAsset)
        {
            path = NormalizePath(path);

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

            if (obj is byte[])
            {
                var binaryAsset = ScriptableObject.CreateInstance<RuntimeBinaryAsset>();
                binaryAsset.Data = obj as byte[];
                binaryAsset.Ext = ext;
                obj = binaryAsset;
            }
            else if (obj is string)
            {
                var textAsset = ScriptableObject.CreateInstance<RuntimeTextAsset>();
                textAsset.Text = (string)obj;
                textAsset.Ext = ext;
                obj = textAsset;
            }

            BeforeCreateAsset?.InvokeSafe(this, new BeforeCreateAssetEventArgs(obj, path));

            var folderID = GetAssetID(folderPath);
            if (folderID == ID.Empty)
            {
                throw new ArgumentException($"Folder {folderPath} does not exist", "path");
            }

            Texture2D assetThumbnail = null;
            ID assetID;
            List<(ID, Texture2D)> subAssets = null;
            ID existingAssetID = ID.Empty;
            using (var l = await m_project.LockAsync())
            {         
                if (folderPath != path)
                {
                    existingAssetID = GetAssetID(path);
                    if (existingAssetID != ID.Empty)
                    {
                        await m_project.DeleteAsync(new[] { GetRef(existingAssetID) });
                    }

                    SetName(obj, Path.GetFileNameWithoutExtension(path));
                }

                var folder = GetRef(folderID);
                if (ReferenceEquals(obj, CurrentScene) || string.Compare(ext, ".rtscene", ignoreCase: true) == 0)
                {
                    var result = await m_project.SaveAsync(new[] { folder }, new[] { new byte[0] }, new[] { (object)SceneManager.GetActiveScene() }, new[] { GetName(obj) });
                    assetID = new ID(result[0]);
                }
                else
                {
                    if (obj is GameObject)
                    {
                        var result = await m_project.CreatePrefabsAsync(new[] { folder }, new[] { (GameObject)obj }, extractSubAsset, uo =>
                        {
                            return CreateThumbnailSync(uo);
                        });

                        var projectItem = m_project.Utils.ToProjectItem((GameObject)obj);
                        if (projectItem == null)
                        {
                            assetID = new ID(result[0]);
                        }
                        else
                        {
                            assetID = new ID(projectItem);

                            if (extractSubAsset)
                            {
                                subAssets = new List<(ID, Texture2D)>();
                                for (int i = 0; i < result.Length; ++i)
                                {
                                    var subAssetID = new ID(result[i]);

                                    if (assetID != subAssetID)
                                    {
                                        Texture2D thumbnail = null;
                                        var preview = result[i].GetPreview();
                                        if (preview != null && preview.Length > 0)
                                        {
                                            thumbnail = new Texture2D(1, 1);
                                            thumbnail.LoadImage(preview);
                                        }

                                        subAssets.Add((subAssetID, thumbnail));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var texture = await m_thumbnailUtil.CreateThumbnailAsync(obj);
                        var thumbnail = await m_thumbnailUtil.EncodeToPngAsync(texture);
                        
                        var result = await m_project.SaveAsync(new[] { folder }, new[] { thumbnail ?? new byte[0] }, new[] { obj });
                        assetID = new ID(result[0]);
                    }
                }
            }

            var args = new CreateAssetEventArgs(assetID, existingAssetID, assetThumbnail);
            CreateAsset?.InvokeSafe(this, args);
            UnityObject.Destroy(assetThumbnail);

            if (subAssets != null)
            {
                foreach (var (subAssetID, subAssetThumbnail) in subAssets)
                {
                    BeforeCreateAsset?.InvokeSafe(this, new BeforeCreateAssetEventArgs(null, null));
                    await Task.Yield();

                    args = new CreateAssetEventArgs(subAssetID, ID.Empty, subAssetThumbnail);
                    CreateAsset?.InvokeSafe(this, args);
                    UnityObject.Destroy(subAssetThumbnail);
                }
            }
            
            return assetID;
        }

        private byte[] CreateThumbnailSync(object obj)
        {
            var thumbnailTask = m_thumbnailUtil.CreateThumbnailAsync(obj);
            while (!thumbnailTask.IsCompleted && !thumbnailTask.IsFaulted) { }
            if (thumbnailTask.IsFaulted)
            {
                return new byte[0];
            }

            var encodeTask = m_thumbnailUtil.EncodeToPngAsync(thumbnailTask.Result);
            while (!encodeTask.IsCompleted && !encodeTask.IsFaulted) { }

            return encodeTask.Result ?? new byte[0];
        }

        public async Task<ID> ImportExternalAssetAsync(ID folderID, object key, string loaderID, string desiredName)
        {
            if (string.IsNullOrEmpty(loaderID))
            { 
                var importItem = (ImportAssetItem)key;
                importItem.Name = desiredName;

                BeforeCreateAsset?.InvokeSafe(this, new BeforeCreateAssetEventArgs(importItem.Object, importItem.ToString()));

                var result = await m_project.Safe.ImportAsync(new[] { importItem });
                var args = new CreateAssetEventArgs(new ID(result[0]), ID.Empty, null);
                if (CreateAsset != null)
                {
                    CreateAsset?.InvokeSafe(this, args);
                }

                return new ID(result[0]);
            }
            else
            {
                return await ImportExternalAssetAsync(folderID, ID.Empty, key, loaderID, desiredName);
            }
        }

        public async Task<ID> ImportExternalAssetAsync(ID folderID, ID assetID, object key, string loaderID, string desiredName)
        {
            var loader = m_externalAssetLoaders[loaderID];
            var tempRoot = m_tempRoot;
            var externalAsset = await loader.LoadAsync(key.ToString(), tempRoot, null);
            if (externalAsset == null)
            {
                throw new ArgumentException($"Can't load external asset with key {key}");
            }

            desiredName = $"{desiredName}{m_project.Utils.GetExt(externalAsset)}";

            var uo = (UnityObject)externalAsset;
            uo.name = desiredName;

            var projectItem = GetRef(assetID);
            if (projectItem == null)
            {
                projectItem = GetRef(folderID);
            }

            return await CreateAssetAsync(uo, projectItem.ToString(), false, false);
        }

        public Task ExportAssetsAsync(ID[] assets, Stream ostream, bool includeDependencies)
        {
            throw new NotSupportedException();
        }

        public Task ImportAssetsAsync(Stream istream)
        {
            throw new NotSupportedException();
        }

        public async Task SaveAssetAsync(ID assetID)
        {
            var asset = GetAsset(assetID);
            var projectItem = GetRef(assetID);

            await  m_project.Safe.SaveAsync(new[] { projectItem }, new[] { asset }, isUserAction:false);

            var thumbnail = await m_thumbnailUtil.CreateThumbnailAsync(asset);
            var thumbnailBytes = await m_thumbnailUtil.EncodeToPngAsync(thumbnail);
            projectItem.SetPreview(thumbnailBytes != null ? thumbnailBytes : new byte[0]);

            await m_project.Safe.SavePreviewsAsync(new[] { projectItem });

            SaveAsset?.InvokeSafe(this, new SaveAssetEventArgs(assetID, thumbnail));
            UpdateAssetThumbnail?.InvokeSafe(this, new SaveAssetEventArgs(assetID, thumbnail));

            UnityObject.Destroy(thumbnail);

            var dependentItems = m_project.Utils.GetProjectItemsDependentOn(new[] { projectItem }).Where(item => !m_project.Utils.IsScene(item)).ToArray();
            if (dependentItems.Length == 0)
            {
                return;
            }

            UnityObject[] loadedObjects = await m_project.LoadAsync(dependentItems);

            for (int i = 0; i < loadedObjects.Length; ++i)
            {
                UnityObject loadedObject = loadedObjects[i];
                var dependentItem = dependentItems[i];
                if (loadedObject != null)
                {
                    thumbnail = await m_thumbnailUtil.CreateThumbnailAsync(loadedObject);
                    thumbnailBytes = await m_thumbnailUtil.EncodeToPngAsync(thumbnail);
                    dependentItem.SetPreview(thumbnailBytes);
                    UpdateAssetThumbnail?.InvokeSafe(this, new SaveAssetEventArgs(new ID(dependentItem), thumbnail));
                    UnityObject.Destroy(thumbnail);
                }
                else
                {
                    dependentItem.SetPreview(null);
                }
            }

            await m_project.SavePreviewsAsync(dependentItems);
        }

        public async Task UpdateThumbnailAsync(ID assetID)
        {
            var asset = GetAsset(assetID);
            var projectItem = GetRef(assetID);

            var thumbnail = await m_thumbnailUtil.CreateThumbnailAsync(asset);
            var thumbnailBytes = await m_thumbnailUtil.EncodeToPngAsync(thumbnail);
            projectItem.SetPreview(thumbnailBytes != null ? thumbnailBytes : new byte[0]);

            await m_project.Safe.SavePreviewsAsync(new[] { projectItem });

            UpdateAssetThumbnail?.InvokeSafe(this, new SaveAssetEventArgs(assetID, thumbnail));

            UnityObject.Destroy(thumbnail);
        }

        public async Task MoveAssetsAsync(IReadOnlyList<ID> assetIDs, IReadOnlyList<string> toPaths)
        {
            if (this.ExcludeDescendants(assetIDs).Count() != assetIDs.Count)
            {
                throw new ArgumentException($"There are descendant IDs in the list of identifiers, exclude the descendants using the ExcludeDescendants method.", "assetIDs");
            }

            var parentIDs = new ID[assetIDs.Count];
            var childrenIDs = new IReadOnlyList<ID>[assetIDs.Count];
            for (int i = 0; i < assetIDs.Count; ++i)
            {
                var item = GetRef(assetIDs[i]);
                var oldParentItem = item.Parent;

                parentIDs[i] = new ID(oldParentItem);
                childrenIDs[i] = GetChildren(assetIDs[i], sortByName: false, recursive: true, searchPattern: null).ToArray();
            }

            var args = new MoveAssetsEventArgs(assetIDs, parentIDs, childrenIDs, toPaths);
            BeforeMoveAssets?.InvokeSafe(this, args);

            using (var l = await m_project.LockAsync())
            {
                await MoveAssetsInternalAsync(assetIDs, toPaths);
            }

            MoveAssets?.InvokeSafe(this, args);
        }

        private async Task MoveAssetsInternalAsync(IReadOnlyList<ID> assetIDs, IReadOnlyList<string> toPaths)
        {
            for (int i = 0; i < assetIDs.Count; ++i)
            {
                var toPath = NormalizePath(toPaths[i]);

                var item = GetRef(assetIDs[i]);
                var oldParentItem = item.Parent;

                var newParentPath = NormalizePath(Path.GetDirectoryName(toPath));
                var newName = Path.GetFileNameWithoutExtension(toPath);
                var newParentItem = m_project.Utils.Get(newParentPath, null);

                if (newParentItem.ToString() != oldParentItem.ToString())
                {
                    await m_project.MoveAsync(new[] { item }, newParentItem);
                }

                await m_project.RenameAsync(new[] { item }, new[] { newName });
            }
        }

        public Task DuplicateAssetsAsync(IReadOnlyList<ID> assetIDs, IReadOnlyList<string> toPaths)
        {
            throw new NotSupportedException();
            /*
            if (this.ExcludeDescendants(assetIDs).Count() != assetIDs.Count)
            {
                throw new ArgumentException($"There are descendant IDs in the list of identifiers, exclude the descendants using the ExcludeDescendants method.", "assetIDs");
            }

            DuplicateAssetsEventArgs args;
            using (var l = await m_project.LockAsync())
            {
                var newIDs = new ID[assetIDs.Count];
     
                var duplicates = await m_project.DuplicateAsync(assetIDs.Select(assetID => GetRef(assetID)).ToArray());
                await MoveAssetsInternalAsync(duplicates.Select(d => new ID(d)).ToArray(), toPaths);

                for (int i = 0; i < assetIDs.Count; ++i)
                {
                    var toPath = toPaths[i];
                    var newID = GetAssetID(toPath);
                    newIDs[i] = newID;
                }

                args = new DuplicateAssetsEventArgs(assetIDs, newIDs);
            }
            
            DuplicateAssets?.InvokeSafe(this, args);
            */
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

                assetParentIDs[i] = new ID(GetRef(id).Parent);
                assetChildrenIDs[i] = children;
            }

            var args = new DeleteAssetsEventArgs(assetIDs, assetParentIDs, assetChildrenIDs); 
            
            BeforeDeleteAssets?.InvokeSafe(this, args);

            await m_project.Safe.DeleteAsync(assetIDs.Select(id => GetRef(id)).ToArray());

            CurrentSceneID = default;

            DeleteAssets?.InvokeSafe(this, args);
        }

        public Task SelectPrefabAsync(GameObject instance)
        {
            return Task.CompletedTask;
        }

        public Task OpenPrefabAsync(GameObject instance)
        {
            BeforeOpenPrefab?.InvokeSafe(this, new AssetEventArgs(ID.Empty));
            OpenPrefab?.InvokeSafe(this, new AssetEventArgs(ID.Empty));
            return Task.CompletedTask;
        }

        public Task ClosePrefabAsync()
        {
            BeforeClosePrefab?.InvokeSafe(this, new AssetEventArgs(ID.Empty));
            ClosePrefab?.InvokeSafe(this, new AssetEventArgs(ID.Empty));
            return Task.CompletedTask;
        }

        public async Task OpenAssetAsync(ID assetID)
        {
            if (!CanOpenAsset(assetID))
            {
                throw new InvalidOperationException("Can't open asset");
            }

            var args = new AssetEventArgs(assetID);
            if (IsScene(assetID))
            {    
                BeforeOpenScene?.Invoke(this, args);

                await m_project.Safe.LoadAsync(new[] { GetRef(assetID) });

                CurrentSceneID = assetID;
                CurrentScene = GameObject.FindGameObjectWithTag(ExposeToEditor.HierarchyRootTag);
                if (CurrentScene == null)
                {
                    CurrentScene = new GameObject("Scene");
                    CurrentScene.tag = ExposeToEditor.HierarchyRootTag;
                }

                var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                rootObjects = RTSLSettings.SaveIncludedObjectsOnly ?
                        rootObjects.Where(go => go.GetComponent<RTSLInclude>() != null).ToArray() :
                        rootObjects.Where(go => go.GetComponent<RTSLIgnore>() == null).ToArray();
                
                for (int i = 0; i < rootObjects.Length; ++i)
                {
                    var rootObject = rootObjects[i];
                    if (rootObject == CurrentScene)
                    {
                        continue;
                    }

                    rootObject.transform.SetParent(CurrentScene.transform, true);
                }

                OpenScene?.InvokeSafe(this, args);
            }
            else
            {
                BeforeOpenAsset?.Invoke(this, args);

                await m_project.Safe.LoadAsync(new[] { GetRef(assetID) });

                OpenAsset?.InvokeSafe(this, args);
            }
        }

        public bool IsCyclicNesting(GameObject instance, Transform parent)
        {
            return false;
        }

        public async Task<InstantiateAssetsResult> InstantiateAssetsAsync(ID[] assetIDs, Transform parent = null)
        {
            ID sceneID = assetIDs.Where(assetID => IsScene(assetID)).FirstOrDefault();
            if (sceneID != default)
            {
                throw new ArgumentException("Can't instantiate scene. Use OpenAssetAsync method instead");
            }

            if (parent == null)
            {
                parent = CurrentHierarchyParent?.transform;
            }

            assetIDs = assetIDs.Where(assetID => GetType(assetID) == typeof(GameObject)).ToArray();

            var projectItems = assetIDs.Select(assetID => GetRef(assetID)).ToArray();
            var assets = await m_project.Safe.LoadAsync(projectItems);

            var instances = new List<GameObject>();
            for (int i = 0; i < assets.Length; ++i)
            {
                var asset = assets[i];
                if (asset != null && asset is GameObject)
                {
                    var instance = (GameObject)UnityObject.Instantiate(asset, m_tempRoot.transform);
                    instance.hideFlags = HideFlags.None;
                    instance.name = asset.name;
                    instances.Add(instance);
                    instance.transform.SetParent(parent);
                }
            }

            var instancesArray = instances.ToArray();
            if (ExposeInstancesToEditor)
            {
                for (int i = 0; i < instancesArray.Length; ++i)
                {
                    GameObject instance = instancesArray[i];
                    ExposeInstanceToEditor(instance);
                }
            }
            
            InstantiateAssets?.Invoke(this, new InstancesEventArgs(instancesArray));
            return new InstantiateAssetsResult(instancesArray, false);
        }

        protected virtual ExposeToEditor ExposeInstanceToEditor(GameObject instance)
        {
            ExposeToEditor exposeToEditor = instance.GetComponent<ExposeToEditor>();
            if (exposeToEditor == null)
            {
                exposeToEditor = instance.AddComponent<ExposeToEditor>();
            }
            return exposeToEditor;
        }

        public Task DetachAsync(GameObject[] instances, bool completely, bool cloneSubAssets)
        {
            Detach?.InvokeSafe(this, new InstancesEventArgs(instances));
            return Task.CompletedTask;
        }

        public Task SetDirtyAsync(object instance)
        {
            if (instance is Component component)
            {
                SetDirty?.InvokeSafe(this, new InstanceEventArgs(component.gameObject));
            }        
            return Task.CompletedTask;
        }

        public Task DuplicateAsync(GameObject[] instances)
        {
            var duplicates = new GameObject[instances.Length];
            for (int i = 0; i < instances.Length; i++)
            {
                duplicates[i] = UnityObject.Instantiate(instances[i], instances[i].transform.parent, worldPositionStays:true);
            }            
            Duplicate?.InvokeSafe(this, new InstancesEventArgs(duplicates));
            return Task.CompletedTask;
        }

        public Task ReleaseAsync(GameObject[] instances)
        {
            for (int i = 0; i < instances.Length; i++)
            {
                UnityObject.Destroy(instances[i]);
            }

            Release?.InvokeSafe(this, new InstancesEventArgs(instances));
            return Task.CompletedTask;
        }

        public bool IsCyclicNestingAfterApplyingChanges(GameObject instance, bool toBase)
        {
            return false;
        }

        public Task ApplyChangesAsync(GameObject instance)
        {
            var args = new InstanceEventArgs(instance);
            BeforeApplyChanges?.InvokeSafe(this, args);
            ApplyChanges?.InvokeSafe(this, args);
            return Task.CompletedTask;
        }

        public Task ApplyToBaseAsync(GameObject instance)
        {
            var args = new InstanceEventArgs(instance);
            BeforeApplyChangesToBase?.InvokeSafe(this, args);
            ApplyChangesToBase?.InvokeSafe(this, args);
            return Task.CompletedTask;
        }

        public Task RevertToBaseAsync(GameObject instance)
        {
            var args = new InstanceEventArgs(instance);
            BeforeRevertChangesToBase?.InvokeSafe(this, args);
            RevertChangesToBase?.InvokeSafe(this, args);
            return Task.CompletedTask;
        }

        private Type GetSurrogateType(Type type)
        {
            var typeMap = IOC.Resolve<RTSL.Interface.ITypeMap>();
            if (typeMap == null)
            {
                return null;
            }

            Type persistentType = typeMap.ToPersistentType(type);
            if (persistentType == null)
            {
                return null;
            }

            return persistentType;
        }

        public Task<byte[]> SerializeAsync(object asset)
        {
            byte[] result = null;

            var surrogateType = GetSurrogateType(asset.GetType());
            var surrogate = surrogateType != null ? (IPersistentSurrogate)Activator.CreateInstance(surrogateType) : null;
            
            if (surrogate != null)
            {
                var serializer = IOC.Resolve<RTSL.Interface.ISerializer>();
                surrogate.ReadFrom(asset);
                result = serializer.Serialize(surrogate);
            }
            return Task.FromResult(result);
        }

        public Task<object> DeserializeAsync(byte[] data, object target)
        {
            object result = null;
            if (data != null)
            {
                var serializer = IOC.Resolve<RTSL.Interface.ISerializer>();
                var surrogateType = GetSurrogateType(target.GetType());
                if (surrogateType != null)
                {
                    var surrogate = (IPersistentSurrogate)serializer.Deserialize(data, surrogateType);
                    surrogate?.WriteTo(target);
                }   
            }
            return Task.FromResult(result);
        }

        public async Task<T> GetValueAsync<T>(string key)
        {
            try
            {
                T result;
                var requestedType = typeof(T);
                if (requestedType == typeof(byte[]))
                {
                    var binaryAsset = await m_project.Safe.GetValueAsync<RuntimeBinaryAsset>(key);
                    result = binaryAsset != null ? (T)(object)binaryAsset.Data : default;
                    UnityObject.Destroy(binaryAsset);

                }
                else if (requestedType == typeof(string))
                {
                    var textAsset = await m_project.Safe.GetValueAsync<RuntimeTextAsset>(key);
                    result = textAsset != null ? (T)(object)textAsset.Text : default;
                    UnityObject.Destroy(textAsset);
                }
                else
                {
                    if (m_project != null)
                    {
                        result = await m_project.Safe.GetValueAsync<T>(key);
                    }
                    else
                    {
                        result = default;
                    }
                }
                return result;
            }
            catch (StorageException e)
            {
                if (e.ErrorCode != Error.E_NotFound)
                {
                    throw;
                }
                return default;
            }
        }

        public async Task SetValueAsync<T>(string key, T obj)
        {
            var targetType = typeof(T);
            if (targetType == typeof(byte[]))
            {
                var binaryAsset = ScriptableObject.CreateInstance<RuntimeBinaryAsset>();
                binaryAsset.Data = (byte[])(object)obj;
                await m_project.Safe.SetValueAsync(key, binaryAsset);
                UnityObject.Destroy(binaryAsset);
            }
            else if (targetType == typeof(string))
            {
                var textAsset = ScriptableObject.CreateInstance<RuntimeTextAsset>();
                textAsset.Text = (string)(object)obj;
                await m_project.Safe.SetValueAsync(key, textAsset);
                UnityObject.Destroy(textAsset);
            }
            else
            {
                await m_project.Safe.SetValueAsync(key, obj);
            }
        }

        public Task DeleteValueAsync<T>(string key)
        {
            var targetType = typeof(T);
            if (targetType == typeof(byte[]))
            {
                return m_project.Safe.DeleteValueAsync<RuntimeBinaryAsset>(key);
            }
            
            if (targetType == typeof(string))
            {
                return m_project.Safe.DeleteValueAsync<RuntimeTextAsset>(key);
            }

            return m_project.Safe.DeleteValueAsync<T>(key);
        }

        private ProjectItem GetRef(in ID id)
        {
            return (ProjectItem)id.Ref;
        }

        private string NormalizePath(string path)
        {
            return path.Replace("\\", "/");
        }
    }

    [DefaultExecutionOrder(-99)]
    public class AssetDatabaseOverRTSLModel : MonoBehaviour
    {
        [Serializable]
        public class Settings
        {
            public bool ExposeInstancesToEditor = true;
        }

        [SerializeField]
        private Settings m_settings = new Settings();

        private AssetsObjectModel m_objectModel;
        private AssetDatabaseOverRTSLModelImpl m_impl;
        private IAssetThumbnailUtil m_thumbnailUtil;

        private void Awake()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            m_impl = new AssetDatabaseOverRTSLModelImpl();
            m_impl.ExposeInstancesToEditor = m_settings.ExposeInstancesToEditor;
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

            name = nameof(AssetDatabaseOverRTSLModel);
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
                m_impl.Dispose();
                m_impl = null;
            }
        }
    }
}
