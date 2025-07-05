using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Battlehub.Storage
{
    public class RuntimeAssetDatabase : RuntimeAssetDatabaseCore<Guid, string, Meta<Guid, string>, Thumbnail, ExternalData<Guid>>, IAssetDatabase, IDisposable
    {
        public const string AssetDatabaseHostTypeName = "Battlehub.Storage.RuntimeAssetDatabaseHost, Assembly-CSharp";
        public const string ModuleDependenciesTypeName = "Battlehub.Storage.ModuleDependencies, Assembly-CSharp";

        public Transform AssetsRoot
        {
            get { return Deps.AssetsRoot as Transform; }
        }

        public IDataLayer<string> DataLayer
        {
            get { return Deps.DataLayer; }
        }

        public IShaderUtil ShaderUtil
        {
            get { return Deps.ShaderUtil; }
        }

        public static IAssetDatabase Instance
        {
            get;
            private set;
        }

        internal static IModuleDependencies<Guid, string> Deps
        {
            get;
            private set;
        }

        public RuntimeAssetDatabase(IModuleDependencies<Guid, string> deps, ExternalAssetList[] externalAssets = null) : base(deps)
        {
            if (Instance != null)
            {
                Debug.Log("Another instance of RuntimeAssetDatabase exists");
                ClearInstance();
            }

            // Fix:UnityException: get_INTERNAL_defaultRenderPipeline can only be called from the main thread.
            // Dummy code to run the RenderPipelineInfo static constructor from the main thread
            if (RenderPipelineInfo.Type == RPType.Unknown) { Debug.Log("RPType.Unknown"); }

            RegisterExternalAssets(externalAssets);

            Instance = this;
            Deps = deps;
        }

        protected override IEqualityComparer<string> GetFileIDComparer()
        {
            return RuntimeAssetDatabaseExtensions.s_ignoreCaseComparer;
        }

        public void Dispose()
        {
            if (Instance == this)
            {
                ClearInstance();
            }

            ClearExternalAssetLoadersImpl();
            ClearExternalAssetsImpl();
        }

        private static void ClearInstance()
        {
            Instance = null;
            Deps = null;
            SerializerExtensionUtil.Reset();
        }

        private void RegisterExternalAssets(ExternalAssetList[] externalAssets)
        {
            if (externalAssets == null)
            {
                return;
            }

            var defaultUIMaterial = Canvas.GetDefaultCanvasMaterial();
            if (defaultUIMaterial != null)
            {
                RegisterExternalAsset(new Guid("be6e7ffb-2b3d-4b47-bf85-5767e942b9e9"), defaultUIMaterial);
            }

            foreach (ExternalAssetList list in externalAssets)
            {
                foreach (var item in list.Items)
                {
                    if (TryGetExternalAsset(item.ID, out var existingAsset))
                    {
                        Debug.LogWarning($"An asset {existingAsset} with the same id {item.ID} is already registered");
                        continue;
                    }

                    if (TryGetExternalAssetID(item.Asset, out var existingAssetID))
                    {
                        Debug.LogWarning($"The same asset {item.Asset} is already registered, but with a different id {existingAssetID}.");
                        continue;
                    }

                    RegisterExternalAsset(item.ID, item.Asset);
                }
            }
        }

        private static bool IsPrefab(GameObject go)
        {
            return go.scene == null || go.scene.buildIndex < 0;
        }

        protected override bool IsInstantiableType(Type type)
        {
            if (typeof(UnityObject).IsAssignableFrom(type))
            {
                return IsInstantiableAssetType(type);
            }

            return type.IsClass;
        }

        protected override bool IsInstantiableAssetType(Type type)
        {
            return type == typeof(GameObject) || typeof(Component).IsAssignableFrom(type);
        }

        protected override object InstantiateObject(object obj, object parent)
        {
            var uo = obj as UnityObject;
            if (uo == null)
            {
                throw new NotSupportedException("Can't instantiate non UnityObject");
            }

            Transform parentTransform = parent as Transform;
            var instance = UnityObject.Instantiate(uo, parentTransform);
            instance.name = uo.name;
            return instance;
        }

        protected override void SetObjectParent(object obj, object parent)
        {
            var uo = obj as UnityObject;
            if (uo == null)
            { 
                return;
            }

            if (uo is GameObject)
            {
                var go = (GameObject)uo;
                bool isPrefab = IsPrefab(go);
                if (!isPrefab)
                {
                    if (parent != null)
                    {
                        var p = parent as Transform;
                        if (p == null)
                        {
                            Debug.LogWarning("Can't set parent of GameObject to non GameObject");
                            return;
                        }

                        go.transform.SetParent(p, true);

                    }
                    else
                    {
                        go.transform.SetParent(null, true);
                    }
                }
            }
            else if (uo is Component)
            {
                var c = (Component)uo;
                bool isPrefab = IsPrefab(c.gameObject);
                if (!isPrefab)
                {
                    if (parent != null)
                    {
                        var p = parent as Transform;
                        if (p == null)
                        {
                            Debug.LogWarning("Can't set parent of Component to non GameObject");
                            return;
                        }

                        c.transform.SetParent(p, true);

                    }
                    else
                    {
                        c.transform.SetParent(null, false);
                    }
                }
            }
        }

        protected override object GetObjectParent(object obj)
        {
            var uo = obj as UnityObject;
            if (uo == null)
            {
                return null;
            }

            if (uo is GameObject)
            {
                GameObject go = (GameObject)uo;
                return go.transform.parent;
            }
            else if (uo is Component)
            {
                var c = (Component)uo;
                return c.transform.parent;
            }

            return null;
        }

        protected override void SetObjectName(object obj, string name)
        {
            var uo = obj as UnityObject;
            if (uo != null)
            {
                uo.name = Path.GetFileNameWithoutExtension(name);
            }
        }

        protected override string GetObjectName(object obj)
        {
            var uo = obj as UnityObject;
            if (uo != null)
            {
                return uo.name;
            }

            return base.GetObjectName(obj);
        }

        protected override object GetObjectRootRepresentation(object obj, int mask = 0)
        {
            if (mask == 0)
            {
                if (obj is Transform)
                {
                    Transform transform = (Transform)obj;
                    if (transform != null)
                    {
                        return transform.gameObject;
                    }
                }
            }
            else
            {
                if (obj is Component)
                {
                    Component component = (Component)obj;
                    if (component != null)
                    {
                        return component.gameObject;
                    }
                }
            }

            return null;
        }

        protected override bool IsObjectDestoryed(object obj)
        {
            return UnityObjectUtils.IsNullOrDestroyed(obj) || base.IsObjectDestoryed(obj);
        }

        protected override bool IsObjectHidden(object obj)
        {
            return UnityObjectUtils.IsHidden(obj);
        }

        protected override void DestroyObject(object obj, bool immediate)
        {
            if (obj is UnityObject)
            {
                if (HasDontDestroyFlag(obj))
                {
                    ClearDontDestroyFlag(obj);
                }
                else
                {
                    var uo = (UnityObject)obj;
                    if (uo == null)
                    {
                        return;
                    }

                    if (uo is Transform)
                    {
                        uo = ((Transform)uo).gameObject;
                    }

                    if (immediate)
                    {
                        UnityObject.DestroyImmediate(uo);
                    }
                    else
                    {
                        UnityObject.Destroy(uo);
                    }
                }

                ClearDirtyImpl(obj);
            }
            else
            {
                base.DestroyObject(obj, immediate);
            }
        }

        protected override bool IsEqualToAsset(object instance, IAssetMap<Guid> assetMap)
        {
            if (instance is Transform)
            {
                Transform instanceTransform = (Transform)instance;

                if (assetMap.TryGetAssetByInstance(instance, out var asset))
                {
                    Transform assetTransform = (Transform)asset;

                    return instanceTransform.localPosition == assetTransform.localPosition &&
                        instanceTransform.localRotation == assetTransform.localRotation &&
                        instanceTransform.localScale == assetTransform.localScale;
                }
            }

            return true;
        }

        [Obsolete]
        protected override Meta<Guid, string> CreateMeta(Guid id, int typeId, string dataFileID, string metaFileID)
        {
            return base.CreateMeta(id, typeId, null, metaFileID);
        }

        protected override Meta<Guid, string> CreateMeta(Guid id, int typeID, string dataFileID, string thumbnailID, string metaFileID, object asset)
        {
            var meta = base.CreateMeta(id, typeID, null, null, metaFileID, asset);
            meta.Name = Path.GetFileName(dataFileID);
            return meta;
        }

        protected override bool TryGetMeta(string fileID, out Meta<Guid, string> meta)
        {
            fileID = NormalizeFileID(fileID);

            if (!Path.GetFileName(fileID).StartsWith(".") && Path.HasExtension(fileID))
            {
                // fileID has extension, look for meta file

                if (fileID.EndsWith(".meta"))
                {
                    return base.TryGetMeta(fileID, out meta);
                }

                return base.TryGetMeta($"{fileID}.meta", out meta);
            }

            // fileID without extension indicates that it may be a folder

            if (base.TryGetMeta(fileID, out meta))
            {
                return true;
            }

            // if the folder is not found, try to find the .meta of the asset without extension

            return base.TryGetMeta($"{fileID}.meta", out meta);
        }

        protected override string NormalizeFileID(string fileID)
        {
            return this.NormalizePath(fileID);
        }

   
        protected override bool IsMetaFileID(string fileID)
        {
            if(string.IsNullOrEmpty(fileID))
            {
                return false;
            }

            string ext = Path.GetExtension(fileID);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }

            return ext.ToLower() == ".meta";
        }

        private string GetFileID(in Meta<Guid, string> meta)
        {
            return NormalizeFileID(meta.FileID);
        }

        protected override string GetMetaFileID(in Meta<Guid, string> meta)
        {
            if (IsFolder(meta))
            {
                return GetFileID(meta);
            }

            return $"{GetFileID(meta)}.meta";
        }

        protected override void SetMetaFileID(ref Meta<Guid, string> meta, string fileID)
        {
            meta.FileID = NormalizeFileID(PathUtils.GetFilePathWithoutExtension(fileID));
        }

        protected override string GetDataFileID(in Meta<Guid, string> meta)
        {
            return GetFileID(meta);
        }

        protected override void SetDataFileID(ref Meta<Guid, string> meta, string dataFileID)
        {
            meta.DataFileID = null;
        }

        protected override string GetThumbnailFileID(in Meta<Guid, string> meta)
        {
            return $"{GetFileID(meta)}.thumb";
        }

        protected override void SetThumbnailFileID(ref Meta<Guid, string> meta, string thumbnailFileID)
        {
            meta.ThumbnailFileID = null;
        }

        protected virtual string NormalizeProjectID(string projectID)
        {
            if (Path.IsPathRooted(projectID))
            {
                return PathUtils.NormalizePath(projectID);
            }

            //return $"{Application.persistentDataPath}/{projectID}";
            return PathUtils.NormalizePath(projectID);
        }

        protected override async Task LoadProjectAsyncImpl(string projectID)
        {
            projectID = NormalizeProjectID(projectID);

            IAssetDatabaseInternalUtils<Guid, string> utils = this;
            var dataLayer = utils.Deps.DataLayer;

            bool exists = await dataLayer.ExistsAsync(projectID);
            if (!exists)
            {
                await dataLayer.CreateFolderAsync(projectID);
            }

            await base.LoadProjectAsyncImpl(projectID);
        }

        protected override Task CreateAssetAsyncImpl(object asset, string fileID, byte[] thumbnail, string thumbnailID, string dataFileID, Guid parentID, string externalLoaderID, string externalAssetKey)
        {
            if (!string.IsNullOrEmpty(dataFileID))
            {
                Debug.LogWarning("dataFileID is ignored for runtime asset database");
            }

            if (!string.IsNullOrEmpty(thumbnailID))
            {
                Debug.LogWarning("dataFileID is ignored for runtime asset database");
            }

            dataFileID = NormalizeFileID(fileID);
            fileID = $"{dataFileID}.meta";
            thumbnailID = $"{dataFileID}.thumb";

            var parentPath = Path.GetDirectoryName(fileID);
            if (!TryGetMeta(parentPath, out Meta<Guid, string> parentMeta))
            {
                throw new ArgumentException("Parent folder not found", parentPath);
            }

            return base.CreateAssetAsyncImpl(asset, fileID, thumbnail, thumbnailID, dataFileID, parentMeta.ID, externalLoaderID, externalAssetKey);
        }

        protected override async Task MoveAssetAsyncImpl(Guid assetID, string newFileID, string newDataFileID, string newThumbnailID, Guid newParentID)
        {
            if (!string.IsNullOrEmpty(newDataFileID))
            {
                Debug.LogWarning("dataFileID is ignored for runtime asset database");
            }

            if (!string.IsNullOrEmpty(newThumbnailID))
            {
                Debug.LogWarning("newThumbnailID is ignored for runtime asset database");
            }

            if (newParentID != Guid.Empty)
            {
                Debug.LogWarning("parentID is ignored for runtime asset database");
            }

            newDataFileID = NormalizeFileID(newFileID);
            newFileID = $"{newFileID}.meta";
            newThumbnailID = $"{newDataFileID}.thumb";

            string newParentFolder = Path.GetDirectoryName(newFileID);
            if (!TryGetMeta(newParentFolder, out Meta<Guid, string> parentMeta))
            {
                throw new ArgumentException("Parent folder not found", newParentFolder);
            }

            if (TryGetMeta(assetID, out Meta<Guid, string> meta))
            {
                meta.Name = Path.GetFileNameWithoutExtension(newFileID);
                SetMeta(meta.ID, meta);
            }

            await base.MoveAssetAsyncImpl(assetID, newFileID, newDataFileID, newThumbnailID, parentMeta.ID);
        }

        protected override async Task CreateFolderAsyncImpl(string folderID, string name, Guid parentID)
        {
            var normalizedPath = NormalizeFileID(folderID);
            var parent = Path.GetDirectoryName(normalizedPath);
            var parentPath = NormalizeFileID(parent);

            if (!TryGetMeta(parentPath, out Meta<Guid, string> parentMeta))
            {
                await CreateFolderAsyncImpl(parentPath, null, default);
                if (!TryGetMeta(parentPath, out parentMeta))
                {
                    throw new ArgumentException("Parent folder not found", parentPath);
                }
            }

            await base.CreateFolderAsyncImpl(normalizedPath, Path.GetFileName(normalizedPath), parentMeta.ID);
        }

        protected override Task DeleteFolderAsyncImpl(string folderID)
        {
            return base.DeleteFolderAsyncImpl(NormalizeFileID(folderID));
        }

        public async Task DuplicateFolderAsync(Guid id, string newFolderID)
        {
            using (await LockAsync())
            {
                await DuplicateFolderAsyncImpl(id, newFolderID);
            }
        }

        protected async Task DuplicateFolderAsyncImpl(Guid id, string newFolderID)
        {
            newFolderID = this.NormalizePath(newFolderID);
            if (this.Exists(newFolderID))
            {
                throw new ArgumentException($"Folder with {newFolderID} already exists");
            }

            await CreateFolderAsyncImpl(newFolderID, null, default);

            var folderMeta = this.GetMeta(id);
            var folderID = this.NormalizePath(folderMeta.FileID);
            var folderRx = new Regex(Regex.Escape(folderID));

            var children = this.GetChildren(id);
            for (int i = 0; i < children.Count; ++i)
            {
                var meta = this.GetMeta(children[i]);

                string childFileID = this.NormalizePath(meta.FileID);
                string newChildFileID = folderRx.Replace(childFileID, newFolderID, 1);

                if (IsFolder(meta.ID))
                {
                    await DuplicateFolderAsyncImpl(meta.ID, newChildFileID);
                }
                else
                {
                    await DuplicateAssetAsyncImpl(meta.ID, newChildFileID);
                }
            }
        }

        public async Task DuplicateAssetAsync(Guid assetID, string newFileID)
        {
            using (await LockAsync())
            {
                await DuplicateAssetAsyncImpl(assetID, newFileID);
            }
        }

        protected async Task DuplicateAssetAsyncImpl(Guid assetID, string duplicateFileID)
        {
            if (!this.IsLoaded(assetID))
            {
                await LoadAssetAsyncImpl(assetID);
            }

            if (!this.IsThumbnailLoaded(assetID))
            {
                await LoadThumbnailAsyncImpl(assetID);
            }

            object asset = this.GetAsset(assetID);
            IAssetDatabaseInternalUtils<Guid, string> utils = this;
            IIDMap<Guid> idMap;
            if(!utils.RootAssetMap.TryGetIDMapByRootAsset(asset, out idMap))
            {
                idMap = null;
            }
            
            if (!CanInstantiateAsset(assetID)) // is not instantiable by means of runtime asset database
            {
                var binData = asset as BinaryData;
                if (binData != null)
                {
                    var copyData = ScriptableObject.CreateInstance<BinaryData>();
                    if (binData.Bytes != null)
                    {
                        var copyBytes = new byte[binData.Bytes.Length];
                        Buffer.BlockCopy(binData.Bytes, 0, copyBytes, 0, copyBytes.Length);
                        copyData.Bytes = copyBytes;
                    }

                    asset = copyData;
                }
                else
                {
                    var uo = asset as UnityObject;
                    if (uo != null)
                    {
                        asset = UnityObject.Instantiate(uo);
                    }
                }
            }
            else
            {
                if (IsExternalAssetRoot(asset))
                {
                    var uo = asset as UnityObject;
                    if (uo != null)
                    {
                        throw new InvalidOperationException("Can't duplicate external asset");
                    }
                }
            }

            byte[] thumbnailData = this.GetThumbnail(assetID);
            //Create duplicate 
            await CreateAssetAsyncImpl(asset, duplicateFileID, thumbnailData, null, null, default, null, null);

            if (asset is not BinaryData)
            {
                var duplicateMeta = this.GetMeta(duplicateFileID);

                await LoadAssetAsyncImpl(duplicateMeta.ID);

                var duplicateAsset = this.GetAsset(duplicateFileID);

                if (idMap != null)
                {
                    // temporary remove references to internal assets to allow create duplicates
                    idMap.Rollback();
                    try
                    {
                        using (var ms = new MemoryStream())
                        {
                            var id = await SerializeToStreamAsyncImpl(duplicateAsset, ms);
                            ms.Position = 0;

                            var obj = await DeserializeFromStreamAsyncImpl(id, ms);
                            duplicateAsset = obj;
                        }

                        await SaveAssetAsyncImpl(duplicateMeta.ID, null, false);
                        await UnloadAssetAsyncImpl(duplicateMeta.ID, true);
                    }
                    finally
                    {
                        idMap.Commit();
                    }
                }
            }
        }
    
        public bool IsCyclicNesting(object asset, Transform parentTransform)
        {
            var deps = new HashSet<object>();
            var gameObjects = new HashSet<object> { asset };
            var decomposition = new RuntimeAssetEnumerable(asset);
            foreach (var subAsset in decomposition)
            {
                if (subAsset == null)
                {
                    continue;
                }

                if (subAsset is Component)
                {
                    object go = ((Component)subAsset).gameObject;
                    do
                    {
                        if (TryGetMeta(go, out Meta<Guid, string> _))
                        {
                            gameObjects.Add(go);
                        }
                        go = this.GetAssetByInstance(go);
                    }
                    while (go != null);   
                }
                else
                {
                    deps.Add(subAsset);
                }
            }

            foreach (GameObject go in gameObjects)
            {
                if (!TryGetMeta(go, out Meta<Guid, string> meta))
                {
                    continue;
                }

                var parent = parentTransform;
                Guid assetID = meta.ID;
                while (parent != null)
                {
                    var parentAsset = this.GetAssetByInstance(parent);
                    if (parentAsset != null)
                    {
                        if (TryGetMeta(parentAsset, out Meta<Guid, string> parentMeta))
                        {
                            if (assetID == parentMeta.ID)
                            {
                                return true;
                            }

                            foreach (var subAsset in deps)
                            {
                                if (TryGetMeta(subAsset, out Meta<Guid, string> subAssetMeta))
                                {
                                    if (subAssetMeta.ID == parentMeta.ID)
                                    {
                                        return true;
                                    }
                                }
                            }

                            if (IsCyclicNesting(go.transform, parentMeta.ID))
                            {
                                return true;
                            }
                        }
                    }

                    parent = parent.parent;
                }
            }

            return false;
        }

        private bool IsCyclicNesting(Transform child, Guid parentID)
        {
            var childAsset = this.GetAssetByInstance(child);
            if (childAsset != null && IsAssetRoot(childAsset))
            {
                if (TryGetMeta(childAsset, out Meta<Guid, string> childMeta))
                {
                    if (parentID == childMeta.ID)
                    {
                        return true;
                    }
                }
            }

            int childCount = child.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                if (IsCyclicNesting(child.GetChild(i), parentID))
                {
                    return true;
                }
            }
            return false;
        }

        private Transform GetParentTransform(object obj)
        {
            var go = obj as GameObject;
            return go != null ? go.transform : null;
        }


        public bool IsCyclicNestingAfterApplyingChanges(object instance, bool toBase)
        {
            using var enumeratorRef = Deps.AcquireEnumeratorRef(instance);
            var enumerator = enumeratorRef.Get();

            Transform parentTransform;
            if (toBase)
            {
                if (!TryGetAssetByInstance(instance, out var asset))
                {
                    return false;
                }

                parentTransform = GetParentTransform(asset);
            }
            else
            {
                parentTransform = GetParentTransform(instance);
            }

            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                if (current == null)
                {
                    continue;
                }

                object rootRepresentation = GetObjectRootRepresentation(current);
                if (rootRepresentation == null)
                {
                    if (toBase && !HasInstantiableType(current))
                    {
                        if (!TryGetAssetID(current, out var assetID))
                        {
                            // this will lead to cycling nesting:
                            // 1. prefab variant will store this asset (material, mesh, etc.)
                            // 2. the base prefab will need this asset to load correctly and will try to load a variant of the prefab
                            // 3. prefab variant will try to load the base prefab
                            // 4. cyclic nesting

                            Debug.LogWarning($"Applying changes to the base will result in cyclic nesting. {current} will be stored in a prefab variant file that depends on the base prefab, which in turn will depend on {current}. Register {current} as a separate or external asset to prevent cyclic nesting.");

                            return true;
                        }

                        // prefab variant already contains asset. Applying changes to base will lead to cyclic nesting
                        if (TryGetAssetRoot(current, out var assetRoot) && assetRoot == this.GetAssetByInstance(instance))
                        {
                            Debug.LogWarning($"Applying changes to the base will result in cyclic nesting. {current} is stored in a prefab variant file that depends on the base prefab, which in turn will depend on {current}. Register {current} as a separate or external asset to prevent cyclic nesting.");

                            return true;
                        }
                    }

                    continue;
                }

                if (!TryGetAssetByInstance(rootRepresentation, out var rootAsset))
                {
                    continue;
                }

                if (toBase ? (IsInstanceRoot(rootAsset) || IsInstanceRoot(rootRepresentation)) : IsInstanceRoot(rootRepresentation))
                {
                    if (rootRepresentation == instance)
                    {
                        continue;
                    }

                    if (IsCyclicNesting(rootAsset, parentTransform))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HasChanges(object instance, object instanceRootOpenedForEditing)
        {
            if (HasChanges(instance))
            {
                return true;
            }

            if (instanceRootOpenedForEditing == null ||
                instanceRootOpenedForEditing == instance) // fix issues/40
            {
                return false;
            }

            if (!TryGetAssetByInstance(instance, out object asset))
            {
                return false;
            }

            return IsInstanceRoot(asset) &&  // fix issue/41
                   HasChanges(asset);

        }
        
        public bool CanApplyChangesAndSaveAsync(object instance, object instanceRootOpenedForEditing)
        {
            if (!IsPrefabOperationAllowed(instance, instanceRootOpenedForEditing))
            {
                return false;
            }
            
            if (!CanApplyChangesTo(instance, instanceRootOpenedForEditing))
            {
                return false;
            }

            if (!HasChanges(instance, instanceRootOpenedForEditing))
            {
                return false;
            }

            return true;
        }

        private bool CanApplyChangesTo(object instance, object instanceRootOpenedForEditing)
        {
            object instanceRoot = this.GetInstanceRoot(instance);
            if (instanceRoot != null)
            {
                if (this.IsAddedObject(instance))
                {
                    return CanApplyChanges(instance, afterApplyingChangesToRoot: false);
                }

                Debug.Assert(ReferenceEquals(instanceRoot, instanceRootOpenedForEditing));
                return CanApplyChanges(instanceRoot, afterApplyingChangesToRoot: false) && CanApplyChanges(instance, afterApplyingChangesToRoot: true);
            }

            return CanApplyChanges(instance, afterApplyingChangesToRoot: false);
        }

        public bool CanApplyChangesToBaseAndSaveAsync(object instance, object instanceRootOpenedForEditing)
        {
            if (!IsPrefabOperationAllowed(instance, instanceRootOpenedForEditing))
            {
                return false;
            }

            if (!this.IsInstanceOfAssetVariant(instance))
            {
                return false;
            }

            if (!CanApplyChanges(instance, afterApplyingChangesToRoot: false))
            {
                return false;
            }
            
            if (!TryGetAssetByInstance(instance, out var asset))
            {
                return false;
            }

            if (!CanApplyChanges(asset, afterApplyingChangesToRoot: false))
            {
                return false;
            }
              
            if (!(HasChanges(instance) || HasChanges(asset)))
            {
                return false;
            }

            return true;
        }

        public bool CanRevertChangesToBaseAndSaveAsync(object instance, object instanceRootOpenedForEditing)
        {
            if (!IsPrefabOperationAllowed(instance, instanceRootOpenedForEditing))
            {
                return false;
            }
            if (!this.IsInstanceOfAssetVariant(instance))
            {
                return false;
            }
            
            if (!CanRevertChanges(instance))
            {
                return false;
            }

            return true;
        }

        private bool IsPrefabOperationAllowed(object instance, object instanceRootOpenedForEditing)
        {
            if (instance == null)
            {
                return false;
            }

            object instanceRoot = this.GetInstanceRoot(instance);
            return instanceRoot == null ||
                ReferenceEquals(instanceRoot, instanceRootOpenedForEditing) ||
                this.IsAddedObject(instance);
        }

        public async Task ApplyChangesAndSaveAsync(object instance, object instanceRootOpenedForEditing, IThumbnailCreatorContext context)
        {
            using (await LockAsync())
            {
                if (instanceRootOpenedForEditing != null && IsCyclicNestingAfterApplyingChanges(instanceRootOpenedForEditing, false))
                {
                    throw new InvalidOperationException("Cyclic Prefab nesting not supported");
                }

                if (IsCyclicNestingAfterApplyingChanges(instance, false))
                {
                    throw new InvalidOperationException("Cyclic Prefab nesting not supported");
                }

                await ApplyChangesAndSaveAsyncImpl(instance, instanceRootOpenedForEditing, context);
            }
        }

        protected async Task ApplyChangesAndSaveAsyncImpl(object instance, object instanceRootOpenedForEditing, IThumbnailCreatorContext context)
        {
            if (!CanApplyChangesAndSaveAsync(instance, instanceRootOpenedForEditing))
            {
                throw new InvalidOperationException("Can't apply changes and save");
            }

            object instanceRoot = this.GetInstanceRoot(instance);
            if (instanceRoot != null)
            {
                await ApplyChangesAndSaveAsyncImpl(instanceRoot, context);
            }

            await ApplyChangesAndSaveAsyncImpl(instance, context);
        }

        public async Task ApplyChangesToBaseAndSaveAsync(object instance, object instanceRootOpenedForEditing, IThumbnailCreatorContext context)
        {
            using (await LockAsync())
            {
                if (instanceRootOpenedForEditing != null && IsCyclicNestingAfterApplyingChanges(instanceRootOpenedForEditing, false))
                {
                    throw new InvalidOperationException("Cyclic Prefab nesting not supported");
                }

                if (IsCyclicNestingAfterApplyingChanges(instance, false))
                {
                    throw new InvalidOperationException("Cyclic Prefab nesting not supported");
                }

                if (instanceRootOpenedForEditing != null && IsCyclicNestingAfterApplyingChanges(instanceRootOpenedForEditing, true))
                {
                    throw new InvalidOperationException("Cyclic Prefab nesting not supported");
                }

                if (IsCyclicNestingAfterApplyingChanges(instance, true))
                {
                    throw new InvalidOperationException("Cyclic Prefab nesting not supported");
                }

                await ApplyChangesToBaseAndSaveAsyncImpl(instance, instanceRootOpenedForEditing, context);
            }
        }

        protected async Task ApplyChangesToBaseAndSaveAsyncImpl(object instance, object instanceRootOpenedForEditing, IThumbnailCreatorContext context)
        {
            if (!CanApplyChangesToBaseAndSaveAsync(instance, instanceRootOpenedForEditing))
            {
                throw new InvalidOperationException("Can't apply changes to base and save");
            }

            object instanceRoot = this.GetInstanceRoot(instance);
            if (instanceRoot != null)
            {
                await ApplyChangesAndSaveAsyncImpl(instanceRoot, context);
            }
            await ApplyChangesAndSaveAsyncImpl(instance, context);

            object asset = this.GetAssetByInstance(instance);
            object assetInstanceRoot = this.GetInstanceRoot(asset);
            if (assetInstanceRoot != null)
            {
                await ApplyChangesAndSaveAsyncImpl(assetInstanceRoot, context);
            }
            await ApplyChangesAndSaveAsyncImpl(asset, context);
        }

        public async Task RevertChangesToBaseAndSaveAsync(object instance, object instanceRootOpenedForEditing, IThumbnailCreatorContext context)
        {
            using (await LockAsync())
            {
                await RevertChangesToBaseAndSaveAsyncImpl(instance, instanceRootOpenedForEditing, context);
            }
        }

        protected async Task RevertChangesToBaseAndSaveAsyncImpl(object instance, object instanceRootOpenedForEditing, IThumbnailCreatorContext context)
        {
            if (!CanRevertChangesToBaseAndSaveAsync(instance, instanceRootOpenedForEditing))
            {
                throw new InvalidOperationException("Can't revert changes to base and save");
            }

            object instanceRoot = this.GetInstanceRoot(instance);
            if (instanceRoot != null)
            {
                await ApplyChangesAndSaveAsyncImpl(instanceRoot, context);
            }

            await RevertChangesToBaseAndSaveAsyncImpl(instance, context);
        }

        public async Task ApplyChangesAndSaveAsync(object instance, IThumbnailCreatorContext context)
        {
            using (await LockAsync())
            {
                if (IsCyclicNestingAfterApplyingChanges(instance, false))
                {
                    throw new InvalidOperationException("Cyclic Prefab nesting not supported");
                }

                await ApplyChangesAndSaveAsyncImpl(instance, context);
            }
        }

        protected async Task ApplyChangesAndSaveAsyncImpl(object instance, IThumbnailCreatorContext context)
        {
            if (context == null)
            {
                context = new ThumbnailCreatorContext();
            }

            var assetIDs = await ApplyChangesAsyncImpl(instance, strict: false);
            foreach (var assetID in assetIDs)
            {
                await SaveAssetAndThumbnailAsyncImpl(assetID, context);
            }

            var affectedIDs = this.GetAssetsAffectedBy(assetIDs);
            foreach (var assetID in affectedIDs)
            {
                await UpdateThumbnailAsyncImpl(assetID, context);
            }
        }

        public async Task RevertChangesToBaseAndSaveAsync(object instance, IThumbnailCreatorContext context)
        {
            using (await LockAsync())
            {
                await RevertChangesToBaseAndSaveAsyncImpl(instance, context);
            }
        }

        protected async Task RevertChangesToBaseAndSaveAsyncImpl(object instance, IThumbnailCreatorContext context)
        {
            if (context == null)
            {
                context = new ThumbnailCreatorContext();
            }

            var assetIDs = await RevertChangesAsyncImpl(instance);
            foreach (var assetID in assetIDs)
            {
                if (!IsExternalAsset(assetID))
                {
                    await SaveAssetAndThumbnailAsyncImpl(assetID, context);
                }
            }

            var affectedIDs = this.GetAssetsAffectedBy(assetIDs);
            foreach (var assetID in affectedIDs)
            {
                if (!IsExternalAsset(assetID))
                {
                    await UpdateThumbnailAsyncImpl(assetID, context);
                }
            }
        }

        public async Task SaveAssetAndUpdateThumbnailsAsync(Guid assetID, IThumbnailCreatorContext context)
        {
            using (await LockAsync())
            {
                await SaveAssetAndThumbnailAsyncImpl(assetID, context);

                var affectedIDs = this.GetAssetsAffectedBy(assetID);
                foreach (var affectedAssetID in affectedIDs)
                {
                    await UpdateThumbnailAsyncImpl(affectedAssetID, context);
                }
            }
        }

        private async Task<Texture2D> CreateThumbnailTextureAsync(Guid assetID, IThumbnailCreatorContext ctx)
        {
            if (!TryGetMeta(assetID, out Meta<Guid, string> meta) || !ctx.CanCreateThumbnail(meta.FileID))
            {
                return null;
            }

            await LoadAssetAsyncImpl(assetID);
            if (CanInstantiateAsset(assetID))
            {
                var target = (UnityObject)await InstantiateAssetAsyncImpl(assetID, parent : AssetsRoot, detachInstance: true, copyDirtyFlags: false);
                if (target is GameObject)
                {
                    GameObject go = (GameObject)target;
                    PrepareForThumbnail(go);
                    go.transform.SetParent(null);
                }

                var texture = await ctx.CreateThumbnailAsync(target, instantiate: false);
                UnityObject.Destroy(target);
                return texture;
            }
            else
            {
                var target = this.GetAsset<UnityObject>(assetID);
                return await ctx.CreateThumbnailAsync(target);
            }
        }

        protected virtual void PrepareForThumbnail(GameObject go)
        {
            var withRequireComponent = new List<MonoBehaviour>();
            MonoBehaviour[] scripts = go.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < scripts.Length; ++i)
            {
                var script = scripts[i];
                if (script == null)
                {
                    continue;
                }

                var type = script.GetType();
                if (type.FullName.StartsWith("UnityEngine"))
                {
                    scripts[i] = null;
                }
                else
                {
                    var requireComponent = type.GetCustomAttributes<RequireComponent>();
                    if (requireComponent.Any())
                    {
                        withRequireComponent.Add(script);
                    }
                }
            }

            for (int i = 0; i < withRequireComponent.Count; ++i)
            {
                var script = withRequireComponent[i];
                if (script == null)
                {
                    continue;
                }

                try
                {
                    UnityObject.DestroyImmediate(script);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }

            for (int i = 0; i < scripts.Length; ++i)
            {
                var script = scripts[i];
                if (script == null)
                {
                    continue;
                }

                UnityObject.DestroyImmediate(script);
            }
        }

        private async Task SaveAssetAndThumbnailAsyncImpl(Guid assetID, IThumbnailCreatorContext context)
        {
            var texture = await CreateThumbnailTextureAsync(assetID, context);
            var thumbnail = texture != null ?
                await context.EncodeThumbnailAsync(texture) :
                new byte[0];

            bool readOnlyIDMap = false;
            if (TryGetObject(assetID, out var asset))
            {
                readOnlyIDMap = HasInstantiableAssetType(asset);
            }

            await SaveAssetAsyncImpl(assetID, thumbnail, readOnlyIDMap);

            if (context.OnSaveAsset != null)
            {
                context.OnSaveAsset.Invoke(new AssetThumbnailEventArgs(assetID, texture));
            }

            if (texture != null)
            {
                UnityObject.Destroy(texture);
            }
        }

        public async Task UpdateThumbnailAsync(Guid assetID, IThumbnailCreatorContext context)
        {
            using (await LockAsync())
            {
                await UpdateThumbnailAsyncImpl(assetID, context);
            }
        }

        protected async Task UpdateThumbnailAsyncImpl(Guid assetID, IThumbnailCreatorContext context)
        {
            if (context == null)
            {
                context = new ThumbnailCreatorContext();
            }

            if (!TryGetMeta(assetID, out Meta<Guid, string> meta) || !context.CanCreateThumbnail(meta.FileID))
            {
                return;
            }

            if (!this.IsLoaded(assetID))
            {
                await LoadAssetAsyncImpl(assetID);
            }

            var texture = await CreateThumbnailTextureAsync(assetID, context);
            var thumbnail = await context.EncodeThumbnailAsync(texture);

            await SaveThumbnailAsyncImpl(assetID, thumbnail);

            if (context.OnUpdateThumbnail != null)
            {
                context.OnUpdateThumbnail.Invoke(new AssetThumbnailEventArgs(assetID, texture));
            }

            if (texture != null)
            {
                UnityObject.Destroy(texture);
            }
        }
    }

    public struct AssetThumbnailEventArgs
    {
        public Guid AssetID
        {
            get;
            private set;
        }

        public Texture2D Thumbnail
        {
            get;
            private set;
        }

        public AssetThumbnailEventArgs(Guid assetID, Texture2D texture)
        {
            AssetID = assetID;
            Thumbnail = texture;
        }
    }

    public interface IThumbnailCreatorContext
    {
        public Action<AssetThumbnailEventArgs> OnSaveAsset { get; }

        public Action<AssetThumbnailEventArgs> OnUpdateThumbnail { get; }

        public bool CanCreateThumbnail(string fileID);

        public Task<Texture2D> CreateThumbnailAsync(object obj, bool instantiate = true);

        public Task<byte[]> EncodeThumbnailAsync(Texture2D texture);
    }

    public class ThumbnailCreatorContext : IThumbnailCreatorContext
    {
        public Action<AssetThumbnailEventArgs> OnSaveAsset { get; }

        public Action<AssetThumbnailEventArgs> OnUpdateThumbnail { get; }

        public bool CanCreateThumbnail(string fileID)
        {
            return m_thumbnailUtil != null && Array.IndexOf(m_excludedExtensions, Path.GetExtension(fileID)) < 0;
        }

        public Task<byte[]> EncodeThumbnailAsync(Texture2D texture)
        {
            if (m_thumbnailUtil == null)
            {
                return Task.FromResult<byte[]>(null);
            }

            return m_thumbnailUtil.EncodeToPngAsync(texture);
        }

        public Task<Texture2D> CreateThumbnailAsync(object obj, bool instantiate)
        {
            var uo = obj as UnityObject;
            if (m_thumbnailUtil == null || uo == null)
            {
                return Task.FromResult<Texture2D>(null);
            }

            return m_thumbnailUtil.CreateThumbnailAsync(uo);
        }

        private IThumbnailUtil m_thumbnailUtil;
        private string[] m_excludedExtensions;

        public ThumbnailCreatorContext(IThumbnailUtil thumbnailUtil = null, Action<AssetThumbnailEventArgs> onSaveAsset = null, Action<AssetThumbnailEventArgs> onUpdateThumbnail = null, params string[] noThumbnailExtensions)
        {
            m_thumbnailUtil = thumbnailUtil;
            OnSaveAsset = onSaveAsset;
            OnUpdateThumbnail = onUpdateThumbnail;
            m_excludedExtensions = noThumbnailExtensions;
        }
    }
}
