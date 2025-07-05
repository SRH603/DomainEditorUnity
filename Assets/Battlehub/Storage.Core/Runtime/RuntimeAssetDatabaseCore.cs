#if UNITY_WEBGL
#define SINGLE_THREADED
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.Storage
{
    public abstract class RuntimeAssetDatabaseCore<TID, TFID, TMeta, TThumbnail, TExternalData> : IAssetDatabase<TID, TFID>, IAssetDatabaseInternalUtils<TID, TFID>, IAssetDatabaseSerializer<TID>
        where TID : IEquatable<TID>
        where TFID : IEquatable<TFID>
        where TMeta : IMeta<TID, TFID>, new()
        where TThumbnail : IThumbnail, new()
        where TExternalData : IExternalData<TID>, new()
    {
        private readonly RuntimeAssetDatabaseLock m_lock = new RuntimeAssetDatabaseLock();
        protected virtual Task<IDisposable> LockAsync()
        {
            return m_lock.LockAsync();
        }

        private IModuleDependencies<TID, TFID> m_deps;
        IModuleDependencies<TID, TFID> IAssetDatabaseInternalUtils<TID, TFID>.Deps
        {
            get { return m_deps; }
        }

        private readonly Dictionary<TID, object> m_assetIDToExternalAsset = new Dictionary<TID, object>();
        private readonly Dictionary<object, TID> m_externalAssetToAssetID = new Dictionary<object, TID>();
        private readonly Dictionary<string, AssetLoaderAdapter<TID>> m_loaderIDToExternalLoader = new Dictionary<string, AssetLoaderAdapter<TID>>();

        private readonly Dictionary<TFID, TFID> m_fileIDToParent;
        private readonly Dictionary<TFID, List<TID>> m_fileIDToChildren;
        private readonly Dictionary<TFID, TID> m_fileIDToID;
        private readonly Dictionary<TID, TMeta> m_idToMeta = new Dictionary<TID, TMeta>();
        private readonly Dictionary<TID, byte[]> m_idToThumbnail = new Dictionary<TID, byte[]>();
        private readonly HashSet<object> m_dontDestroyObjects = new HashSet<object>();

        private readonly TID[] m_emptyIDs = new TID[0];
        private readonly object[] m_emptyObjects = new object[0];

        private IIDMap<TID> m_idMap;
        private IAssetMap<TID> m_assetMap;

        IIDMap<TID> IAssetDatabaseInternalUtils<TID, TFID>.RootIDMap
        {
            get { return m_idMap; }
        }

        IAssetMap<TID> IAssetDatabaseInternalUtils<TID, TFID>.RootAssetMap
        {
            get { return m_assetMap; }
        }

        public bool IsProjectLoaded
        {
            get;
            private set;
        }

        public TID RootID
        {
            get;
            private set;
        }

        protected TFID RootFID
        {
            get;
            private set;
        }

        public RuntimeAssetDatabaseCore(IModuleDependencies<TID, TFID> deps)
        {
            m_deps = deps;
            m_fileIDToParent = new Dictionary<TFID, TFID>(GetFileIDComparer());
            m_fileIDToChildren = new Dictionary<TFID, List<TID>>(GetFileIDComparer());
            m_fileIDToID = new Dictionary<TFID, TID>(GetFileIDComparer());

            // warming up the serializer register serializable types
            _ = m_deps.Serializer;
        }

        protected virtual IEqualityComparer<TFID> GetFileIDComparer()
        {
            return EqualityComparer<TFID>.Default;
        }

        protected virtual IEqualityComparer<TID> GetIDComparer()
        {
            return EqualityComparer<TID>.Default;
        }

        protected bool Equals(TID id, TID other)
        {
            return GetIDComparer().Equals(id, other);
        }

        protected bool Equals(TFID id, TFID other)
        {
            return GetFileIDComparer().Equals(id, other);
        }

        //protected static readonly Guid s_folderGuid = new Guid("b56b591e-162c-46ed-82e2-80c49432788a");
        protected static readonly int s_FolderTypeID = -1;

        protected virtual TMeta CreateFolderMeta(TFID fileID, string name)
        {
            return new TMeta
            {
                ID = m_idMap.CreateID(),
                FileID = NormalizeFileID(fileID),
                TypeID = s_FolderTypeID,
                Name = name,
            };
        }

        protected virtual int GetBinaryTypeID()
        {
            if (!m_deps.TypeMap.TryGetID(typeof(BinaryData), out var typeID))
            {
                throw new KeyNotFoundException($"{nameof(BinaryData)} typeID not found");
            }

            return typeID;
        }

        protected virtual bool IsBinaryData(object obj)
        {
            return obj is BinaryData;
        }

        protected virtual bool IsBinaryData(int typeID)
        {
            return typeID == GetBinaryTypeID();
        }

        protected virtual bool IsBinaryData(in TMeta meta)
        {
            return IsBinaryData(meta.TypeID);
        }

        public async Task LoadProjectAsync(TFID projectID)
        {
            using (await LockAsync())
            {
                await LoadProjectAsyncImpl(projectID);
            }
        }

        protected virtual async Task LoadProjectAsyncImpl(TFID projectID)
        {
            var dataLayer = m_deps.DataLayer;
            bool exists = await dataLayer.ExistsAsync(projectID);
            if (!exists)
            {
                throw new ArgumentException($"project {projectID} does not exist", "fileID");
            }

            m_assetMap = m_deps.AcquireAssetMap();
            m_idMap = m_deps.AcquireIDMap();
            foreach (var kvp in m_assetIDToExternalAsset)
            {
                m_idMap.AddObject(kvp.Value, kvp.Key);
            }

            var treeItems = await dataLayer.GetTreeAsync(projectID, true);
            var projectItem = treeItems[0];
            var rootMeta = new TMeta
            {
                ID = m_idMap.CreateID(),
                FileID = projectID,
                TypeID = s_FolderTypeID,
                Name = projectItem.Name,
            };
            RootID = rootMeta.ID;
            RootFID = projectID;
            AddMeta(projectItem.ParentID, RootFID, in rootMeta);

            await ConvertTreeItemsToMetaAsync(dataLayer, treeItems, true);

            IsProjectLoaded = true;
        }

        private async Task ConvertTreeItemsToMetaAsync(IDataLayer<TFID> dataLayer, IList<TreeItem<TFID>> treeItems, bool skipRoot)
        {
            var serializer = m_deps.Serializer;

            for (int i = skipRoot ? 1 : 0; i < treeItems.Count; ++i)
            {
                var treeItem = treeItems[i];
                try
                {
                    var id = NormalizeFileID(treeItem.ID);
                    var parentID = NormalizeFileID(treeItem.ParentID);

                    var meta = treeItem.IsFolder ?
                        CreateFolderMeta(id, treeItem.Name) :
                        await DeserializeAsync(id, dataLayer, serializer);
                    SetMetaFileID(ref meta, id);
                    AddMeta(parentID, in meta);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError("Failed to load metadata " + treeItem.ID);
                }
            }
        }

        private static async Task<TMeta> DeserializeAsync(TFID metaFileID, IDataLayer<TFID> dataLayer, ISerializer serializer)
        {
            var stream = await dataLayer.OpenReadAsync(metaFileID);
            try
            {
                var pack = await serializer.Deserialize<TMeta>(stream);
                return pack.Data;
            }
            finally
            {
                await dataLayer.ReleaseAsync(stream);
            }
        }

        public async Task UnloadProjectAsync(bool destroy)
        {
            using (await LockAsync())
            {
                await UnloadProjectAsyncImpl(destroy);
            }
        }

        protected async Task UnloadProjectAsyncImpl(bool destroy)
        {
            IsProjectLoaded = false;

            await UnloadAllAssetsAsyncImpl(destroy: destroy);

            foreach (var externalAssetID in m_assetIDToExternalAsset.Keys)
            {
                m_idMap.Remove(externalAssetID);
            }

            m_fileIDToChildren.Clear();
            m_fileIDToParent.Clear();
            m_fileIDToID.Clear();
            m_idToMeta.Clear();
            m_idToThumbnail.Clear();
            m_dontDestroyObjects.Clear();

            m_deps.ReleaseIDMap(m_idMap);
            m_idMap = null;
            m_deps.ReleaseAssetMap(m_assetMap);
            m_assetMap = null;
            RootID = default;
        }

        public bool TryGetMeta(TID id, out IMeta<TID, TFID> meta)
        {
            if (TryGetMeta(id, out TMeta result))
            {
                meta = result;
                return true;
            }

            meta = default(TMeta);
            return false;
        }

        protected virtual bool TryGetMeta(TID id, out TMeta meta)
        {
            return m_idToMeta.TryGetValue(id, out meta);
        }

        public bool TryGetMeta(object obj, out IMeta<TID, TFID> meta)
        {
            if (TryGetMeta(obj, out TMeta result))
            {
                meta = result;
                return true;
            }

            meta = default(TMeta);
            return false;
        }

        protected virtual bool TryGetMeta(object obj, out TMeta meta)
        {
            if (m_assetMap != null && m_assetMap.TryGetRootAssetIDByAsset(obj, out TID id))
            {
                if (m_idToMeta.TryGetValue(id, out meta))
                {
                    return true;
                }
            }

            meta = default;
            return false;
        }

        public bool TryGetMeta(TFID fileID, out IMeta<TID, TFID> meta)
        {
            if (TryGetMeta(fileID, out TMeta result))
            {
                meta = result;
                return true;
            }

            meta = default(TMeta);
            return false;
        }

        protected virtual bool TryGetMeta(TFID fileID, out TMeta meta)
        {
            fileID = NormalizeFileID(fileID);

            if (m_fileIDToID.TryGetValue(fileID, out TID id))
            {
                if (TryGetMeta(id, out meta))
                {
                    return true;
                }
            }

            meta = default;
            return false;
        }

        public bool TryGetThumbnail(TID id, out byte[] thumbnail)
        {
            return m_idToThumbnail.TryGetValue(id, out thumbnail);
        }

        protected bool TryGetObject(TID id, out object obj)
        {
            return m_idMap.TryGetObject(id, out obj);
        }

        private void AddMeta(TFID parentID, in TMeta meta)
        {
            TFID metaFileID = GetMetaFileID(in meta);

            AddMeta(parentID, metaFileID, in meta);
        }

        private void AddMeta(TFID parentID, TFID metaFileID, in TMeta meta)
        {
            m_idToMeta.Add(meta.ID, meta);
            m_fileIDToID.Add(metaFileID, meta.ID);

            if (meta.TypeID == s_FolderTypeID)
            {
                if (!m_fileIDToChildren.ContainsKey(metaFileID))
                {
                    m_fileIDToChildren.Add(metaFileID, new List<TID>());
                }
            }

            if (!m_fileIDToChildren.TryGetValue(parentID, out List<TID> childIDs))
            {
                childIDs = new List<TID>();
                m_fileIDToChildren.Add(parentID, childIDs);
            }

            childIDs.Add(meta.ID);

            if (!m_fileIDToParent.ContainsKey(metaFileID))
            {
                m_fileIDToParent.Add(metaFileID, parentID);
            }
        }

        protected void SetMeta(TID id, in TMeta meta)
        {
            m_idToMeta[id] = meta;
        }

        private void RemoveMeta(TID id)
        {
            m_idToThumbnail.Remove(id);

            if (m_idToMeta.TryGetValue(id, out var meta))
            {
                var metaFileID = GetMetaFileID(in meta);
                m_idToMeta.Remove(id);
                m_fileIDToID.Remove(metaFileID);

                if (m_fileIDToParent.TryGetValue(metaFileID, out TFID parentID))
                {
                    if (m_fileIDToChildren.TryGetValue(parentID, out List<TID> children))
                    {
                        children.Remove(meta.ID);
                    }
                    m_fileIDToParent.Remove(metaFileID);
                }

                if (m_fileIDToChildren.TryGetValue(metaFileID, out var childIDs))
                {
                    m_fileIDToChildren.Remove(metaFileID);
                    foreach (var childID in childIDs)
                    {
                        RemoveMeta(childID);
                    }
                }
            }
        }

        public bool TryGetParent(TID id, out TID parentID)
        {
            parentID = default;

            if (!TryGetMeta(id, out TMeta meta))
            {
                return false;
            }

            if (!m_fileIDToParent.TryGetValue(GetMetaFileID(in meta), out TFID parentFID))
            {
                return false;
            }

            if (!m_fileIDToID.TryGetValue(parentFID, out parentID))
            {
                return false;
            }

            return true;
        }

        public bool TryGetChildren(TID id, out IReadOnlyList<TID> children)
        {
            children = m_emptyIDs;
            if (!TryGetMeta(id, out TMeta meta))
            {
                return false;
            }

            var metaFileID = GetMetaFileID(in meta);
            if (!m_fileIDToChildren.TryGetValue(metaFileID, out List<TID> childrenList))
            {
                return false;
            }

            children = childrenList;
            return true;
        }

        public bool IsFolder(TID id)
        {
            if (!TryGetMeta(id, out TMeta meta))
            {
                return false;
            }

            return IsFolder(meta);
        }

        protected virtual bool IsFolder(TMeta meta)
        {
            return meta.TypeID == s_FolderTypeID;
        }

        public bool TryGetAssetType(TID id, out Type type)
        {
            type = null;

            if (!TryGetMeta(id, out TMeta meta))
            {
                return false;
            }

            return m_deps.TypeMap.TryGetType(meta.TypeID, out type);
        }

        public async Task CreateFolderAsync(TFID folderID, string name, TID parentID)
        {
            using (await LockAsync())
            {
                await CreateFolderAsyncImpl(folderID, name, parentID);
            }
        }

        protected virtual async Task CreateFolderAsyncImpl(TFID folderID, string name, TID parentID)
        {
            if (!IsProjectLoaded)
            {
                throw new InvalidOperationException("Project is not loaded");
            }

            folderID = NormalizeFileID(folderID);

            var dataLayer = m_deps.DataLayer;
            bool exists = await dataLayer.ExistsAsync(folderID);
            if (exists)
            {
                throw new ArgumentException($"Folder with {folderID} already exists");
            }

            if (!TryGetMeta(parentID, out TMeta parentMeta))
            {
                throw new ArgumentException($"Parent with {parentID} not found");
            }

            await dataLayer.CreateFolderAsync(folderID);
            TMeta meta = CreateFolderMeta(folderID, name);
            AddMeta(GetMetaFileID(in parentMeta), in meta);
        }

        public async Task MoveFolderAsync(TFID folderID, TFID newFolderID)
        {
            using (await LockAsync())
            {
                await MoveFolderAsyncImpl(folderID, newFolderID);
            }
        }

        protected virtual async Task MoveFolderAsyncImpl(TFID folderID, TFID newFolderID)
        {
            if (!IsProjectLoaded)
            {
                throw new InvalidOperationException("Project is not loaded");
            }

            folderID = NormalizeFileID(folderID);
            newFolderID = NormalizeFileID(newFolderID);

            var dataLayer = m_deps.DataLayer;
            bool exists = await dataLayer.ExistsAsync(newFolderID);
            if (exists)
            {
                throw new ArgumentException($"Folder with {newFolderID} already exists");
            }

            await dataLayer.MoveFolderAsync(folderID, newFolderID);

            if (m_fileIDToParent.TryGetValue(folderID, out TFID parentID))
            {
                if (m_fileIDToChildren.TryGetValue(parentID, out List<TID> parentChildren))
                {
                    TID id = m_fileIDToID[folderID];

                    var children = this.GetChildren(id, recursive: true);
                    foreach (var childID in children)
                    {
                        if (TryGetMeta(childID, out TMeta meta))
                        {
                            TFID fileID = GetMetaFileID(meta);
                            m_fileIDToChildren.Remove(fileID);
                            m_fileIDToID.Remove(fileID);
                            m_fileIDToParent.Remove(fileID);
                            m_idToMeta.Remove(meta.ID);
                        }
                    }

                    m_idToMeta.Remove(id);
                    parentChildren.Remove(id);

                    m_fileIDToChildren.Remove(folderID);
                }
                m_fileIDToParent.Remove(folderID);
                m_fileIDToID.Remove(folderID);
            }

            var tree = await dataLayer.GetTreeAsync(newFolderID);
            await ConvertTreeItemsToMetaAsync(dataLayer, tree, false);
        }

        public async Task DeleteFolderAsync(TFID folderID)
        {
            using (await LockAsync())
            {
                await DeleteFolderAsyncImpl(folderID);
            }
        }

        protected virtual async Task DeleteFolderAsyncImpl(TFID folderID)
        {
            if (!IsProjectLoaded)
            {
                throw new InvalidOperationException("Project is not loaded");
            }

            folderID = NormalizeFileID(folderID);

            if (m_fileIDToID.TryGetValue(folderID, out TID id))
            {
                var assets = this.GetAssets(id, recursive: true);

                for (int i = 0; i < assets.Count; ++i)
                {
                    await DeleteAssetAsyncImpl(assets[i]);
                }

                RemoveMeta(id);
            }

            var dataLayer = m_deps.DataLayer;
            await dataLayer.DeleteFolderAsync(folderID);
        }

        private void InstantiateLinkedObjects(TMeta meta, IEnumerable<(TID, TID)> existingLinks, IAssetMap<TID> assetMap, IIDMap<TID> idMap, object parent)
        {
            foreach (var (instanceID, assetID) in existingLinks)
            {
                InstantiateLinkedObject(meta, instanceID, assetID, assetMap, idMap, parent);
            }
        }

        private void InstantiateLinkedObject(TMeta meta, TID rootInstanceID, TID linkID, IAssetMap<TID> assetMap, IIDMap<TID> idMap, object parent)
        {
            object linkedAsset = this.GetAsset(linkID);
            if (linkedAsset == null)
            {
                if (TryGetMeta(linkID, out TMeta linkMeta))
                {
                    Debug.LogWarning($"Linked Asset {linkID} not found. {linkMeta.Name}");
                }
                else
                {
                    Debug.LogWarning($"Linked Asset {linkID} not found");
                }

                return;
            }

            var linkMap = meta.GetLinkMap(rootInstanceID);
            object linkedAssetInstance = InstantiateObject(linkedAsset, parent);
            if (!assetMap.TryGetIDMapByRootAsset(linkedAsset, out var linkedAssetIDMap))
            {
                throw new KeyNotFoundException($"IDMap for asset not found {linkedAsset}");
            }

            assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(linkedAsset, out var assetsMarkedAsDestroyed);

            if (linkMap.TryGetValue(linkID, out var assetID))
            {
                idMap.AddObject(linkedAssetInstance, assetID);
            }

            using var linkedAssetEnumeratorRef = m_deps.AcquireEnumeratorRef(linkedAsset);
            using var linkedInstanceEnumeratorRef = m_deps.AcquireEnumeratorRef(linkedAssetInstance);

            var linkedAssetEnumerator = linkedAssetEnumeratorRef.Get();
            var linkedInstanceEnumerator = linkedInstanceEnumeratorRef.Get();

            var children = new List<object>();
            var typeMap = m_deps.TypeMap;
            while (MoveNext(linkedAssetEnumerator, typeMap))
            {
                MoveNext(linkedInstanceEnumerator, typeMap);

                var currentInstance = linkedInstanceEnumerator.Current;
                var currentAsset = linkedAssetEnumerator.Current;

                if (currentAsset == null || currentInstance == null || currentInstance == currentAsset)
                {
                    continue;
                }

                if (assetsMarkedAsDestroyed != null && assetsMarkedAsDestroyed.Contains(currentAsset))
                {
                    continue;
                }

                object rootReprensentation = GetObjectRootRepresentation(currentAsset);
                TID currentAssetID;
                if (rootReprensentation != null)
                {
                    if (linkedAssetIDMap.TryGetID(rootReprensentation, out currentAssetID))
                    {
                        if (linkMap.TryGetValue(currentAssetID, out TID instanceID))
                        {
                            idMap.AddObject(GetObjectRootRepresentation(currentInstance), instanceID);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"ID not found for asset {rootReprensentation}");
                    }
                }

                if (linkedAssetIDMap.TryGetID(currentAsset, out currentAssetID))
                {
                    if (linkMap.TryGetValue(currentAssetID, out TID instanceID))
                    {
                        idMap.AddObject(currentInstance, instanceID);
                    }
                }
            }

            var linkedAssetToInstance = CreateObjectMap(linkedAsset, linkedAssetInstance).Map;
            TrimInstance(linkedAsset, linkedAssetToInstance);
            assetMap.AddRootInstance(linkedAssetInstance, linkedAsset, linkedAssetToInstance);
        }

        private Task BeginDeserialize(TMeta meta, ISurrogatesSerializer<TID> serializer)
        {
            return TaskUtils.Run(async () =>
            {
                IDataLayer<TFID> dataLayer = m_deps.DataLayer;
                Stream stream = await dataLayer.OpenReadAsync(GetDataFileID(in meta));
                try
                {
                    await serializer.DeserializeFromStream(stream);
                }
                finally
                {
                    await dataLayer.ReleaseAsync(stream);
                }
            });
        }

        private Task<TExternalData> DeserializeExternalDataAsync(TFID fileID)
        {
            var dataLayer = m_deps.DataLayer;
            var serializer = m_deps.Serializer;

            return TaskUtils.Run(async () =>
            {
                var stream = await dataLayer.OpenReadAsync(fileID);
                try
                {
                    var pack = await serializer.Deserialize<TExternalData>(stream);
                    var result = pack.Data;
                    if (result.ExternalIDs == null)
                    {
                        result.ExternalIDs = new Dictionary<string, TID>();
                    }
                    return result;
                }
                finally
                {
                    await dataLayer.ReleaseAsync(stream);
                }
            });
        }

        private Task<byte[]> LoadBinaryDataAsync(TFID fileID)
        {
            var dataLayer = m_deps.DataLayer;
            var serializer = m_deps.Serializer;

            return TaskUtils.Run(async () =>
            {
                var stream = await dataLayer.OpenReadAsync(fileID);
                try
                {
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
                finally
                {
                    await dataLayer.ReleaseAsync(stream);
                }
            });
        }

        private const string k_rootPathFormat = "root#{0}";

        private bool TryCreateLoadQueue(TID assetID, out Queue<TID> loadQueue)
        {
            loadQueue = null;
            if (m_assetIDToExternalAsset.ContainsKey(assetID))
            {
                //Debug.Log($"The external asset {assetID} already loaded");
                return false;
            }

            if (!TryGetMeta(assetID, out TMeta meta))
            {
                throw new ArgumentException($"Metadata {assetID} not found");
            }


            if (!meta.HasLinks() && !meta.HasOutboundDependencies())
            {
                return false;
            }

            loadQueue = new Queue<TID>(); ;
            PopulateLoadQueue(in meta, new HashSet<TID>(), loadQueue);
            return true;
        }

        private void PopulateLoadQueue(in TMeta meta, HashSet<TID> visited, Queue<TID> loadQueue)
        {
            if (!visited.Add(meta.ID))
            {
                //Debug.LogWarning($"Circular reference detected {meta.ID}, {meta.Name}");
                return;
            }

            if (meta.OutboundDependencies != null)
            {
                foreach (TID depID in meta.OutboundDependencies)
                {
                    if (TryGetMeta(depID, out TMeta depMeta))
                    {
                        PopulateLoadQueue(in depMeta, visited, loadQueue);
                    }
                    else
                    {
                        //Debug.LogWarning($"{meta.FileID}: Metadata for asset {depID} not found");
                    }
                }
            }

            if (meta.Links != null)
            {
                foreach (var link in meta.Links)
                {
                    var linkID = link.Value;

                    if (meta.OutboundDependencies != null && meta.OutboundDependencies.Contains(linkID))
                    {
                        continue;
                    }

                    if (TryGetMeta(linkID, out TMeta linkMeta))
                    {
                        PopulateLoadQueue(in linkMeta, visited, loadQueue);
                    }
                    else
                    {
                        Debug.LogWarning($"Metadata for asset {linkID} not found");
                    }
                }
            }

            loadQueue.Enqueue(meta.ID);
        }

        public async Task LoadAssetAsync(TID assetID)
        {
            using (await LockAsync())
            {
                await LoadAssetAsyncImpl(assetID);
            }
        }

        protected async Task LoadAssetAsyncImpl(TID assetID)
        {
            if (TryGetObject(assetID, out var _) || IsFolder(assetID))
            {
                return;
            }

            if (TryCreateLoadQueue(assetID, out var loadQueue))
            {
                while (loadQueue.Count > 0)
                {
                    await LoadNextAssetAsync(loadQueue.Dequeue());
                }
            }
            else
            {
                await LoadNextAssetAsync(assetID);
            }
        }

        protected async Task LoadNextAssetAsync(TID assetID)
        {
            if (TryGetObject(assetID, out var _) || IsFolder(assetID))
            {
                return;
            }

            if (!TryGetMeta(assetID, out TMeta meta))
            {
                throw new ArgumentException($"Metadata {assetID} not found");
            }

            var workloadCtrl = m_deps.WorkloadController;

            if (IsBinaryData(in meta))
            {
                using var idMapRef = m_deps.AcquireIDMapRef(m_idMap);
                var idMap = idMapRef.Get();
                var bytes = await LoadBinaryDataAsync(GetDataFileID(in meta));
                var binaryData = ScriptableObject.CreateInstance<BinaryData>();
                binaryData.Bytes = bytes;
                SetObjectName(binaryData, meta.Name);

                idMap.AddObject(binaryData, assetID);

                idMap.Commit();
                idMapRef.Detach();

                m_assetMap.AddRootAsset(binaryData, idMap, null);
            }
            else if (IsExternalAssetRoot(in meta))
            {
                using var idMapRef = m_deps.AcquireIDMapRef(m_idMap);

                var tempRoot = m_deps.AssetsRoot;
                var idMap = idMapRef.Get();
                var typeMap = m_deps.TypeMap;

                object externalAsset = null;
                if (!m_loaderIDToExternalLoader.TryGetValue(meta.LoaderID, out var externalLoader))
                {
                    Debug.LogWarning($"External loader {meta.LoaderID} not found");
                }
                else
                {
                    var externalData = await DeserializeExternalDataAsync(GetDataFileID(in meta));

                    bool shouldInstantiateExternalAsset = externalLoader.IsLoaded(externalData.ExternalKey);
                    externalAsset = shouldInstantiateExternalAsset ?
                        externalLoader.Instantiate(externalData.ExternalKey, tempRoot) :
                        await externalLoader.LoadAsync(externalData.ExternalKey, tempRoot, null);

                    if (externalAsset == null)
                    {
                        Debug.LogWarning($"External asset {assetID} not found");
                    }
                    else
                    {
                        if (m_idMap.TryGetID(externalAsset, out var externalAssetID))
                        {
                            if (!Equals(assetID, externalAssetID))
                            {
                                // external asset already present in id map but has different id.
                                externalAsset = externalLoader.Instantiate(externalData.ExternalKey, tempRoot);
                            }
                        }

                        using var enumeratorRef = m_deps.AcquireEnumeratorRef(externalAsset);
                        var enumerator = enumeratorRef.Get();
                        var externalIDs = externalData.ExternalIDs;
                        int externalIDsCount = externalIDs.Count;
                        while (enumerator.MoveNext())
                        {
                            await workloadCtrl.TryPostponeTask();

                            object current = enumerator.Current;
                            if (current == null)
                            {
                                continue;
                            }

                            string currentPath = enumerator.GetCurrentPath(typeMap);
                            if (!externalIDs.TryGetValue(currentPath, out TID currentID))
                            {
                                currentID = idMap.CreateID();
                                externalIDs.Add(currentPath, currentID);
                            }

                            idMap.AddObject(current, currentID);

                            object rootRepresentation = GetObjectRootRepresentation(current);
                            if (rootRepresentation != null)
                            {
                                if (!externalIDs.TryGetValue(string.Format(k_rootPathFormat, currentPath), out TID rootRepresentationID))
                                {
                                    rootRepresentationID = idMap.CreateID();
                                    externalIDs.Add(string.Format(k_rootPathFormat, currentPath), rootRepresentationID);
                                }

                                idMap.AddObject(rootRepresentation, rootRepresentationID);
                            }
                        }

                        idMap.Commit();
                        idMapRef.Detach();

                        RegisterExternalAsset(meta.ID, externalAsset, false);
                        m_assetMap.AddRootAsset(externalAsset, idMap, null);
                        SetObjectParent(externalAsset, m_deps.AssetsRoot);
                        SetObjectName(externalAsset, meta.Name);

                        if (externalIDs.Count != externalIDsCount)
                        {
                            await SerializeExternalDataAsync(GetDataFileID(in meta), externalData);
                        }
                    }
                }
            }
            else
            {
                using var idMapRef = m_deps.AcquireIDMapRef(m_idMap);
                using var assetMapRef = m_deps.AcquireAssetMapRef(m_assetMap);

                var idMap = idMapRef.Get();
                var assetMap = assetMapRef.Get();
                var tempRoot = m_deps.AssetsRoot;

                if (meta.HasLinks())
                {
                    var existingLinks = new List<(TID InstanceID, TID LinkID)>(meta.Links.Count);
                    foreach (var link in meta.Links)
                    {
                        if (TryGetMeta(link.Value, out TMeta _))
                        {
                            existingLinks.Add((link.Key, link.Value));
                        }
                        else
                        {
                            Debug.LogWarning($"Metadata {assetID} not found");
                        }
                    }

                    InstantiateLinkedObjects(meta, existingLinks, assetMap, idMap, tempRoot);
                }

                using var serializerRef = m_deps.AcquireSerializerRef();
                using var contextRef = m_deps.AcquireContextRef();

                var serializer = serializerRef.Get();
                var context = contextRef.Get();
                context.IDMap = idMap;
                context.ShaderUtil = m_deps.ShaderUtil;
                context.TypeMap = m_deps.TypeMap;
                context.TempRoot = tempRoot;

                var deserializeTask = BeginDeserialize(meta, serializer);
                await DeserializeObjectAsync(serializer, context, assetMap, deserializeTask);

                if (!idMap.TryGetObject<object>(meta.ID, out var asset))
                {
                    Debug.LogWarning($"Asset {assetID} is null");
                    return;
                }

                if (asset != null)
                {
                    SetObjectParent(asset, m_deps.AssetsRoot);
                    SetObjectName(asset, meta.Name);

                    idMap.Commit();
                    idMapRef.Detach();

                    var markedAsDestroyed = GetAssetsMarkedAsDestroyed(meta, idMap);

                    /* DestroyComponentAndApplyTest2*/
                    if (markedAsDestroyed != null && meta.HasLinks())
                    {
                        foreach (var link in meta.Links)
                        {
                            if (Equals(meta.ID, link.Key))
                            {
                                // prefab variant link
                                continue;
                            }

                            var linkedInstance = idMap.GetObject<object>(link.Key);
                            if (linkedInstance == null)
                            {
                                continue;
                            }
                            if (assetMap.TryGetAssetToInstanceMapByRootInstance(linkedInstance, out var assetToInstanceMap))
                            {
                                bool waitForEndOfFrame = false;
                                foreach (var kvp in assetToInstanceMap)
                                {
                                    if (markedAsDestroyed.Contains(kvp.Value))
                                    {
                                        DestroyObject(kvp.Value);
                                        waitForEndOfFrame = true;
                                    }
                                }
                                if (waitForEndOfFrame)
                                {
                                    await Task.Yield();
                                }
                            }
                        }
                    }

                    assetMap.AddRootAsset(asset, idMap, markedAsDestroyed);
                    assetMap.Commit();
                }
            }
        }

        private HashSet<object> GetAssetsMarkedAsDestroyed(object rootAsset, IIDMap<TID> idMap)
        {
            if (TryGetMeta(rootAsset, out TMeta meta))
            {
                return GetAssetsMarkedAsDestroyed(meta, idMap);
            }
            return null;
        }

        private static HashSet<object> GetAssetsMarkedAsDestroyed(TMeta meta, IIDMap<TID> idMap)
        {
            HashSet<object> markedAsDestroyed = null;
            if (meta.MarkedAsDestroyed != null)
            {
                markedAsDestroyed = new HashSet<object>();
                foreach (var id in meta.MarkedAsDestroyed)
                {
                    if (idMap.TryGetObject<object>(id, out var obj))
                    {
                        markedAsDestroyed.Add(obj);
                    }
                    //else
                    //{
                    //    Debug.Log($"object with id {id} not found");
                    //}
                }
            }

            return markedAsDestroyed;
        }

        private async Task DeserializeObjectAsync(ISurrogatesSerializer<TID> serializer, IIDMap<TID> idMap, IAssetMap<TID> assetMap = null, Task deserializeTask = null)
        {
            var contextRef = m_deps.AcquireContextRef();
            var context = contextRef.Get();
            context.IDMap = idMap;
            context.ShaderUtil = m_deps.ShaderUtil;
            context.TypeMap = m_deps.TypeMap;
            context.TempRoot = m_deps.AssetsRoot;

            await DeserializeObjectAsync(serializer, context, assetMap, deserializeTask);
        }

        private async Task DeserializeObjectAsync(ISurrogatesSerializer<TID> serializer, ISerializationContext<TID> context, IAssetMap<TID> assetMap, Task deserializeTask = null)
        {
            IWorkloadController workloadCtrl = m_deps.WorkloadController;
            while (true)
            {
                object obj = null;
                try
                {
                    obj = await serializer.Dequeue(context);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                if (obj is int)
                {
                    if (-1 == (int)obj)
                    {
                        await workloadCtrl.TryPostponeTask();
                    }
                    else if (-2 == (int)obj)
                    {
                        break;
                    }
                }
                else if (obj != null && assetMap != null)
                {
                    if (assetMap.IsInstance(obj))
                    {
                        if (IsDirtyByDefault(obj, assetMap))
                        {
                            if (!IsEqualToAsset(obj, assetMap))
                            {
                                assetMap.SetDirty(obj);
                            }
                        }
                        else
                        {
                            assetMap.SetDirty(obj);
                        }
                    }
                }

                if (deserializeTask != null && deserializeTask.IsFaulted)
                {
                    throw new Exception("LoadAssetAsync failed", deserializeTask.Exception);
                }
            }
        }

        private static async Task<TThumbnail> DeserializeThumbnailAsync(TFID fileID, IDataLayer<TFID> dataLayer, ISerializer serializer)
        {
            var stream = await dataLayer.OpenReadAsync(fileID);
            try
            {
                if (stream.Length == 0)
                {
                    return default;
                }

                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return new TThumbnail { Data = ms.ToArray() };
            }
            finally
            {
                await dataLayer.ReleaseAsync(stream);
            }
        }

        public async Task LoadThumbnailAsync(TID assetID)
        {
            using (await LockAsync())
            {
                await LoadThumbnailAsyncImpl(assetID);
            }
        }

        protected async Task LoadThumbnailAsyncImpl(TID assetID)
        {
            if (!TryGetMeta(assetID, out TMeta meta))
            {
                if (m_assetIDToExternalAsset.ContainsKey(assetID))
                {
                    Debug.LogWarning($"The external asset {assetID} cannot be loaded");
                    return;
                }

                throw new ArgumentException($"Metadata {assetID} not found");
            }

            if (TryGetThumbnail(assetID, out var _))
            {
                return;
            }

            var thumbnailFileID = GetThumbnailFileID(in meta);
            if (Equals(default, thumbnailFileID))
            {
                return;
            }

            var dataLayer = m_deps.DataLayer;
            bool exists = await dataLayer.ExistsAsync(thumbnailFileID);
            if (exists)
            {
                var thumbnail = await DeserializeThumbnailAsync(thumbnailFileID, dataLayer, m_deps.Serializer);
                m_idToThumbnail.Add(assetID, thumbnail != null ? thumbnail.Data : null);
            }
            else
            {
                m_idToThumbnail.Add(assetID, null);
            }
        }

        public async Task SaveThumbnailAsync(TID assetID, byte[] thumbnail)
        {
            using (await LockAsync())
            {
                await SaveThumbnailAsyncImpl(assetID, thumbnail);
            }
        }

        protected async Task SaveThumbnailAsyncImpl(TID assetID, byte[] thumbnail)
        {
            if (!TryGetMeta(assetID, out TMeta meta))
            {
                throw new ArgumentException($"Metadata {assetID} not found");
            }

            var thumbnailFileID = GetThumbnailFileID(in meta);
            if (Equals(default, thumbnailFileID))
            {
                return;
            }

            var dataLayer = m_deps.DataLayer;
            await TaskUtils.Run(() => SerializeThumbnailAsync(thumbnailFileID, dataLayer, new TThumbnail { Data = thumbnail }));
            m_idToThumbnail[assetID] = thumbnail;
        }

        public bool CanCreateAsset(object obj)
        {
            var typeMap = m_deps.TypeMap;
            if (!typeMap.TryGetID(obj.GetType(), out _))
            {
                return false;
            }

            bool isExternalAsset = TryGetExternalAssetID(obj, out var _);
            bool shouldInstantiateObject = HasInstantiableAssetType(obj) && !isExternalAsset;
            bool isRootAsset = m_assetMap.IsRootAsset(obj);
            bool isChildAsset = m_assetMap.IsAsset(obj) && !isRootAsset;
            bool isNotAsset = !isRootAsset && !isChildAsset;
            bool isRootInstance = m_assetMap.IsRootInstance(obj);
            bool isChildInstance = m_assetMap.IsInstance(obj) && !isRootInstance;

            if (shouldInstantiateObject)
            {
                if (isRootInstance)
                {
                    if (isRootAsset || isChildAsset)
                    {
                        if (!m_assetMap.TryGetAssetByInstance(obj, out var asset))
                        {
                            return false;
                        }

                        if (!m_assetMap.TryGetRootAssetByAsset(obj, out object _))
                        {
                            return false;
                        }

                        if (!AreEqual(asset, obj, CreateObjectMap(asset, obj).Map))
                        {
                            return false;
                        }
                    }
                }
                else if (isChildInstance)
                {
                    if (isNotAsset)
                    {
                        if (!m_assetMap.TryGetAssetByInstance(obj, out var asset))
                        {
                            return false;
                        }

                        if (m_assetMap.IsRootAsset(asset))
                        {
                            return false;
                        }

                        if (m_assetMap.IsInstance(asset))
                        {
                            if (!m_assetMap.IsRootInstance(asset))
                            {
                                return false;
                            }
                        }

                        if (!AreEqual(asset, obj, CreateObjectMap(asset, obj).Map))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (isRootAsset)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task CreateAssetAsync(object obj, TFID metaFileID, byte[] thumbnail, TFID thumbnailID, TFID dataFileID, TID parentID)
        {
            using (await LockAsync())
            {
                await CreateAssetAsyncImpl(obj, metaFileID, thumbnail, thumbnailID, dataFileID, parentID, null, null);
            }
        }

        protected virtual async Task CreateAssetAsyncImpl(object obj, TFID metaFileID, byte[] thumbnail, TFID thumbnailID, TFID dataFileID, TID parentID, string externalLoaderID, string externalAssetKey)
        {
            if (TryGetMeta(metaFileID, out TMeta _))
            {
                throw new ArgumentException($"Asset with File ID {metaFileID} already exist");
            }

            if (Equals(dataFileID, default))
            {
                throw new ArgumentException("Data file ID is not specified", "dataFileID");
            }

            if (!TryGetMeta(parentID, out TMeta parentMeta))
            {
                throw new ArgumentException("Parent is not found", "parentID");
            }

            var parentMetaFileID = GetMetaFileID(in parentMeta);
            if (!m_fileIDToChildren.TryGetValue(parentMetaFileID, out _))
            {
                throw new ArgumentException($"Unknown File ID {parentMetaFileID}", "parentID");
            }

            var typeMap = m_deps.TypeMap;
            if (!typeMap.TryGetID(obj.GetType(), out int typeID))
            {
                throw new ArgumentException($"Unable to create asset of type {obj.GetType().FullName}. Create Surrogate for {obj.GetType().FullName} and click Build All in Tools > Runtime Asset Database menu", "obj");
            }

            bool isExternalAsset = TryGetExternalAssetID(obj, out var externalAssetID);
            bool shouldInstantiateObject = HasInstantiableAssetType(obj) && !isExternalAsset;
            bool isRootAsset = m_assetMap.IsRootAsset(obj);
            bool isChildAsset = m_assetMap.IsAsset(obj) && !isRootAsset;
            bool isNotAsset = !isRootAsset && !isChildAsset;
            bool isRootInstance = m_assetMap.IsRootInstance(obj);
            bool isChildInstance = m_assetMap.IsInstance(obj) && !isRootInstance;

            var assetsRoot = m_deps.AssetsRoot;
            var workloadController = m_deps.WorkloadController;
            using var idMapRef = m_deps.AcquireIDMapRef(m_idMap);
            using var assetMapRef = m_deps.AcquireAssetMapRef(m_assetMap);
            var assetMap = assetMapRef.Get();

            var removeFromAssetMap = new List<object>();
            object newAsset = null;
            if (shouldInstantiateObject)
            {
                if (isRootInstance)
                {
                    if (isRootAsset || isChildAsset)
                    {
                        if (!assetMap.TryGetAssetByInstance(obj, out var asset))
                        {
                            throw new KeyNotFoundException($"Unable to find asset by instance {obj}");
                        }

                        if (!assetMap.TryGetRootAssetByAsset(obj, out object rootAsset))
                        {
                            throw new KeyNotFoundException($"Unable to find root asset {obj}");
                        }

                        if (!assetMap.TryGetAssetToInstanceMapByRootInstance(obj, out var assetToObjMap))
                        {
                            throw new KeyNotFoundException($"Unable to find asset to obj map {obj}");
                        }

                        newAsset = InstantiateObject(obj, assetsRoot);
                        MapInstances(obj, newAsset, detachSourceInstances: false, assetMap);

                        var objToNewAssetMap = CreateObjectMap(obj, newAsset);
                        objToNewAssetMap.ThrowIfCorrupted();

                        var assetToNewAssetMap = Remap(assetToObjMap, objToNewAssetMap.Map);
                        assetMap.AddRootInstance(newAsset, asset, assetToNewAssetMap);

                        foreach (var kvp in objToNewAssetMap.Map)
                        {
                            if (assetMap.IsDirty(kvp.Key))
                            {
                                assetMap.SetDirty(kvp.Value);
                            }
                        }
                    }
                    else
                    {
                        newAsset = ConvertToAssetVariant(obj, assetsRoot, assetMap);
                    }
                }
                else if (isChildInstance)
                {
                    if (isNotAsset)
                    {
                        if (!assetMap.TryGetAssetByInstance(obj, out var asset))
                        {
                            throw new KeyNotFoundException($"Unable to find asset by instance {obj}");
                        }

                        if (assetMap.IsRootAsset(asset))
                        {
                            throw new InvalidOperationException("Unable to create asset");
                        }

                        if (assetMap.IsInstance(asset))
                        {
                            if (!assetMap.IsRootInstance(asset))
                            {
                                throw new InvalidOperationException("Unable to create asset");
                            }
                        }

                        if (!AreEqual(asset, obj, CreateObjectMap(asset, obj).Map))
                        {
                            throw new InvalidOperationException($"Apply instance changes to asset first. Use {nameof(ApplyChangesAsync)}");
                        }

                        if (assetMap.IsRootInstance(asset))
                        {
                            newAsset = ConvertToAssetVariant(asset, assetsRoot, assetMap);
                        }
                        else
                        {
                            // Looks like there's nothing to do here with dirty and deleted flags
                            newAsset = InstantiateObject(asset, assetsRoot);

                            var objectMap = CreateObjectMap(newAsset, asset);
                            objectMap.ThrowIfCorrupted();

                            assetMap.AddRootInstance(asset, newAsset, objectMap.Map);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("Cannot create asset from child instance");
                    }
                }
                else
                {
                    if (isRootAsset || isChildAsset)
                    {
                        newAsset = InstantiateObject(obj, assetsRoot);
                        MapInstances(obj, newAsset, detachSourceInstances: false, assetMap);
                    }
                    else
                    {
                        newAsset = InstantiateObject(obj, assetsRoot);
                        MapInstances(obj, newAsset, detachSourceInstances: true, assetMap);

                        var newAssetToInstanceMap = CreateObjectMap(newAsset, obj);
                        newAssetToInstanceMap.ThrowIfCorrupted();

                        assetMap.AddRootInstance(obj, newAsset, newAssetToInstanceMap.Map);

                        // propagate dirty flags
                        foreach (var kvp in newAssetToInstanceMap.Map)
                        {
                            if (assetMap.IsDirty(kvp.Value) && assetMap.IsInstance(kvp.Key))
                            {
                                assetMap.SetDirty(kvp.Key);
                            }
                        }
                    }
                }
            }
            else
            {
                if (isRootAsset)
                {
                    throw new ArgumentException($"Asset {obj.GetType()} already created. Use {nameof(ApplyChangesAsync)} & {nameof(SaveAssetAsync)} instead");
                }

                newAsset = obj;
            }

            using var serializerRef = m_deps.AcquireSerializerRef();

            TID rootAssetID;
            TMeta meta;
            HashSet<TMeta> updateMetaHs = null;
            Task serializeDataTask = Task.CompletedTask;
            var idMap = idMapRef.Get();
            bool isBinaryData = IsBinaryData(obj);
            if (isBinaryData)
            {
                rootAssetID = idMap.CreateID();
                meta = CreateMeta(rootAssetID, typeID, dataFileID, thumbnailID, metaFileID, newAsset);

                idMap.AddObject(newAsset, rootAssetID);
                await SaveBinaryDataAsync(dataFileID, newAsset);
                idMap.Commit();
            }
            else if (isExternalAsset)
            {
                rootAssetID = externalAssetID;
                meta = CreateMeta(rootAssetID, typeID, dataFileID, thumbnailID, metaFileID, newAsset);

                idMap.AddObject(newAsset, rootAssetID);

                using var rootAssetEnumeratorRef = m_deps.AcquireEnumeratorRef(newAsset);
                var rootAssetEnumerator = rootAssetEnumeratorRef.Get();

                var externalIDs = new Dictionary<string, TID>();
                m_loaderIDToExternalLoader.TryGetValue(externalLoaderID, out var assetLoader);

                static TID AddExternalAssetToIDMap(object asset, IIDMap<TID> idMap, IExternalIDMap<TID> externalIDMap)
                {
                    bool gotAssetID = idMap.TryGetID(asset, out var assetID);
                    if (gotAssetID)
                    {
                        //Debug.LogWarning($"External asset already mapped {asset} {assetID}");
                        return assetID;
                    }
                    else
                    {
                        if (externalIDMap == null || !externalIDMap.TryGetID(asset, out assetID))
                        {
                            assetID = idMap.CreateID();
                        }
                    }

                    idMap.AddObject(asset, assetID);
                    return assetID;
                }

                while (MoveNext(rootAssetEnumerator, typeMap))
                {
                    await workloadController.TryPostponeTask();

                    object asset = rootAssetEnumerator.Current;
                    if (asset == null)
                    {
                        continue;
                    }

                    var assetType = asset.GetType();
                    if (!typeMap.TryGetID(assetType, out _))
                    {
                        continue;
                    }

                    string currentPath = rootAssetEnumerator.GetCurrentPath(typeMap);
                    externalIDs.Add(currentPath, AddExternalAssetToIDMap(asset, idMap, assetLoader));

                    object rootRepresentation = GetObjectRootRepresentation(asset);
                    if (rootRepresentation != null)
                    {
                        externalIDs.Add(string.Format(k_rootPathFormat, currentPath), AddExternalAssetToIDMap(rootRepresentation, idMap, assetLoader));
                    }
                }

                idMap.Commit();

                try
                {
                    var externalData = new TExternalData
                    {
                        ExternalKey = externalAssetKey,
                        ExternalIDs = externalIDs,
                    };

                    serializeDataTask = SerializeExternalDataAsync(dataFileID, externalData);
                }
                catch
                {
                    idMap.Rollback();
                    throw;
                }
            }
            else
            {
#if SINGLE_THREADED
                var serializationResult = await SerializeAssetAsync(default, newAsset, serializerRef.Get(), assetMap, idMap);
                serializeDataTask = SerializeDataAsync(dataFileID, serializerRef.Get());
#else
                serializeDataTask = SerializeDataAsync(dataFileID, serializerRef.Get());
                var serializationResult = await SerializeAssetAsync(default, newAsset, serializerRef.Get(), assetMap, idMap);
#endif
                bool gotAssetID = idMap.TryGetID(newAsset, out rootAssetID);
                if (!gotAssetID)
                {
                    throw new KeyNotFoundException("Root asset ID not found");
                }

                idMap.Commit();
                try
                {
                    meta = CreateMeta(rootAssetID, typeID, dataFileID, thumbnailID, metaFileID, newAsset);
                    meta.OutboundDependencies = serializationResult.OutboundDeps;
                    meta.Links = serializationResult.RootInstanceToRootAssetID;
                    foreach (var kvp in serializationResult.RootInstanceToAssetToInstanceID)
                    {
                        var rootInstanceID = kvp.Key;
                        var assetToInstanceID = kvp.Value;
                        meta.AddLinkMap(rootInstanceID, assetToInstanceID);
                    }
                    meta.MarkedAsDestroyed = serializationResult.MarkedAsDestroyed.Count > 0 ? serializationResult.MarkedAsDestroyed : null;
                    updateMetaHs = UpdateInboundDepsOfOutboundDeps(rootAssetID, serializationResult.OutboundDeps);
                }
                catch
                {
                    idMap.Rollback();
                    throw;
                }
            }

            try
            {
                await serializeDataTask;
                await SerializeDepsMetaAsync(updateMetaHs);

                meta.LoaderID = externalLoaderID;

                await SerializeMetaAndThumbnailAsync(metaFileID, meta, thumbnailID, thumbnail);

                var newAssetsMarkedAsDestroyed = GetAssetsMarkedAsDestroyed(meta, idMap);
                assetMap.AddRootAsset(newAsset, idMap, newAssetsMarkedAsDestroyed);
                assetMap.Commit();

                AddMeta(parentMetaFileID, in meta);
                SetObjectName(newAsset, meta.Name);

                m_idToThumbnail.Add(rootAssetID, thumbnail);
            }
            catch
            {
                idMap.Rollback();
                throw;
            }

            idMapRef.Detach();
        }

        private object ConvertToAssetVariant(object obj, object assetsRoot, IAssetMap<TID> assetMap)
        {
            object newAsset;
            if (!assetMap.TryGetAssetByInstance(obj, out var asset))
            {
                throw new KeyNotFoundException($"Unable to find asset by instance {obj}");
            }

            if (!assetMap.TryGetAssetToInstanceMapByRootInstance(obj, out var assetToInstanceMap))
            {
                throw new KeyNotFoundException($"Unable to find assetToInstanceMap by instance {obj}");
            }

            newAsset = InstantiateObject(obj, assetsRoot);
            MapInstances(obj, newAsset, detachSourceInstances: true, assetMap);

            var newAssetToInstanceMap = CreateObjectMap(newAsset, obj).Map;
            assetMap.AddRootInstance(obj, newAsset, newAssetToInstanceMap);

            var assetToNewAssetMap = Remap(assetToInstanceMap, newAssetToInstanceMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key));
            assetMap.AddRootInstance(newAsset, asset, assetToNewAssetMap);

            // propagate "dirty" flags
            foreach (var kvp in newAssetToInstanceMap)
            {
                if (assetMap.IsDirty(kvp.Value))
                {
                    assetMap.ClearDirty(kvp.Value, waitForCommit: true);
                    assetMap.SetDirty(kvp.Key);
                }
            }

            assetMap.Remove(obj, true);
            return newAsset;
        }

        private static HashSet<TID> GetObjectIDs(HashSet<object> newAssetsMarkedAsDestroyed, IIDMap<TID> idMap)
        {
            HashSet<TID> markedAsDestroyedIDs = null;
            if (newAssetsMarkedAsDestroyed != null && newAssetsMarkedAsDestroyed.Count > 0)
            {
                markedAsDestroyedIDs = new HashSet<TID>();
                foreach (object asset in newAssetsMarkedAsDestroyed)
                {
                    if (idMap.TryGetID(asset, out TID id))
                    {
                        markedAsDestroyedIDs.Add(id);
                    }
                    else
                    {
                        Debug.LogWarning($"asset with id {id} not found");
                    }
                }
            }

            return markedAsDestroyedIDs;
        }

        private void MapInstances(object sourceRoot, object targetRoot, bool detachSourceInstances, IAssetMap<TID> assetMap)
        {
            var sourceToTargetMap = CreateObjectMap(sourceRoot, targetRoot).Map;

            using var sourceEnumeratorRef = m_deps.AcquireEnumeratorRef(sourceRoot);
            using var targetEnumeratorRef = m_deps.AcquireEnumeratorRef(targetRoot);

            var sourceEnumerator = sourceEnumeratorRef.Get();
            var targetEnumerator = targetEnumeratorRef.Get();

            var typeMap = m_deps.TypeMap;

            while (MoveNext(sourceEnumerator, typeMap))
            {
                MoveNext(targetEnumerator, typeMap);

                object source = GetObjectRootRepresentation(sourceEnumerator.Current);
                if (source != null && assetMap.IsRootInstance(source))
                {
                    if (source == sourceRoot)
                    {
                        continue;
                    }

                    if (!assetMap.TryGetRootAssetByInstance(source, out var rootAsset))
                    {
                        throw new KeyNotFoundException($"Unable to find root asset. Key {source}");
                    }

                    if (!assetMap.TryGetAssetToInstanceMapByRootInstance(source, out var assetToInstance))
                    {
                        throw new KeyNotFoundException($"Unable to assetToInstanceMap. Key {source}");
                    }

                    if (detachSourceInstances)
                    {
                        assetMap.Remove(source, true);
                    }

                    var assetToTargetMap = Remap(assetToInstance, sourceToTargetMap);
                    object target = GetObjectRootRepresentation(targetEnumerator.Current);
                    assetMap.AddRootInstance(target, rootAsset, assetToTargetMap);

                    sourceEnumerator.Trim();
                    targetEnumerator.Trim();
                }
            }
        }

        private void MapInstances(object sourceRoot, IReadOnlyDictionary<object, object> sourceToTargetMap, bool detachSourceInstances, IAssetMap<TID> assetMap)
        {
            foreach (var kvp in sourceToTargetMap)
            {
                object source = GetObjectRootRepresentation(kvp.Key);
                if (source != null && assetMap.IsRootInstance(source))
                {
                    if (source == sourceRoot)
                    {
                        continue;
                    }

                    if (!assetMap.TryGetRootAssetByInstance(source, out var rootAsset))
                    {
                        throw new KeyNotFoundException($"Unable to find root asset. Key {source}");
                    }

                    if (!assetMap.TryGetAssetToInstanceMapByRootInstance(source, out var assetToInstance))
                    {
                        throw new KeyNotFoundException($"Unable to assetToInstanceMap. Key {source}");
                    }

                    if (detachSourceInstances)
                    {
                        assetMap.Remove(source, waitForCommit: true);
                    }

                    var assetToTargetMap = Remap(assetToInstance, sourceToTargetMap);
                    object target = GetObjectRootRepresentation(kvp.Value);
                    assetMap.AddRootInstance(target, rootAsset, assetToTargetMap);
                }
            }
        }

        private class NullObject
        {
            public override string ToString()
            {
                return "RuntimeAssetDatabaseCore+NullObject";
            }
        }

        private static Dictionary<object, object> Remap(IReadOnlyDictionary<object, object> abMap, IReadOnlyDictionary<object, object> bcMap)
        {
            var acMap = new Dictionary<object, object>();
            foreach (var kvp in abMap)
            {
                var a = kvp.Key;
                var b = kvp.Value;
                if (b != null && bcMap.TryGetValue(b, out var c))
                {
                    acMap.Add(a, c);
                }
                else
                {
                    acMap.Add(a, new NullObject());
                }
            }
            return acMap;
        }

        public async Task SaveAssetAsync(TID assetID, byte[] thumbnail)
        {
            using (await LockAsync())
            {
                bool readOnlyIDMap = false;
                if (TryGetObject(assetID, out var asset))
                {
                    readOnlyIDMap = HasInstantiableAssetType(asset);
                }
                await SaveAssetAsyncImpl(assetID, thumbnail, readOnlyIDMap);
            }
        }

        protected async Task SaveAssetAsyncImpl(TID assetID, byte[] thumbnail, bool readOnlyIDMap)
        {
            if (m_assetIDToExternalAsset.ContainsKey(assetID))
            {
                Debug.LogWarning($"The external asset {assetID} cannot be saved");
                return;
            }

            if (!TryGetObject(assetID, out var asset))
            {
                throw new ArgumentException($"Asset {assetID} not found. Use CreateAssetAsync or LoadAssetAsync method");
            }

            if (!TryGetMeta(asset, out TMeta meta))
            {
                throw new ArgumentException($"Metadata {assetID} not found");
            }

            if (!meta.ID.Equals(assetID))
            {
                throw new ArgumentException($"{assetID} is not the identifier of the root asset", nameof(assetID));
            }

            if (IsBinaryData(in meta))
            {
                await SaveBinaryDataAsync(GetDataFileID(in meta), asset);
            }
            else
            {
                using var serializeRef = m_deps.AcquireSerializerRef();
                using var idMapRef = m_deps.AcquireIDMapRef(m_idMap);
                var idMap = idMapRef.Get();
                //Saving the instantiable asset shouldn't change anything. ApplyChanges should have done all the work
                idMap.IsReadOnly = readOnlyIDMap;

                if (!idMap.IsReadOnly)
                {
                    if (m_assetMap.TryGetIDMapByRootAsset(asset, out var existingIDMap))
                    {
                        idMap = existingIDMap;
                    }
                }

                var serializer = serializeRef.Get();
#if SINGLE_THREADED
                var serializationResult = await SerializeAssetAsync(assetID, asset, serializer, m_assetMap, idMap);
                var serializeDataTask = SerializeDataAsync(GetDataFileID(in meta), serializer);
#else
                var serializeDataTask = SerializeDataAsync(GetDataFileID(in meta), serializer);
                var serializationResult = await SerializeAssetAsync(assetID, asset, serializer, m_assetMap, idMap);
#endif
                if (!idMap.IsReadOnly)
                {
                    m_assetMap.Remove(asset);

                    var newIDMap = m_deps.AcquireIDMap(m_idMap);
                    await FillIDMapAsync(asset, idMap, newIDMap);

                    idMapRef.Detach();
                    m_deps.ReleaseIDMap(idMap);
                    idMap = newIDMap;

                    m_assetMap.AddRootAsset(asset, idMap, null);
                    idMap.Commit();
                }

                var updateMetaHs = UpdateInboundDepsOfOutboundDeps(assetID, serializationResult.OutboundDeps, meta.OutboundDependencies);
                meta.OutboundDependencies = serializationResult.OutboundDeps;
                meta.Links = serializationResult.RootInstanceToRootAssetID;
                meta.ClearLinkMaps();
                foreach (var kvp in serializationResult.RootInstanceToAssetToInstanceID)
                {
                    var rootInstanceID = kvp.Key;
                    var assetToInstanceID = kvp.Value;
                    meta.AddLinkMap(rootInstanceID, assetToInstanceID);
                }

                meta.MarkedAsDestroyed = serializationResult.MarkedAsDestroyed.Count > 0 ? serializationResult.MarkedAsDestroyed : null;

                SetMeta(assetID, in meta);
                await serializeDataTask;
                await SerializeDepsMetaAsync(updateMetaHs);
            }

            if (thumbnail != null)
            {
                if (thumbnail.Length == 0)
                {
                    m_idToThumbnail.Remove(assetID);
                }
                else
                {
                    m_idToThumbnail[assetID] = thumbnail;
                }
            }

            await SerializeMetaAndThumbnailAsync(GetMetaFileID(in meta), meta, GetThumbnailFileID(in meta), thumbnail);
        }

        private HashSet<TMeta> UpdateInboundDepsOfOutboundDeps(TID id, HashSet<TID> outboundDeps, IEnumerable<TID> existingOutboundDeps = null)
        {
            HashSet<TMeta> updateMetaHs = null;

            if (existingOutboundDeps != null)
            {
                foreach (TID dependencyID in existingOutboundDeps)
                {
                    if (TryGetMeta(dependencyID, out TMeta depMeta))
                    {
                        if (!outboundDeps.Contains(dependencyID))
                        {
                            if (depMeta.InboundDependencies != null && depMeta.InboundDependencies.Remove(id))
                            {
                                if (updateMetaHs == null)
                                {
                                    updateMetaHs = new HashSet<TMeta>();
                                }

                                updateMetaHs.Add(depMeta);
                                SetMeta(dependencyID, in depMeta);
                            }
                        }
                    }
                }
            }

            foreach (TID dependencyID in outboundDeps)
            {
                if (TryGetMeta(dependencyID, out TMeta depMeta))
                {
                    //inbound dependencies are dependencies in which other objects depend on this object
                    if (depMeta.InboundDependencies == null)
                    {
                        depMeta.InboundDependencies = new HashSet<TID>();
                    }

                    if (depMeta.InboundDependencies.Add(id))
                    {
                        if (updateMetaHs == null)
                        {
                            updateMetaHs = new HashSet<TMeta>();
                        }

                        updateMetaHs.Add(depMeta);
                        SetMeta(dependencyID, in depMeta);
                    }
                }
            }

            return updateMetaHs;
        }

        private async Task SaveBinaryDataAsync(TFID dataFileID, object newAsset)
        {
            await TaskUtils.Run(async () =>
            {
                var dataLayer = m_deps.DataLayer;
                var stream = await dataLayer.OpenWriteAsync(dataFileID);
                try
                {
                    var data = (BinaryData)newAsset;
                    stream.Write(data.Bytes, 0, data.Bytes.Length);
                }
                finally
                {
                    await dataLayer.ReleaseAsync(stream);
                }
            });
        }

        private async Task SerializeDataAsync(TFID dataFileID, ISurrogatesSerializer<TID> serializer)
        {
            await TaskUtils.Run(async () =>
            {
                var dataLayer = m_deps.DataLayer;
                try
                {
                    var stream = await dataLayer.OpenWriteAsync(dataFileID);
                    try
                    {
                        await serializer.SerializeToStream(stream);
                    }
                    finally
                    {
                        await dataLayer.ReleaseAsync(stream);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            });
        }

        private Task SerializeDepsMetaAsync(IEnumerable<TMeta> metas)
        {
            if (metas == null)
            {
                return Task.CompletedTask;
            }

            return TaskUtils.Run(async () =>
            {
                var dataLayer = m_deps.DataLayer;
                var messagePackSerializer = m_deps.Serializer;

                // TODO: Use named lock (named_lock(meta.FileID))
                await Task.WhenAll(metas.Select(meta => SerializeMetaAsync(GetMetaFileID(in meta), dataLayer, messagePackSerializer, meta)));
            });
        }

        private Task SerializeMetaAndThumbnailAsync(TFID metaFileID, TMeta meta, TFID thumbnailID, byte[] data)
        {
            return TaskUtils.Run(async () =>
            {
                var dataLayer = m_deps.DataLayer;
                var serializer = m_deps.Serializer;
                await SerializeMetaAsync(metaFileID, dataLayer, serializer, meta);
                if (data != null)
                {
                    TThumbnail thumbnail = new TThumbnail();
                    thumbnail.Data = data;
                    await SerializeThumbnailAsync(thumbnailID, dataLayer, thumbnail);
                }
            });
        }

        protected async Task SerializeMetaAsync(TFID metaFileID, IDataLayer<TFID> dataLayer, ISerializer serializer, TMeta meta)
        {
            var stream = await dataLayer.OpenWriteAsync(metaFileID);
            try
            {
                serializer.Serialize(stream, meta);
            }
            finally
            {
                await dataLayer.ReleaseAsync(stream);
            }
        }

        private static async Task SerializeThumbnailAsync(TFID fileID, IDataLayer<TFID> dataLayer, TThumbnail thumbnail)
        {
            if (thumbnail.Data != null && thumbnail.Data.Length == 0)
            {
                await dataLayer.DeleteAsync(fileID);
                return;
            }

            var stream = await dataLayer.OpenWriteAsync(fileID);
            try
            {
                if (thumbnail.Data != null)
                {
                    stream.Write(thumbnail.Data, 0, thumbnail.Data.Length);
                }
            }
            finally
            {
                await dataLayer.ReleaseAsync(stream);
            }
        }

        private Task SerializeExternalDataAsync(TFID fileID, TExternalData externalData)
        {
            return TaskUtils.Run(async () =>
            {
                var dataLayer = m_deps.DataLayer;
                var serializer = m_deps.Serializer;
                await SerializeExternalDataAsync(fileID, dataLayer, serializer, externalData);
            });
        }

        private static async Task SerializeExternalDataAsync(TFID fileID, IDataLayer<TFID> dataLayer, ISerializer serializer, TExternalData externalData)
        {
            var stream = await dataLayer.OpenWriteAsync(fileID);
            try
            {
                serializer.Serialize(stream, externalData);
            }
            finally
            {
                await dataLayer.ReleaseAsync(stream);
            }
        }

        /// <summary>
        /// This method required to nullify references to game objects out of current subtree when creating "prefabs".
        /// Must be moved to derived class. this class should not make assumptions about asset types.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        protected virtual bool ShouldSerialize(IObjectTreeEnumerator enumerator)
        {
            object parent = enumerator.Parent;
            if (ReferenceEquals(parent, enumerator.Root))
            {
                return true;
            }

            UnityEngine.Object obj;
            GameObject go = parent as GameObject;
            if (go == null)
            {
                if (parent is Component component)
                {
                    obj = component;
                    go = component.gameObject;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                obj = go;
            }

            //Another hack here (this check should be moved somewhere else) 
            if (obj.hideFlags.HasFlag(HideFlags.DontSave) || IsObjectHidden(obj))
            {
                // do not serialize game objects with DontSave flag
                return false;
            }

            return IsInHierarchy(enumerator, go);
        }

        private static bool IsInHierarchy(IObjectTreeEnumerator enumerator, object obj)
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                if (obj is Component component)
                {
                    go = component.gameObject;
                }
                else
                {
                    return true;
                }
            }

            Transform transform = go.transform;
            while (true)
            {
                if (ReferenceEquals(enumerator.Root, transform.gameObject))
                {
                    return true;
                }

                transform = transform.parent;
                if (transform == null)
                {
                    if (enumerator.Root is Scene)
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        private class SerializeAssetResult
        {
            public HashSet<TID> OutboundDeps
            {
                get;
                private set;
            }

            public Dictionary<TID, TID> RootInstanceToRootAssetID // (root instance id -> root asset id)
            {
                get;
                private set;
            }

            public Dictionary<TID, Dictionary<TID, TID>> RootInstanceToAssetToInstanceID // (root asset id -> (asset id -> instance id))
            {
                get;
                private set;
            }

            public HashSet<TID> MarkedAsDestroyed
            {
                get;
                private set;
            }

            public SerializeAssetResult(HashSet<TID> outboundDeps, Dictionary<TID, TID> rootInstanceToRootAssetID, Dictionary<TID, Dictionary<TID, TID>> rootAssetToAssetToInstanceID, HashSet<TID> markedAsDestroyed)
            {
                OutboundDeps = outboundDeps;
                RootInstanceToRootAssetID = rootInstanceToRootAssetID;
                RootInstanceToAssetToInstanceID = rootAssetToAssetToInstanceID;
                MarkedAsDestroyed = markedAsDestroyed;
            }
        }

        private async Task SerializeObjectAsync(object obj, ISurrogatesSerializer<TID> serializer, IIDMap<TID> idMap, TID assetID = default)
        {
            using var contextRef = m_deps.AcquireContextRef();
            var context = contextRef.Get();
            context.IDMap = idMap;
            context.ShaderUtil = m_deps.ShaderUtil;
            context.TypeMap = m_deps.TypeMap;
            context.TempRoot = m_deps.AssetsRoot;

            await SerializeAssetAsync(assetID, obj, serializer, context, m_assetMap, null, null);
        }

        private async Task SerializeAssetAsync(TID currentRootAssetID, object rootAsset, ISurrogatesSerializer<TID> serializer, ISerializationContext<TID> context, IAssetMap<TID> assetMap, HashSet<TID> outboundDeps, HashSet<object> rootInstances)
        {
            object enumeratorParent = null;
            bool shouldSerialize = false;
            var workloadController = m_deps.WorkloadController;

            using var assetEnumeratorRef = m_deps.AcquireEnumeratorRef(rootAsset);
            var assetEnumerator = assetEnumeratorRef.Get();
            var typeMap = m_deps.TypeMap;
            while (MoveNext(assetEnumerator, typeMap))
            {
                await workloadController.TryPostponeTask();

                if (IsObjectDestoryed(assetEnumerator.Current))
                {
                    // Debug.LogWarning($"enumerator.Current = null {assetEnumerator.GetType()} {assetEnumerator.Root.GetType()}");
                    continue;
                }

                TID dependencyID;
                bool enumeratorParentChanged = false;
                if (assetEnumerator.Parent != enumeratorParent)
                {
                    enumeratorParentChanged = true;
                    enumeratorParent = assetEnumerator.Parent;

                    if (context.IDMap.TryGetID(enumeratorParent, out _) && assetMap.TryGetRootAssetIDByAsset(enumeratorParent, out dependencyID) && !dependencyID.Equals(currentRootAssetID))
                    {
                        // when id is not equal to currentRootAssetID this means that this object has dependency on another asset
                        outboundDeps?.Add(dependencyID);

                        // skip enumeration on current parent
                        assetEnumerator.Trim();
                        continue;
                    }

                    shouldSerialize = ShouldSerialize(assetEnumerator);
                }

                if (context.IDMap.TryGetID(assetEnumerator.Current, out _) && assetMap.TryGetRootAssetIDByAsset(assetEnumerator.Current, out dependencyID) && !dependencyID.Equals(currentRootAssetID))
                {
                    outboundDeps?.Add(dependencyID);
                    continue;
                }

                if (!shouldSerialize)
                {
                    assetEnumerator.Trim();
                    continue;
                }

                if (rootInstances != null && enumeratorParentChanged && assetMap.IsRootInstance(enumeratorParent) && assetMap.TryGetRootAssetIDByInstance(enumeratorParent, out var _))
                {
                    rootInstances.Add(enumeratorParent);
                }

                object obj = assetEnumerator.Current;
                if (IsExternalAssetRoot(obj))
                {
                    continue;
                }

                if (context.IDMap.IsReadOnly && !context.IDMap.TryGetID(obj, out _))
                {
                    continue;
                }

                if (assetMap.IsInstance(obj))
                {
                    // check if it is marked as dirty
                    if (assetMap.IsDirty(obj) || IsDirtyByDefault(obj, assetMap))
                    {
                        // serialize
                        await serializer.Enqueue(obj, context);
                    }
                    else
                    {
                        object rootRepresentation = GetObjectRootRepresentation(obj);
                        if (rootRepresentation != null && !context.IDMap.TryGetID(rootRepresentation, out _))
                        {
                            var instanceID = context.IDMap.CreateID();
                            context.IDMap.AddObject(rootRepresentation, instanceID);
                        }

                        if (!context.IDMap.TryGetID(obj, out _))
                        {
                            var instanceID = context.IDMap.CreateID();
                            context.IDMap.AddObject(obj, instanceID);
                        }
                    }
                }
                else
                {
                    await serializer.Enqueue(obj, context);
                }
            }

            //null == end
            await serializer.Enqueue(null, context);
        }

        private async Task<SerializeAssetResult> SerializeAssetAsync(TID currentRootAssetID, object rootAsset, ISurrogatesSerializer<TID> serializer, IAssetMap<TID> assetMap, IIDMap<TID> idMap)
        {
            var outboundDeps = new HashSet<TID>();
            var rootInstances = new HashSet<object>();
            var assetIDsMarkedAsDestroyed = new HashSet<TID>();

            using var contextRef = m_deps.AcquireContextRef();
            var context = contextRef.Get();
            context.IDMap = idMap;
            context.ShaderUtil = m_deps.ShaderUtil;
            context.TypeMap = m_deps.TypeMap;
            context.TempRoot = m_deps.AssetsRoot;

            await SerializeAssetAsync(currentRootAssetID, rootAsset, serializer, context, assetMap, outboundDeps, rootInstances);

            var rootInstanceToRootAssetID = new Dictionary<TID, TID>();
            var rootInstanceToAssetToInstanceID = new Dictionary<TID, Dictionary<TID, TID>>();
            foreach (object rootInstance in rootInstances)
            {
                using var instanceEnumeratorRef = m_deps.AcquireEnumeratorRef(rootInstance);

                if (!context.IDMap.TryGetID(rootInstance, out var rootInstanceID))
                {
                    throw new KeyNotFoundException($"Root instance ID not found for root instance {rootInstance}");
                }

                if (!assetMap.TryGetRootAssetIDByInstance(rootInstance, out var rootAssetID))
                {
                    throw new KeyNotFoundException($"Root asset ID not found for root instance {rootInstance}");
                }

                var assetToInstanceID = new Dictionary<TID, TID>();
                rootInstanceToAssetToInstanceID.Add(rootInstanceID, assetToInstanceID);
                rootInstanceToRootAssetID.Add(rootInstanceID, rootAssetID);
                outboundDeps.Add(rootAssetID);

                if (!assetMap.TryGetAssetToInstanceMapByRootInstance(rootInstance, out var assetToInstanceMap))
                {
                    throw new KeyNotFoundException($"Asset To Instance Map not found for root instance {rootInstance}");
                }

                if (assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(rootInstance, out var assetsMarkedAsDestroyed))
                {
                    foreach (object asset in assetsMarkedAsDestroyed)
                    {
                        if (idMap.TryGetID(asset, out TID assetID))
                        {
                            assetIDsMarkedAsDestroyed.Add(assetID);
                        }
                    }
                }

                foreach (var kvp in assetToInstanceMap)
                {
                    var asset = kvp.Key;
                    if (!idMap.TryGetID(asset, out TID assetID))
                    {
                        Debug.LogWarning($"assetID not found {asset}");
                        continue;
                    }

                    TID instanceID;
                    var instance = kvp.Value;
                    if (!IsObjectDestoryed(instance))
                    {
                        if (!idMap.TryGetID(instance, out instanceID))
                        {
                            Debug.LogWarning($"instanceID not found {asset}");
                            continue;
                        }
                    }
                    else
                    {
                        instanceID = idMap.CreateID();
                        assetIDsMarkedAsDestroyed.Add(instanceID);
                    }

                    assetToInstanceID[assetID] = instanceID;
                }

                /*
                object instanceEnumeratorParent = null;
                bool isInHierarchy = false;
                var instanceEnumerator = instanceEnumeratorRef.Get();

                while (instanceEnumerator.MoveNext())
                {
                    if (instanceEnumerator.Current == null)
                    {
                        Debug.LogWarning($"instanceEnumerator.Current = null {instanceEnumerator.GetType()} {instanceEnumerator.Root.GetType()}");
                        continue;
                    }

                    bool enumeratorParentChanged = false;
                    if (instanceEnumerator.Parent != instanceEnumeratorParent)
                    {
                        instanceEnumeratorParent = instanceEnumerator.Parent;
                        enumeratorParentChanged = true;

                        if (assetMap.TryGetRootAssetIDByAsset(instanceEnumeratorParent, out var parentDepID) && !parentDepID.Equals(currentRootAssetID))
                        {
                            // when id is not equal to currentRootAssetID this means that this object has dependency on another asset
                            instanceEnumerator.Trim();
                            continue;
                        }
                    }

                    if (assetMap.TryGetRootAssetIDByAsset(instanceEnumerator.Current, out var depID) && !depID.Equals(currentRootAssetID))
                    {
                        continue;
                    }

                    if (enumeratorParentChanged)
                    {
                        isInHierarchy = IsInHierarchy(instanceEnumerator, instanceEnumeratorParent);
                        if (isInHierarchy)
                        {
                            if (assetMap.TryGetAssetIDByInstance(instanceEnumeratorParent, out var assetID))
                            {
                                if (context.IDMap.TryGetID(instanceEnumeratorParent, out var instanceID))
                                {
                                    assetToInstanceID[assetID] = instanceID;
                                }
                            }
                        }
                    }

                    if (isInHierarchy)
                    {
                        if (assetMap.TryGetAssetIDByInstance(instanceEnumerator.Current, out var assetID))
                        {
                            if (context.IDMap.TryGetID(instanceEnumerator.Current, out var instanceID))
                            {
                                assetToInstanceID[assetID] = instanceID;
                            }
                        }
                    }
                    else
                    {
                        instanceEnumerator.Trim();
                    }
                }
                */
            }

            return new SerializeAssetResult(outboundDeps, rootInstanceToRootAssetID, rootInstanceToAssetToInstanceID, assetIDsMarkedAsDestroyed);
        }

        private void Enumerate<TContext>(object obj, TContext context, Action<object, TContext> callback)
        {
            using var objEnumeratorRef = m_deps.AcquireEnumeratorRef(obj);
            var objEnumerator = objEnumeratorRef.Get();
            Enumerate(objEnumerator, context, callback);
        }

        private void Enumerate<TContext>(IObjectTreeEnumerator objEnumerator, TContext context, Action<object, TContext> callback)
        {
            var typeMap = m_deps.TypeMap;
            while (MoveNext(objEnumerator, typeMap))
            {
                if (objEnumerator.Current != null)
                {
                    object rootRepresentation = GetObjectRootRepresentation(objEnumerator.Current);
                    if (rootRepresentation != null)
                    {
                        callback(rootRepresentation, context);
                    }
                    callback(objEnumerator.Current, context);
                }
            }
        }

        private async Task EnumerateAsync<TContext>(object obj, TContext context, Action<object, TContext> callback, Func<object, bool> filter = null)
        {
            var workloadCtrl = m_deps.WorkloadController;
            using var objEnumeratorRef = m_deps.AcquireEnumeratorRef(obj);
            var objEnumerator = objEnumeratorRef.Get();
            var typeMap = m_deps.TypeMap;
            while (MoveNext(objEnumerator, typeMap))
            {
                await workloadCtrl.TryPostponeTask();
                if (objEnumerator.Current != null)
                {
                    object rootRepresentation = GetObjectRootRepresentation(objEnumerator.Current);
                    if (rootRepresentation != null)
                    {
                        if (filter != null)
                        {
                            if (filter(rootRepresentation))
                            {
                                callback(rootRepresentation, context);
                            }
                        }
                        else
                        {
                            callback(rootRepresentation, context);
                        }
                    }

                    if (filter != null)
                    {
                        if (filter(objEnumerator.Current))
                        {
                            callback(objEnumerator.Current, context);
                        }
                    }
                    else
                    {
                        callback(objEnumerator.Current, context);
                    }
                }
            }
        }

        private void GetUnusedObjects(IIDMap<TID> oldIDMap, IIDMap<TID> newIDMap, HashSet<object> unusedObjects)
        {
            foreach (var kvp in oldIDMap.IDToObject)
            {
                var id = kvp.Key;
                object obj = kvp.Value;
                if (!newIDMap.IDToObject.ContainsKey(id))
                {
                    unusedObjects.Add(obj);
                }
            }
        }

        public Task FillIDMapAsync(object obj, IIDMap<TID> objIDMap, IIDMap<TID> outIDMap)
        {
            using var objEnumeratorRef = m_deps.AcquireEnumeratorRef(obj);
            var objEnumerator = objEnumeratorRef.Get();
            var typeMap = m_deps.TypeMap;
            while (MoveNext(objEnumerator, typeMap))
            {
                if (objEnumerator.Current != null)
                {
                    object rootRepresentation = GetObjectRootRepresentation(objEnumerator.Current);
                    if (rootRepresentation != null)
                    {
                        TryAddObjectToIDMap(rootRepresentation, objIDMap, outIDMap);
                    }

                    TryAddObjectToIDMap(objEnumerator.Current, objIDMap, outIDMap);
                }
            }
            return Task.CompletedTask;
        }

        private bool TryAddObjectToIDMap(object obj, IIDMap<TID> objIDMap, IIDMap<TID> outIDMap)
        {
            if (obj == null)
            {
                return false;
            }

            if (outIDMap.TryGetID(obj, out _))
            {
                return false;
            }

            TID id;
            if (objIDMap != null)
            {
                bool isInIDMap = objIDMap.ObjectToID.TryGetValue(obj, out id);
                if (!isInIDMap)
                {
                    bool isInParentIDMap = objIDMap.ParentMap != null && objIDMap.ParentMap.TryGetID(obj, out _);
                    if (isInParentIDMap)
                    {
                        return false;
                    }

                    id = outIDMap.CreateID();
                }

                // don't check if obj is present in m_idmap because we will call Rollback on idmap before commit on target idmap
            }
            else
            {
                id = outIDMap.CreateID();
            }

            outIDMap.AddObject(obj, id);
            return true;
        }

        private bool MoveNext(IObjectTreeEnumerator enumerator, ITypeMap typeMap)
        {
            do
            {
                if (!enumerator.MoveNext())
                {
                    return false;
                }
            }
            while (enumerator.Current != null && !typeMap.TryGetID(enumerator.Current.GetType(), out _));
            return true;
        }

        private struct ObjectMap
        {
            public Dictionary<object, object> Map
            {
                get;
            }
            public bool IsCorrupted
            {
                get;
            }

            public void ThrowIfCorrupted()
            {
                if (IsCorrupted)
                {
                    var sb = new StringBuilder();
                    sb.Append(Environment.NewLine);
                    foreach (var kvp in Map)
                    {
                        sb.Append($"{kvp.Key} -> {kvp.Value}").Append(Environment.NewLine);
                    }

                    throw new Exception($"Unable to create a valid object map: {Environment.NewLine}" +
                        $"Ensure that UnityEngine.Object.Instantiate(obj) creates a valid copy of the object and that properties used in surrogate and enumerator code do not return null after copying. {Environment.NewLine}" +
                        $"Make sure that every object property that you access using a surrogate or enumerator has a serializable backing field (it's public or marked with the SerializeField attribute). {Environment.NewLine}" +
                        $"Object Map: {sb.ToString()}");
                }

            }

            public ObjectMap(Dictionary<object, object> dict, bool isCorrupted)
            {
                Map = dict;
                IsCorrupted = isCorrupted;
            }
        }

        public async Task<Dictionary<object, object>> GetObjectMapAsync(object source, object target, IWorkloadController workloadController)
        {
            var objectMap = await CreateObectMapAsync(source, target, workloadController);
            return objectMap.Map;
        }

        private async Task<ObjectMap> CreateObectMapAsync(object source, object target, IWorkloadController workloadController)
        {
            using var targetEnumeratorRef = m_deps.AcquireEnumeratorRef(target);
            using var sourceEnumeratorRef = m_deps.AcquireEnumeratorRef(source);
            var targetEnumerator = targetEnumeratorRef.Get();
            var sourceEnumerator = sourceEnumeratorRef.Get();
            var objectMap = new Dictionary<object, object>();
            var typeMap = m_deps.TypeMap;

            bool isCorrupted = false;
            object currentParent = null;
            while (MoveNext(targetEnumerator, typeMap))
            {
                if (workloadController != null)
                {
                    await workloadController.TryPostponeTask();
                }

                if (!MoveNext(sourceEnumerator, typeMap))
                {
                    isCorrupted = true;
                    Debug.LogWarning("Object map is corrupted");
                    break;
                }

                if (sourceEnumerator.Parent != currentParent)
                {
                    currentParent = sourceEnumerator.Parent;

                    if (sourceEnumerator.Parent == targetEnumerator.Parent || !IsInHierarchy(sourceEnumerator, sourceEnumerator.Parent))
                    {
                        targetEnumerator.Trim();
                        sourceEnumerator.Trim();
                        continue;
                    }
                }

                bool sourceDestroyed = sourceEnumerator.Current == null;
                bool targetDestroyed = targetEnumerator.Current == null;
                if (sourceDestroyed && targetDestroyed)
                {
                    continue;
                }
                else
                {
                    if (sourceDestroyed || targetDestroyed)
                    {
                        continue;
                    }
                }

                if (sourceEnumerator.Current != targetEnumerator.Current)
                {
                    objectMap[sourceEnumerator.Current] = targetEnumerator.Current;
                }

                object sourceRootRepresentation = GetObjectRootRepresentation(sourceEnumerator.Current);
                object targetRootRepresentation = GetObjectRootRepresentation(targetEnumerator.Current);

                sourceDestroyed = sourceRootRepresentation == null;
                targetDestroyed = targetRootRepresentation == null;
                if (sourceDestroyed && targetDestroyed)
                {
                    continue;
                }
                else
                {
                    if (sourceDestroyed || targetDestroyed)
                    {
                        continue;
                    }
                }

                if (sourceRootRepresentation != targetRootRepresentation)
                {
                    objectMap[sourceRootRepresentation] = targetRootRepresentation;
                }
            }

            while (MoveNext(sourceEnumerator, typeMap)) { isCorrupted = true; }
            return new ObjectMap(objectMap, isCorrupted);
        }

        public Dictionary<object, object> GetObjectMap(object source, object target)
        {
            return CreateObjectMap(source, target).Map;
        }

        private ObjectMap CreateObjectMap(object source, object target)
        {
            var task = CreateObectMapAsync(source, target, null);

            Debug.Assert(task.IsCompleted);

            return task.Result;
        }

        public void DumpObjectTree(object obj)
        {
            var sb = new System.Text.StringBuilder();
            var enumeratorRef = m_deps.AcquireEnumeratorRef(obj);
            var enumerator = enumeratorRef.Get();
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                if (current == null)
                {
                    sb.Append("null");
                }
                else if (IsObjectDestoryed(current))
                {
                    if (current is UnityEngine.Object)
                    {
                        var uo = (UnityEngine.Object)current;
                        sb.Append($"Destroyed {uo.GetType().FullName} {GetObjectName(current)}");
                    }
                    else
                    {
                        sb.Append("Destroyed Unknown");
                    }
                }
                else
                {
                    sb.Append($"{current.GetType().FullName} {GetObjectName(current)}");
                }
                sb.Append(Environment.NewLine);
            }

            Debug.Log(sb.ToString());
        }

        public bool AreEqual(object source, object target, IReadOnlyDictionary<object, object> sourceToTargetMap)
        {
            return AreEqual(source, target, sourceToTargetMap, null, null);
        }

        private bool AreEqual(object source, object target, IReadOnlyDictionary<object, object> sourceToTargetMap, HashSet<object> sourceMarkedAsDestroyed, HashSet<object> targetMarkedAsDestroyed)
        {
            var targetEnumeratorRef = m_deps.AcquireEnumeratorRef(target);
            var sourceEnumeratorRef = m_deps.AcquireEnumeratorRef(source);
            var targetEnumerator = targetEnumeratorRef.Get();
            var sourceEnumerator = sourceEnumeratorRef.Get();
            var typeMap = m_deps.TypeMap;

            object currentParent = null;
            while (MoveNext(targetEnumerator, typeMap))
            {
                if (targetMarkedAsDestroyed != null && targetMarkedAsDestroyed.Count > 0)
                {
                    if (targetMarkedAsDestroyed.Contains(targetEnumerator.Current))
                    {
                        continue;
                    }

                    if (targetMarkedAsDestroyed.Contains(targetEnumerator.Parent))
                    {
                        targetEnumerator.Trim();
                        continue;
                    }
                }

                if (sourceMarkedAsDestroyed == null || sourceMarkedAsDestroyed.Count == 0)
                {
                    if (!MoveNext(sourceEnumerator, typeMap))
                    {
                        return false;
                    }
                }
                else
                {
                    while (true)
                    {
                        if (!MoveNext(sourceEnumerator, typeMap))
                        {
                            return false;
                        }

                        if (sourceMarkedAsDestroyed.Contains(sourceEnumerator.Parent))
                        {
                            sourceEnumerator.Trim();
                            continue;
                        }

                        if (!sourceMarkedAsDestroyed.Contains(sourceEnumerator.Current))
                        {
                            break;
                        }
                    }
                }

                if (sourceEnumerator.Parent != currentParent)
                {
                    currentParent = sourceEnumerator.Parent;

                    if (sourceEnumerator.Parent != targetEnumerator.Parent && IsInHierarchy(sourceEnumerator, sourceEnumerator.Parent))
                    {
                        if (targetEnumerator.Parent == null || sourceEnumerator.Parent == null)
                        {
                            return false;
                        }

                        if (targetEnumerator.Parent.GetType() != sourceEnumerator.Parent.GetType())
                        {
                            return false;
                        }

                        if (!sourceToTargetMap.TryGetValue(sourceEnumerator.Parent, out var targetParent))
                        {
                            return false;
                        }

                        if (targetEnumerator.Parent != targetParent)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        targetEnumerator.Trim();
                        sourceEnumerator.Trim();
                        continue;
                    }
                }

                if (sourceEnumerator.Current != targetEnumerator.Current)
                {
                    if (targetEnumerator.Current == null || sourceEnumerator.Current == null)
                    {
                        return false;
                    }

                    if (targetEnumerator.Current.GetType() != sourceEnumerator.Current.GetType())
                    {
                        return false;
                    }

                    if (!sourceToTargetMap.TryGetValue(sourceEnumerator.Current, out var targetCurrent))
                    {
                        return false;
                    }

                    if (targetEnumerator.Current != targetCurrent)
                    {
                        return false;
                    }
                }
            }
            return !MoveNext(sourceEnumerator, typeMap);
        }

        public Task<Dictionary<object, object>> CopyObjectAsync(object source, IReadOnlyDictionary<object, object> sourceToTargetMap, ICollection<object> markedAsDestroyed, bool strict = false)
        {
            return CopyObjectAsync(source, sourceToTargetMap, markedAsDestroyed, null, strict);
        }

        /// <summary>
        /// Copy source object to target.
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="sourceToTargetMap">existing source to target map</param>
        /// <param name="strict">if true, removes anything from the target that is not present in the source, defaults to false</param>
        /// <returns>new source to target map</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<Dictionary<object, object>> CopyObjectAsync(object source, IReadOnlyDictionary<object, object> sourceToTargetMap, ICollection<object> markedAsDestroyed, IAssetMap<TID> assetMap, bool strict = false)
        {
            var newSourceToTargetMap = new Dictionary<object, object>();

            var workloadCtrl = m_deps.WorkloadController;

            using var sourceEnumeratorRef = m_deps.AcquireEnumeratorRef(source);
            using var serializerRef = m_deps.AcquireSerializerRef();
            using var idMapRef = m_deps.AcquireIDMapRef(m_idMap);
            using var contextRef = m_deps.AcquireContextRef();

            var context = m_deps.AcquireContext();
            var idMap = idMapRef.Get();
            context.IDMap = idMap;
            context.ShaderUtil = m_deps.ShaderUtil;
            context.TypeMap = m_deps.TypeMap;
            context.TempRoot = GetObjectParent(source);

            var serializer = serializerRef.Get();
            var sourceEnumerator = sourceEnumeratorRef.Get();

            using var targetIDMapRef = m_deps.AcquireIDMapRef();
            var targetIDMap = targetIDMapRef.Get();
            var idToNewObject = new Dictionary<TID, object>();

            object target = sourceToTargetMap[source];
            string name = GetObjectName(target);

            // this is needed to support external assets that might not be serializable
            var sourceCopy = InstantiateObject(source, m_deps.AssetsRoot);
            using var sourceCopyEnumeratorRef = m_deps.AcquireEnumeratorRef(sourceCopy);
            var sourceCopyEnumerator = sourceCopyEnumeratorRef.Get();

            static bool IsMarkedAsDestroyed(ICollection<object> markedAsDestroyed, object obj)
            {
                return markedAsDestroyed != null && markedAsDestroyed.Contains(obj);
            }

            var typeMap = m_deps.TypeMap;

            // serialize source object
           

            while (MoveNext(sourceEnumerator, typeMap))
            {
                MoveNext(sourceCopyEnumerator, typeMap);

                await workloadCtrl.TryPostponeTask();
                if (sourceEnumerator.Current == null)
                {
                    continue;
                }

                object currentSource = sourceEnumerator.Current;
                object rootRepresentation = GetObjectRootRepresentation(currentSource);

                if (IsMarkedAsDestroyed(markedAsDestroyed, currentSource) || IsMarkedAsDestroyed(markedAsDestroyed, rootRepresentation))
                {
                    continue;
                }

                if (sourceToTargetMap.TryGetValue(currentSource, out var currentTarget))
                {
                    // this is to prevent overriding instances marked as dirty due to changes made to the asset.
                    bool serialize = assetMap == null || !m_assetMap.IsDirty(currentTarget);
                    if (serialize)
                    {
                        await serializer.Enqueue(currentSource, context);
                        serializer.CopyToDeserializationQueue();
                    }
                    else
                    {
                        if (rootRepresentation != null)
                        {
                            idMap.GetOrCreateID(rootRepresentation);
                        }
                        idMap.GetOrCreateID(currentSource);
                    }

                    if (rootRepresentation != null)
                    {
                        if (sourceToTargetMap.TryGetValue(rootRepresentation, out var targetRootRepresentation))
                        {
                            newSourceToTargetMap.Add(rootRepresentation, targetRootRepresentation);
                        }
                    }

                    newSourceToTargetMap.Add(currentSource, currentTarget);
                }
                else
                {
                    if (!HasInstantiableType(currentSource))
                    {
                        continue;
                    }

                    bool enqueued = await serializer.Enqueue(currentSource, context);
                    if (!enqueued)
                    {
                        continue;
                    }

                    serializer.CopyToDeserializationQueue();

                    if (rootRepresentation != null)
                    {
                        if (!context.IDMap.TryGetID(rootRepresentation, out var rootRepresentationID))
                        {
                            rootRepresentationID = context.IDMap.CreateID();
                            context.IDMap.AddObject(rootRepresentation, rootRepresentationID);
                        }

                        idToNewObject.Add(rootRepresentationID, rootRepresentation);
                        targetIDMap.AddObject(GetObjectRootRepresentation(sourceCopyEnumerator.Current), rootRepresentationID);
                    }

                    if (!context.IDMap.TryGetID(currentSource, out var currentID))
                    {
                        currentID = context.IDMap.CreateID();
                        context.IDMap.AddObject(currentSource, currentID);
                    }

                    var rootRepresentationCopy = GetObjectRootRepresentation(sourceCopyEnumerator.Current, -1);
                    if (rootRepresentationCopy != null && targetIDMap.ObjectToID.ContainsKey(rootRepresentationCopy))
                    {
                        targetIDMap.AddObject(sourceCopyEnumerator.Current, currentID);
                    }

                    idToNewObject.Add(currentID, currentSource);
                }
            }

            await serializer.Enqueue(null, context);
            serializer.CopyToDeserializationQueue();

            // Remove all new Instantiable objects that are not in the sourceToTargetMap
            foreach (var obj in idToNewObject.Values)
            {
                if (idMap.TryGetID(obj, out TID newID))
                {
                    idMap.Remove(newID);
                    if (idMap.TryGetObject<object>(newID, out _))
                    {
                        // mask object in parent id map
                        idMap.AddObject(null, newID);
                    }
                }
            }

            // copy ids from idmap to target map
            foreach (var kvp in sourceToTargetMap)
            {
                object sourceObj = kvp.Key;
                object targetObj = kvp.Value;

                if (idMap.TryGetID(sourceObj, out var sourceObjID))
                {
                    targetIDMap.AddObject(targetObj, sourceObjID);
                }
            }

            // use idMap as fallback for targetIDMap
            targetIDMap.ParentMap = idMap;

            // change id map in context
            context.IDMap = targetIDMap;
            context.TempRoot = m_deps.AssetsRoot;

            // deserialize queued objects
            while (true)
            {
                object targetObj = null;
                try
                {
                    targetObj = await serializer.Dequeue(context);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                if (targetObj is int)
                {
                    if (-1 == (int)targetObj)
                    {
                        await workloadCtrl.TryPostponeTask();
                    }
                    else if (-2 == (int)targetObj)
                    {
                        break;
                    }
                }
            }

            await Task.Yield();

            DestroyObject(sourceCopy);

            foreach (var kvp in idToNewObject)
            {
                var id = kvp.Key;
                var newSource = kvp.Value;
                if (!targetIDMap.TryGetObject(id, out object newTarget))
                {
                    throw new KeyNotFoundException($"object with id {id} not found");
                }
                newSourceToTargetMap.Add(newSource, newTarget);
            }

            // destroy objects that are not in the source
            foreach (var kvp in sourceToTargetMap)
            {
                object sourceObj = kvp.Key;
                object targetObj = kvp.Value;

                if (IsObjectDestoryed(sourceObj) && !HasInstantiableType(sourceObj) || IsMarkedAsDestroyed(markedAsDestroyed, sourceObj))
                {
                    DestroyObject(targetObj);
                }
            }


            SetObjectName(target, name);

            if (strict)
            {
                var newTargetToSourceMap = newSourceToTargetMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

                static void TryDestroy(object obj, (RuntimeAssetDatabaseCore<TID, TFID, TMeta, TThumbnail, TExternalData> assetDatabase, Dictionary<object, object>, Action<object, bool>) args)
                {
                    var (assetDatabase, newTargetToSourceMap, destroy) = args;

                    if (!assetDatabase.IsExternalAssetRoot(obj) && assetDatabase.HasInstantiableType(obj) && !newTargetToSourceMap.ContainsKey(obj))
                    {
                        destroy(obj, false);
                    }
                }

                await EnumerateAsync(target, (this, newTargetToSourceMap, new Action<object, bool>(DestroyObject)), TryDestroy);
            }

            await Task.Yield(); // wait for the end of the frame until the objects are destroyed

            return newSourceToTargetMap;
        }

        private static void PropagateDirtyFlags(IAssetMap<TID> assetMap, IReadOnlyDictionary<object, object> instanceToAssetMap, Dictionary<object, object> newInstanceToAssetMap)
        {
            foreach (var kvp in newInstanceToAssetMap)
            {
                object instance = kvp.Key;
                object asset = kvp.Value;

                if (!assetMap.IsDirty(instance))
                {
                    continue;
                }

                assetMap.ClearDirty(instance, waitForCommit: true);

                if (!assetMap.IsInstance(asset))
                {
                    continue;
                }

                assetMap.SetDirty(asset);
            }

            foreach (var kvp in instanceToAssetMap)
            {
                object instance = kvp.Key;

                if (!assetMap.IsDirty(instance))
                {
                    continue;
                }

                if (newInstanceToAssetMap.ContainsKey(instance))
                {
                    continue;
                }

                assetMap.ClearDirty(instance, waitForCommit: true);
            }
        }

        public async Task<HashSet<object>> PropagateChangesAsync(object obj, IAssetMap<TID> assetMap, bool strict)
        {
            bool isRootInstance = assetMap.IsRootInstance(obj);
            bool isRootAsset = assetMap.IsRootAsset(obj);

            if (!isRootInstance && !isRootAsset)
            {
                throw new ArgumentException("obj is not a root instance or root asset", "obj");
            }

            using var tempRootIDMapRef = m_deps.AcquireIDMapRef();
            using var newAssetMapRef = m_deps.AcquireAssetMapRef();
            var newAssetMap = newAssetMapRef.Get();

            object sourceInstance = null;
            object sourceAsset = obj;
            bool sourceAssetIsVariant = false;
            var sourceAssetPartsMarkedAsDestroyed = new HashSet<object>();

            if (isRootInstance)
            {
                // propagate changes "down" source instance -> source asset

                if (!assetMap.TryGetInstanceToAssetMapByRootInstance(obj, out var sourceInstanceToAssetMap))
                {
                    throw new KeyNotFoundException($"instance to asset map for {obj} not found");
                }

                sourceInstance = obj;
                sourceAsset = sourceInstanceToAssetMap[sourceInstance];
                sourceAssetIsVariant = assetMap.IsRootInstance(sourceAsset);

                HashSet<object> sourceInstancePartsMarkedAsDestroyed = null;
                if (!sourceAssetIsVariant)
                {
                    // physically destroy child assets if the asset is not an asset variant

                    if (!assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(sourceInstance, out sourceInstancePartsMarkedAsDestroyed))
                    {
                        sourceInstancePartsMarkedAsDestroyed = new HashSet<object>();
                    }

                    foreach (var kvp in sourceInstanceToAssetMap)
                    {
                        if (!IsObjectDestoryed(kvp.Key))
                        {
                            continue;
                        }

                        sourceInstancePartsMarkedAsDestroyed.Add(kvp.Key);
                    }
                }
                else
                {
                    if (assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(sourceInstance, out sourceInstancePartsMarkedAsDestroyed))
                    {
                        foreach (object instance in sourceInstancePartsMarkedAsDestroyed)
                        {
                            DestroyObject(instance);
                        }

                        if (sourceInstancePartsMarkedAsDestroyed.Count > 0)
                        {
                            sourceInstancePartsMarkedAsDestroyed.Clear();
                            await Task.Yield();
                        }
                    }
                }

                var newSourceInstanceToAssetMap = await CopyObjectAsync(sourceInstance, sourceInstanceToAssetMap, sourceInstancePartsMarkedAsDestroyed);
                if (!assetMap.TryGetRootAssetByInstance(sourceInstance, out sourceAsset))
                {
                    throw new KeyNotFoundException("source asset for source instance not found");
                }

                bool waitForEndOfFrame = false;
                foreach (var kvp in sourceInstanceToAssetMap)
                {
                    object instance = kvp.Key;
                    if (!HasInstantiableType(instance))
                    {
                        continue;
                    }

                    if (newSourceInstanceToAssetMap.ContainsKey(instance))
                    {
                        continue;
                    }


                    if (!sourceAssetIsVariant)
                    {
                        DestroyObject(instance);
                        waitForEndOfFrame = true;
                    }

                    object asset = kvp.Value;
                    if (!assetMap.TryGetAssetByInstance(asset, out _))
                    {
                        // mark as destroyed only if there is a corresponding object in the underlying asset

                        DestroyObject(asset);
                        waitForEndOfFrame = true;

                        continue;
                    }

                    sourceAssetPartsMarkedAsDestroyed.Add(asset);
                }

                if (waitForEndOfFrame)
                {
                    await Task.Yield();
                    waitForEndOfFrame = false;
                }

                newAssetMap.ParentMap = assetMap;
                PropagateDirtyFlags(newAssetMap, sourceInstanceToAssetMap, newSourceInstanceToAssetMap);

                MapInstances(sourceInstance, newSourceInstanceToAssetMap, detachSourceInstances: true, newAssetMap);
                newAssetMap.ParentMap = null;

                var newSourceAssetToInstanceMap = newSourceInstanceToAssetMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
                newAssetMap.AddRootInstance(sourceInstance, sourceAsset, newSourceAssetToInstanceMap);
            }

            // propagate changes "up" source asset -> instances

            var potentiallyUnusedObjects = new HashSet<object>();
            var deps = assetMap.GetDependencies(sourceAsset).ToArray();
            for (int i = deps.Length - 1; i >= 0; --i)
            {
                if (!assetMap.IsRootAsset(deps[i]))
                {
                    continue;
                }

                object rootAsset = deps[i];
                if (!assetMap.TryGetIDMapByRootAsset(rootAsset, out var rootAssetIDMap))
                {
                    throw new KeyNotFoundException("targetAssetIDMap for targetAsset not found");
                }

                var newIDMap = m_deps.AcquireIDMap(tempRootIDMapRef.Get());
                await FillIDMapAsync(rootAsset, rootAssetIDMap, newIDMap);
                GetUnusedObjects(rootAssetIDMap, newIDMap, potentiallyUnusedObjects);
                newIDMap.Commit();

                assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(rootAsset, out var assetsMarkedAsDestroyed);
                if (assetsMarkedAsDestroyed != null)
                {
                    foreach (object asset in assetsMarkedAsDestroyed.ToArray())
                    {
                        if (IsObjectDestoryed(asset))
                        {
                            assetsMarkedAsDestroyed.Remove(asset);
                        }
                    }
                }


                if (rootAsset == sourceAsset)
                {
                    if (sourceAssetPartsMarkedAsDestroyed.Count > 0)
                    {
                        if (assetsMarkedAsDestroyed == null)
                        {
                            assetsMarkedAsDestroyed = new HashSet<object>();
                        }
                        else
                        {
                            assetsMarkedAsDestroyed = new HashSet<object>(assetsMarkedAsDestroyed);
                        }

                        foreach (object asset in sourceAssetPartsMarkedAsDestroyed)
                        {
                            if (!IsObjectDestoryed(asset))
                            {
                                assetsMarkedAsDestroyed.Add(asset);
                            }
                        }
                    }
                }

                newAssetMap.AddRootAsset(rootAsset, newIDMap, (sourceAssetIsVariant || assetsMarkedAsDestroyed != null && assetsMarkedAsDestroyed.Count > 0) ? assetsMarkedAsDestroyed : null);

                //newAssetMap.AddRootAsset(rootAsset, newIDMap, sourceAssetIsVariant ? assetsMarkedAsDestroyed : null);

                if (!assetMap.TryGetRootInstancesByRootAsset(rootAsset, out var rootInstances))
                {
                    continue;
                }

                foreach (var rootInstance in rootInstances)
                {
                    if (rootInstance == sourceInstance)
                    {
                        continue;
                    }

                    if (!assetMap.TryGetAssetToInstanceMapByRootInstance(rootInstance, out var assetToInstanceMap))
                    {
                        throw new KeyNotFoundException($"instance to asset map for {rootInstance} not found");
                    }

                    // when strict == true and you apply changes to the root asset instead of an instance,
                    // CopyObjectAsync must perform a "strict" copy and remove all children and components
                    // from instances that are not in that asset

                    var newAssetToInstanceMap = await CopyObjectAsync(rootAsset, assetToInstanceMap, assetsMarkedAsDestroyed, newAssetMap, strict);
                    newAssetMap.AddRootInstance(rootInstance, rootAsset, newAssetToInstanceMap);

                    bool waitForEndOfFrame = false;
                    // AddRemoveComponentAsyncTest
                    foreach (var kvp in assetToInstanceMap)
                    {
                        if (IsObjectDestoryed(kvp.Key) && !IsObjectDestoryed(kvp.Value))
                        {
                            DestroyObject(kvp.Value);
                            waitForEndOfFrame = true;
                        }
                    }

                    if (waitForEndOfFrame)
                    {
                        await Task.Yield();
                        waitForEndOfFrame = false;
                    }

                    foreach (var instance in newAssetToInstanceMap.Values)
                    {
                        if (assetMap.IsDirty(instance))
                        {
                            newAssetMap.SetDirty(instance);
                        }
                    }
                }
            }

            var objectsToRemove = new List<object>();
            var newIDMaps = new HashSet<IIDMap<TID>>();
            for (int i = 0; i < deps.Length; ++i)
            {
                if (!assetMap.IsRootAsset(deps[i]))
                {
                    continue;
                }

                object rootAsset = deps[i];

                if (!assetMap.TryGetIDMapByRootAsset(rootAsset, out var rootAssetIDMap))
                {
                    throw new KeyNotFoundException($"rootAsset {rootAsset}");
                }
                if (!newAssetMap.TryGetIDMapByRootAsset(rootAsset, out var newAssetIDMap))
                {
                    throw new KeyNotFoundException($"rootAsset {rootAsset}");
                }

                newAssetIDMap.ParentMap = rootAssetIDMap.ParentMap;

                if (assetMap.TryGetRootInstancesByRootAsset(rootAsset, out var rootInstances))
                {
                    foreach (var rootInstance in rootInstances.ToArray())
                    {
                        if (!objectsToRemove.Contains(rootAsset))
                        {
                            objectsToRemove.Add(rootInstance);
                        }
                    }
                }

                if (assetMap.IsRootInstance(rootAsset) && !newAssetMap.IsRootInstance(rootAsset))
                {
                    if (!assetMap.TryGetRootAssetByInstance(rootAsset, out object rootAssetOfAsset))
                    {
                        throw new KeyNotFoundException($"root asset of asset {rootAsset} not found");
                    }

                    if (!assetMap.TryGetInstanceToAssetMapByRootInstance(rootAsset, out var instanceAssetMap))
                    {
                        throw new KeyNotFoundException($"asset to instance map for {rootAsset} not found");
                    }

                    var newAssetToInstanceMap = new Dictionary<object, object>();

                    static void CreateAssetToInstanceMap(object instance, (IReadOnlyDictionary<object, object>, Dictionary<object, object>) args)
                    {
                        var (instanceAssetMap, newAssetToInstanceMap) = args;

                        if (instanceAssetMap.TryGetValue(instance, out var asset))
                        {
                            newAssetToInstanceMap.Add(asset, instance);
                        }

                    }

                    Enumerate(rootAsset, (instanceAssetMap, newAssetToInstanceMap), CreateAssetToInstanceMap);
                    newAssetMap.AddRootInstance(rootAsset, rootAssetOfAsset, newAssetToInstanceMap);
                }

                if (!objectsToRemove.Contains(rootAsset))
                {
                    objectsToRemove.Add(rootAsset);
                }

                newIDMaps.Add(newAssetIDMap);
            }


            foreach (object objToRemove in objectsToRemove)
            {
                if (assetMap.TryGetIDMapByRootAsset(objToRemove, out IIDMap<TID> idMap))
                {
                    assetMap.Remove(objToRemove);
                    m_deps.ReleaseIDMap(idMap);
                }
                else
                {
                    assetMap.Remove(objToRemove);
                }
            }

            foreach (var idMap in newIDMaps)
            {
                idMap.Commit();
            }

            newAssetMap.ParentMap = assetMap;
            newAssetMap.Commit();

            return potentiallyUnusedObjects;
        }

        public bool HasChanges(object obj)
        {
            if (!IsProjectLoaded)
            {
                return false;
            }

            bool isInstance = m_assetMap.IsInstance(obj);
            if (!isInstance)
            {
                return false;
            }

            object rootInstance = obj;
            if (!m_assetMap.IsRootInstance(rootInstance))
            {
                if (!TryGetInstanceRoot(rootInstance, out rootInstance))
                {
                    //Debug.LogWarning("Failed to get instance root");
                    rootInstance = obj;
                }
            }

            HashSet<object> instancesMarkedAsDestroyed;
            m_assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(rootInstance, out instancesMarkedAsDestroyed);

            HashSet<object> assetsMarkedAsDestroyed = null;
            if (m_assetMap.TryGetAssetByInstance(rootInstance, out var rootAsset))
            {
                m_assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(rootAsset, out assetsMarkedAsDestroyed);
            }

            if (m_assetMap.TryGetAssetToInstanceMapByRootInstance(obj, out var assetToInstanceMap))
            {
                foreach (var kvp in assetToInstanceMap)
                {
                    object a = kvp.Key;
                    object i = kvp.Value;

                    bool isAssetDestroyed = IsObjectDestoryed(a) || assetsMarkedAsDestroyed != null && assetsMarkedAsDestroyed.Contains(a);
                    bool isInstanceDestroyed = IsObjectDestoryed(i) || instancesMarkedAsDestroyed != null && instancesMarkedAsDestroyed.Contains(i);

                    if (isAssetDestroyed != isInstanceDestroyed)
                    {
                        return true;
                    }

                    if (!isAssetDestroyed)
                    {
                        if (IsObjectHidden(a) != IsObjectHidden(i))
                        {
                            return true;
                        }
                    }
                }

                using var enumeratorRef = m_deps.AcquireEnumeratorRef(obj);
                var enumerator = enumeratorRef.Get();
                var typeMap = m_deps.TypeMap;

                while (MoveNext(enumerator, typeMap))
                {
                    object current = enumerator.Current;

                    if (!HasInstantiableType(current))
                    {
                        continue;
                    }

                    if (!IsInHierarchy(enumerator, current))
                    {
                        continue;
                    }

                    if (!TryGetAssetByInstance(current, out _))
                    {
                        return true;
                    }

                    if (m_assetMap.IsDirty(current))
                    {
                        return true;
                    }

                    object rootRepresentation = GetObjectRootRepresentation(current);
                    if (obj != rootRepresentation && rootRepresentation != null && IsInstanceRoot(rootRepresentation) && this.IsAddedObject(rootRepresentation))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CanApplyChanges(object obj, bool afterApplyingChangesToRoot)
        {
            bool isRootInstance = m_assetMap.IsRootInstance(obj);
            bool isChildInstance = m_assetMap.IsInstance(obj) && !isRootInstance;
            bool isRootAsset = m_assetMap.IsRootAsset(obj);

            if (!isRootInstance && !isChildInstance && !isRootAsset)
            {
                return false;
            }

            if (IsExternalAsset(obj))
            {
                return false;
            }

            if (this.IsExternalAssetInstance(obj))
            {
                return false;
            }

            if (isChildInstance)
            {
                object childInstance = obj;
                object asset = this.GetAssetByInstance(childInstance);
                if (asset == null || !m_assetMap.IsRootInstance(asset))
                {
                    return false;
                }

                if (IsExternalAsset(asset))
                {
                    return false;
                }

                if (this.IsExternalAssetInstance(asset))
                {
                    return false;
                }

                if (!afterApplyingChangesToRoot)
                {
                    if (!AreEqual(childInstance, asset, CreateObjectMap(childInstance, asset).Map))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public async Task<TID[]> ApplyChangesAsync(object obj)
        {
            using (await LockAsync())
            {
                return await ApplyChangesAsyncImpl(obj, strict: false);
            }
        }

        protected async Task<TID[]> ApplyChangesAsyncImpl(object obj, bool strict)
        {
            bool isRootInstance = m_assetMap.IsRootInstance(obj);
            bool isChildInstance = m_assetMap.IsInstance(obj) && !isRootInstance;
            bool isRootAsset = m_assetMap.IsRootAsset(obj);

            if (!isRootInstance && !isChildInstance && !isRootAsset)
            {
                throw new ArgumentException("object is not a root asset or instance", "obj");
            }

            //if (IsExternalAsset(obj))
            //{
            //    Debug.LogWarning("Can't apply changes to external asset");
            //    return new TID[0];
            //}

            if (this.IsExternalAssetInstance(obj))
            {
                throw new ArgumentException("Can't apply changes to external asset", "obj");
            }

            if (!HasInstantiableAssetType(obj))
            {
                return new TID[] { this.GetAssetID(obj) };
            }

            if (isChildInstance)
            {
                object childInstance = obj;
                object childAsset = this.GetAssetByInstance(childInstance);
                if (childAsset == null || !m_assetMap.IsRootInstance(childAsset))
                {
                    throw new ArgumentException("object is not an instance of the root asset", "obj");
                }

                if (IsExternalAsset(childAsset))
                {
                    throw new ArgumentException("Can't apply changes to external asset", "obj");
                }

                if (this.IsExternalAssetInstance(childAsset))
                {
                    throw new ArgumentException("Can't apply changes to external asset", "obj");
                }

                /*
                HashSet<object> markedAsDestroyed = null;
                if(m_assetMap.TryGetRootAssetByAsset(childAsset, out var rootAsset))
                {
                    m_assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(rootAsset, out markedAsDestroyed);
                }
                */

                var childInstanceToRootAsset = CreateObjectMap(childInstance, childAsset).Map;
                if (!AreEqual(childInstance, childAsset, childInstanceToRootAsset))
                {
                    throw new InvalidOperationException("Apply the changes to the root instance first");
                }

                // copy changes from childInstance to rootAsset
                await CopyObjectAsync(childInstance, childInstanceToRootAsset, null, strict: false);

                obj = childAsset;
                isRootInstance = true;
            }

            var deps = isRootInstance ?
                m_assetMap.GetDependencies(this.GetAssetByInstance(obj)) :
                m_assetMap.GetDependencies(obj).ToArray();

            var unusedObjects = await PropagateChangesAsync(obj, m_assetMap, strict);
            foreach (var unusedObject in unusedObjects)
            {
                if (!m_idMap.TryGetID(unusedObject, out _) && !IsExternalAssetRoot(unusedObject))
                {
                    // Sometimes, after calling DestroyObject, you may get the message
                    // "Destroying Assets is not Permitted to Avoid Data Loss" in the console.

                    // The workarounds are following:
                    // 1. Do nothing
                    // 2. Register these assets as external using RegisterExternalAsset method
                    // 3. Instantiate asset before passing it to CreateAssetAsync method
                    // (It might be sharedMaterial of MeshRenderer or sharedMesh of Mesh filter)

                    DestroyObject(unusedObject);
                }
            }

            var ids = new List<TID>();
            foreach (object dep in deps)
            {
                if (m_assetMap.IsRootAsset(dep) && m_idMap.TryGetID(dep, out var id))
                {
                    ids.Add(id);
                }
            }
            return ids.ToArray();
        }

        public async Task RenameAssetAsync(TID assetID, string name)
        {
            using (await LockAsync())
            {
                await RenameAssetAsyncImpl(assetID, name);
            }
        }

        protected async Task RenameAssetAsyncImpl(TID assetID, string name)
        {
            if (!TryGetMeta(assetID, out TMeta meta))
            {
                throw new ArgumentException($"Metadata {assetID} not found");
            }

            var dataLayer = m_deps.DataLayer;
            var serializer = m_deps.Serializer;
            var metaFileID = GetMetaFileID(in meta);

            meta.Name = name;
            SetMeta(meta.ID, meta);
            await SerializeMetaAsync(metaFileID, dataLayer, serializer, meta);

            object asset = this.GetAsset(assetID);
            if (asset != null)
            {
                SetObjectName(asset, meta.Name);
            }
        }

        public async Task MoveAssetAsync(TID assetID, TFID newFileID, TFID newDataFileID, TFID newThumbnailID, TID newParentID)
        {
            using (await LockAsync())
            {
                await MoveAssetAsyncImpl(assetID, newFileID, newDataFileID, newThumbnailID, newParentID);
            }
        }

        protected virtual async Task MoveAssetAsyncImpl(TID assetID, TFID newFileID, TFID newDataFileID, TFID newThumbnailID, TID newParentID)
        {
            if (!TryGetMeta(assetID, out TMeta meta))
            {
                throw new ArgumentException($"Metadata {assetID} not found");
            }

            if (!TryGetMeta(newParentID, out TMeta newParentMeta))
            {
                throw new ArgumentException($"Metadata {newParentID} not found");
            }

            newFileID = NormalizeFileID(newFileID);
            newDataFileID = NormalizeFileID(newDataFileID);
            newThumbnailID = NormalizeFileID(newThumbnailID);

            var dataLayer = m_deps.DataLayer;
            var serializer = m_deps.Serializer;
            var metaFileID = GetMetaFileID(in meta);
            var newParentFileID = GetMetaFileID(in newParentMeta);

            await dataLayer.MoveAsync(metaFileID, newFileID);
            await dataLayer.MoveAsync(GetDataFileID(in meta), newDataFileID);
            if (!Equals(newThumbnailID, default))
            {
                var thumbnailFileID = GetThumbnailFileID(in meta);
                bool exists = await dataLayer.ExistsAsync(thumbnailFileID);
                if (exists)
                {
                    await dataLayer.MoveAsync(thumbnailFileID, newThumbnailID);
                }
            }

            if (m_fileIDToID.TryGetValue(metaFileID, out var id))
            {
                m_fileIDToID.Remove(metaFileID);
                m_fileIDToID.Add(newFileID, id);
            }

            if (m_fileIDToParent.TryGetValue(metaFileID, out var parentID))
            {
                if (m_fileIDToChildren.TryGetValue(parentID, out var parentChildren))
                {
                    parentChildren.Remove(meta.ID);
                }

                if (m_fileIDToChildren.TryGetValue(newParentFileID, out var newParentChildren))
                {
                    newParentChildren.Add(meta.ID);
                }

                m_fileIDToParent.Remove(metaFileID);
                m_fileIDToParent.Add(newFileID, newParentFileID);
            }

            SetMetaFileID(ref meta, newFileID);
            SetDataFileID(ref meta, newDataFileID);
            SetThumbnailFileID(ref meta, newThumbnailID);
            SetMeta(meta.ID, meta);

            if (TryGetAsset(meta.ID, out object asset))
            {
                SetObjectName(asset, meta.Name);
            }

            await SerializeMetaAsync(newFileID, dataLayer, serializer, meta);
        }

        public async Task UnloadAssetAsync(TID assetID, bool destroy)
        {
            using (await LockAsync())
            {
                await UnloadAssetAsyncImpl(assetID, destroy);
            }
        }

        protected Task UnloadAssetAsyncImpl(TID assetID, bool destroy)
        {
            bool isExternalAsset = m_assetIDToExternalAsset.TryGetValue(assetID, out var externalAsset);
            TMeta externalAssetMeta = default;
            if (isExternalAsset)
            {
                if (!TryGetMeta(assetID, out externalAssetMeta))
                {
                    // external asset is not mapped using ImportExternalAssetAsync method

                    Debug.LogWarning($"The external asset {assetID} cannot be unloaded");
                    return Task.CompletedTask;
                }
            }

            if (!TryGetObject(assetID, out var rootAsset))
            {
                return Task.CompletedTask;
            }

            if (!m_assetMap.TryGetIDMapByRootAsset(rootAsset, out var idMap))
            {
                throw new ArgumentException($"Asset already unloaded", "asset");
            }

            if (m_assetMap.TryGetRootInstancesByRootAsset(rootAsset, out var rootInstances))
            {
                foreach (object rootInstance in rootInstances.ToArray())
                {
                    if (m_assetMap.TryGetRootAssetIDByAsset(rootInstance, out TID id))
                    {
                        throw new InvalidOperationException($"Instance is of this asset is used by another asset {id}");
                    }
                }

                foreach (object rootInstance in rootInstances.ToArray())
                {
                    if (!m_assetMap.IsRootInstance(rootInstance))
                    {
                        continue;
                    }

                    if (destroy)
                    {
                        ReleaseImpl(rootInstance);
                    }
                    else
                    {
                        DetachImpl(rootInstance, completely: true);
                    }
                }
            }

            DetachImpl(rootAsset, completely: true); // instances of other assets
            foreach (object asset in idMap.ObjectToID.Keys)
            {
                if (destroy || HasInstantiableType(asset))
                {
                    if (IsExternalAsset(asset))
                    {
                        continue;
                    }

                    DestroyObject(asset);
                }
            }

            m_assetMap.Remove(rootAsset);

            idMap.Rollback();
            m_deps.ReleaseIDMap(idMap);

            if (isExternalAsset)
            {
                try
                {
                    UnregisterExternalAsset(externalAsset);

                    if (destroy)
                    {
                        if (m_loaderIDToExternalLoader.TryGetValue(externalAssetMeta.LoaderID, out var loader))
                        {
                            loader.Release(externalAsset);
                        }
                        else
                        {
                            Debug.LogWarning($"Loader with ID {externalAssetMeta.LoaderID} not found");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return Task.CompletedTask;
        }

        public async Task UnloadAllAssetsAsync(bool destroy)
        {
            using (await LockAsync())
            {
                await UnloadAllAssetsAsyncImpl(destroy);
            }
        }

        protected async Task UnloadAllAssetsAsyncImpl(bool destroy)
        {
            var rootInstances = m_assetMap.RootInstances.ToArray();
            var rootAssets = m_assetMap.RootAssets.ToArray();
            foreach (object rootInstance in rootInstances)
            {
                if (!m_assetMap.IsRootInstance(rootInstance))
                {
                    continue;
                }

                if (destroy)
                {
                    ReleaseImpl(rootInstance);
                }
                else
                {
                    DetachImpl(rootInstance, completely: true);
                }
            }

            foreach (object rootAsset in rootAssets)
            {
                if (m_idMap.TryGetID(rootAsset, out var id))
                {
                    await UnloadAssetAsyncImpl(id, destroy);
                }
            }
        }

        public async Task DeleteAssetAsync(TID assetID)
        {
            using (await LockAsync())
            {
                await DeleteAssetAsyncImpl(assetID);
            }
        }

        protected async Task DeleteAssetAsyncImpl(TID assetID)
        {
            const bool destroy = true;

            if (!TryGetMeta(assetID, out TMeta meta))
            {
                return;
            }

            if (TryGetObject(assetID, out var asset))
            {
                if (m_assetMap.TryGetRootInstancesByRootAsset(asset, out var rootInstances))
                {
                    foreach (object rootInstance in rootInstances.ToArray())
                    {
                        if (m_assetMap.TryGetRootAssetIDByAsset(rootInstance, out _))
                        {
                            DetachImpl(rootInstance, completely: true);
                        }
                    }
                }
            }

            await UnloadAssetAsyncImpl(assetID, destroy);

            var dataLayer = m_deps.DataLayer;
            await dataLayer.DeleteAsync(GetMetaFileID(in meta));
            await dataLayer.DeleteAsync(GetDataFileID(in meta));
            await dataLayer.DeleteAsync(GetThumbnailFileID(in meta));

            HashSet<TMeta> updateMetaHs = RemoveInboundDependency(assetID, meta);
            await SerializeDepsMetaAsync(updateMetaHs);
            RemoveMeta(assetID);
        }

        private HashSet<TMeta> RemoveInboundDependency(TID assetID, TMeta meta)
        {
            HashSet<TMeta> updateMetaHs = null;

            if (meta.OutboundDependencies != null)
            {
                foreach (TID dependencyID in meta.OutboundDependencies)
                {
                    if (!TryGetMeta(dependencyID, out TMeta depMeta))
                    {
                        continue;
                    }

                    if (depMeta.InboundDependencies != null)
                    {
                        depMeta.InboundDependencies.Remove(assetID);
                    }

                    SetMeta(dependencyID, in depMeta);

                    if (updateMetaHs == null)
                    {
                        updateMetaHs = new HashSet<TMeta>();
                    }

                    updateMetaHs.Add(depMeta);
                }
            }

            return updateMetaHs;
        }

        public async Task UnloadThumbnailAsync(TID assetID)
        {
            using (await LockAsync())
            {
                await UploadThumbnailAsyncImpl(assetID);
            }
        }

        protected Task UploadThumbnailAsyncImpl(TID assetID)
        {
            m_idToThumbnail.Remove(assetID);
            return Task.CompletedTask;
        }

        public bool IsAssetRoot(object obj)
        {
            return m_assetMap != null && m_assetMap.IsRootAsset(obj);
        }

        public bool IsAsset(object obj)
        {
            return m_assetMap != null && m_assetMap.IsAsset(obj);
        }

        public bool TryGetAssetRoot(object asset, out object assetRoot)
        {
            if (m_assetMap == null)
            {
                assetRoot = null;
                return false;
            }

            return m_assetMap.TryGetRootAssetByAsset(asset, out assetRoot);
        }

        public bool TryGetAsset(TID assetID, out object asset)
        {
            return m_idMap.TryGetObject(assetID, out asset);
        }

        public bool TryGetAssetID(object asset, out TID assetID)
        {
            return m_idMap.TryGetID(asset, out assetID);
        }

        public bool TryGetAssetByInstance(object instance, out object asset)
        {
            if (m_assetMap == null)
            {
                asset = null;
                return false;
            }

            return m_assetMap.TryGetAssetByInstance(instance, out asset);
        }

        public bool TryGetInstances(TID assetID, out IReadOnlyCollection<object> instances)
        {
            if (!m_idMap.TryGetObject(assetID, out object asset))
            {
                instances = m_emptyObjects;
                return false;
            }

            if (!m_assetMap.TryGetRootInstancesByRootAsset(asset, out instances))
            {
                instances = m_emptyObjects;
                return false;
            }

            return true;
        }

        public bool TryGetDestroyed(TID assetID, out IReadOnlyCollection<object> destroyed)
        {
            if (!TryGetAsset(assetID, out object asset) || !m_assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(asset, out var destroyedHs))
            {
                destroyed = m_emptyObjects;
                return false;
            }

            destroyed = destroyedHs;
            return true;
        }

        public bool CanRevertChanges(object asset)
        {
            if (!m_assetMap.TryGetRootAssetByAsset(asset, out object rootAsset))
            {
                asset = this.GetAssetByInstance(asset);
                if (asset == null)
                {
                    return false;
                }

                if (!m_assetMap.TryGetRootAssetByAsset(asset, out rootAsset))
                {
                    return false;
                }
            }

            if (m_assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(rootAsset, out var assetsMarkedAsDestroyed))
            {
                if (asset != rootAsset)
                {
                    if (assetsMarkedAsDestroyed.Contains(asset))
                    {
                        return true;
                    }
                }
                else
                {
                    if (assetsMarkedAsDestroyed.Count > 0)
                    {
                        return true;
                    }
                }
            }

            if (!m_assetMap.TryGetInstanceToAssetMapByRootInstance(rootAsset, out var variantToBaseAssetMap))
            {
                variantToBaseAssetMap = null;
            }

            using var enumeratorRef = m_deps.AcquireEnumeratorRef(asset);
            var enumerator = enumeratorRef.Get();
            var typeMap = m_deps.TypeMap;
            while (MoveNext(enumerator, typeMap))
            {
                object current = enumerator.Current;
                if (!HasInstantiableType(current))
                {
                    continue;
                }

                if (!IsInHierarchy(enumerator, current))
                {
                    continue;
                }

                if (m_assetMap.IsDirty(current))
                {
                    return true;
                }

                if (variantToBaseAssetMap != null && !variantToBaseAssetMap.ContainsKey(current))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<TID[]> RevertChangesAsync(object asset)
        {
            using (await LockAsync())
            {
                return await RevertChangesAsyncImpl(asset);
            }
        }

        protected async Task<TID[]> RevertChangesAsyncImpl(object asset)
        {
            TID[] affectedAssetIDs = m_emptyIDs;

            if (!m_assetMap.TryGetRootAssetByAsset(asset, out object rootAsset))
            {
                asset = this.GetAssetByInstance(asset);
                if (asset == null)
                {
                    new ArgumentException($"{asset} is not a asset", "asset");
                }

                if (!m_assetMap.TryGetRootAssetByAsset(asset, out rootAsset))
                {
                    new ArgumentException($"{asset} is not a asset", "asset");
                }
            }

            if (!m_assetMap.TryGetInstanceToAssetMapByRootInstance(rootAsset, out var variantToBaseAssetMap))
            {
                variantToBaseAssetMap = null;
            }

            using var enumeratorRef = m_deps.AcquireEnumeratorRef(asset);
            var enumerator = enumeratorRef.Get();

            bool applyChanges = false;
            bool applyBaseChanges = false;
            var typeMap = m_deps.TypeMap;
            while (MoveNext(enumerator, typeMap))
            {
                object current = enumerator.Current;
                if (!HasInstantiableType(current))
                {
                    continue;
                }

                if (!IsInHierarchy(enumerator, current))
                {
                    continue;
                }

                if (m_assetMap.IsDirty(current))
                {
                    m_assetMap.ClearDirty(current);
                    applyBaseChanges = true;
                }

                if (variantToBaseAssetMap != null && !variantToBaseAssetMap.ContainsKey(current))
                {
                    // asset variant has component or child game object which is not present in base asset

                    applyBaseChanges = true;
                }
            }

            if (m_assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(rootAsset, out var assetsMarkedAsDestroyed))
            {
                if (asset == rootAsset)
                {
                    if (assetsMarkedAsDestroyed.Count > 0)
                    {
                        assetsMarkedAsDestroyed.Clear();
                        applyChanges = true;
                    }
                }
                else
                {
                    if (assetsMarkedAsDestroyed.Remove(asset))
                    {
                        applyChanges = true;
                    }
                }
            }

            if (applyBaseChanges)
            {
                if (TryGetAssetByInstance(rootAsset, out var baseAsset))
                {
                    affectedAssetIDs = await ApplyChangesAsyncImpl(baseAsset, strict: true);
                }
                else
                {
                    affectedAssetIDs = await ApplyChangesAsyncImpl(rootAsset, strict: true);
                }
            }
            else if (applyChanges)
            {
                affectedAssetIDs = await ApplyChangesAsyncImpl(rootAsset, strict: true);
            }

            return affectedAssetIDs;
        }

        public bool IsInstanceRoot(object obj)
        {
            return m_assetMap != null && m_assetMap.IsRootInstance(obj);
        }

        public bool IsInstance(object obj)
        {
            return m_assetMap != null && m_assetMap.IsInstance(obj);
        }

        private bool TryGetInstanceRoot(object obj, IAssetMap<TID> assetMap, out object instanceRoot)
        {
            object parent = GetObjectParent(obj);
            while (parent != null && !assetMap.IsRootInstance(GetObjectRootRepresentation(parent)))
            {
                parent = GetObjectParent(parent);
            }
            instanceRoot = GetObjectRootRepresentation(parent);
            return instanceRoot != null;
        }

        public bool TryGetInstanceRoot(object obj, out object instanceRoot)
        {
            instanceRoot = null;

            return m_assetMap != null && TryGetInstanceRoot(obj, m_assetMap, out instanceRoot);
        }

        private void TrimInstance(HashSet<object> assetsMarkedAsDestroyed, Dictionary<object, object> assetToInstance)
        {
            foreach (object asset in assetToInstance.Keys.ToArray())
            {
                if (assetsMarkedAsDestroyed.Contains(asset))
                {
                    DestroyObject(assetToInstance[asset], immediate: true);
                    assetToInstance.Remove(asset);
                }
            }
        }

        private void TrimInstance(object rootAsset, Dictionary<object, object> assetToInstance)
        {
            if (m_assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(rootAsset, out var assetsMarkedAsDestroyed))
            {
                TrimInstance(assetsMarkedAsDestroyed, assetToInstance);
            }
        }

        private void CopyDirtyFlags(IReadOnlyDictionary<object, object> assetToInstance)
        {
            foreach (var kvp in assetToInstance)
            {
                object asset = kvp.Key;
                object instance = kvp.Value;
                if (m_assetMap.IsDirty(asset))
                {
                    m_assetMap.SetDirty(instance);
                }
            }
        }

        public bool CanInstantiateAsset(TID assetID)
        {
            if (!TryGetAssetType(assetID, out Type type))
            {
                return false;
            }

            return IsInstantiableAssetType(type);
        }

        public async Task<object> InstantiateAssetAsync(TID assetID, object parent, bool detachInstance)
        {
            using (await LockAsync())
            {
                object asset = this.GetAsset(assetID);
                if (asset == null)
                {
                    await LoadAssetAsyncImpl(assetID);
                }

                return await InstantiateAssetAsyncImpl(assetID, parent, detachInstance, copyDirtyFlags: false);
            }
        }

        protected Task<object> InstantiateAssetAsyncImpl(TID assetID, object parent, bool detachInstance, bool copyDirtyFlags)
        {
            object asset = this.GetAsset(assetID);

            // Instance should be disabled until enumeration complete.
            // Otherwise there is risk that random scripts will modify tree structure which will cause errors during creation of linkmap

            if (asset == null)
            {
                Debug.LogWarning($"asset {assetID} is not loaded");
                return Task.FromResult<object>(null);
            }

            if (!m_assetMap.IsRootAsset(asset))
            {
                throw new InvalidOperationException("Can't instantiate non-root asset");
            }

            if (!HasInstantiableAssetType(asset))
            {
                throw new InvalidOperationException($"Can't instantiate asset {assetID} {asset.GetType().FullName}");
            }

            object tempRoot = m_deps.AssetsRoot;
            object instance = InstantiateObject(asset, tempRoot); // instance should be in deactivated state
            if (detachInstance)
            {
                if (m_assetMap.TryGetAssetsMarkedAsDestroyedByRootAsset(asset, out var assetsMarkedAsDestroyed) && assetsMarkedAsDestroyed.Count > 0)
                {
                    var assetToInstance = CreateObjectMap(asset, instance).Map;
                    TrimInstance(asset, assetToInstance);
                }
            }
            else
            {
                var assetToInstance = CreateObjectMap(asset, instance).Map;
                TrimInstance(asset, assetToInstance);
                m_assetMap.AddRootInstance(instance, asset, assetToInstance);

                if (copyDirtyFlags)
                {
                    CopyDirtyFlags(assetToInstance);
                }
            }

            SetObjectParent(instance, parent);
            return Task.FromResult(instance);
        }

        public async Task<object> InstantiateAsync(object obj, object parent, bool detachInstance)
        {
            using (await LockAsync())
            {
                return await InstantiateAsyncImpl(obj, parent, detachInstance);
            }
        }

        protected async Task<object> InstantiateAsyncImpl(object obj, object parent, bool detachInstance)
        {
            bool isRootInstance = IsInstanceRoot(obj);
            bool isRootAsset = IsAssetRoot(obj);

            if (!isRootInstance && !isRootAsset || detachInstance)
            {
                return InstantiateObject(obj, parent);
            }

            if (isRootAsset)
            {
                return await InstantiateAssetAsyncImpl(this.GetAssetID(obj), parent, detachInstance, copyDirtyFlags: true);
            }

            object tempRoot = m_deps.AssetsRoot;
            object instance = InstantiateObject(obj, tempRoot);

            if (!m_assetMap.TryGetAssetToInstanceMapByRootInstance(obj, out var assetToInstanceMap))
            {
                throw new KeyNotFoundException($"Can't find asset to instance map by key {obj}");
            }

            var instanceToNewInstanceMap = CreateObjectMap(obj, instance).Map;
            var assetToNewInstanceMap = Remap(assetToInstanceMap, instanceToNewInstanceMap);
            var asset = this.GetAssetByInstance(obj);

            m_assetMap.AddRootInstance(instance, asset, assetToNewInstanceMap);
            CopyDirtyFlags(instanceToNewInstanceMap);

            SetObjectParent(instance, parent);
            return instance;
        }

        public async Task ReleaseAsync(object instance)
        {
            using (await LockAsync())
            {
                ReleaseImpl(instance);
            }
        }

        protected void ReleaseImpl(object instance)
        {
            DetachImpl(instance, completely: true, unconditionally: true, m_assetMap);
            if (!IsExternalAssetRoot(instance))
            {
                DestroyObject(instance);
            }
        }

        public async Task SetDontDestroyFlagAsync(object obj)
        {
            using (await LockAsync())
            {
                m_dontDestroyObjects.Add(obj);
            }
        }

        public async Task ClearDontDestroyFlagAsync(object obj)
        {
            using (await LockAsync())
            {
                ClearDontDestroyFlag(obj);
            }
        }

        public async Task ClearDontDestroyFlagsAsync()
        {
            using (await LockAsync())
            {
                m_dontDestroyObjects.Clear();
            }
        }

        protected bool HasDontDestroyFlag(object obj)
        {
            return m_dontDestroyObjects.Contains(obj);
        }

        [Obsolete]
        protected void CrearDontDestroyFlag(object obj)
        {
            m_dontDestroyObjects.Remove(obj);
        }

        protected void ClearDontDestroyFlag(object obj)
        {
            m_dontDestroyObjects.Remove(obj);
        }

        private bool CanCloneSubAssets(object instance)
        {
            bool isRootInstance = m_assetMap.IsRootInstance(instance);
            bool isChildInstance = m_assetMap.IsInstance(instance) && !isRootInstance;

            if (!isRootInstance && !isChildInstance)
            {
                return false;
            }

            var rootAssetMap = m_assetMap;
            var rootInstance = instance;
            if (!rootAssetMap.TryGetAssetByInstance(rootInstance, out object rootAsset) || !rootAssetMap.IsRootAsset(rootAsset))
            {
                return false;
            }

            if (!rootAssetMap.TryGetIDMapByRootAsset(rootAsset, out var _))
            {
                return false;
            }

            return true;
        }

        private async Task CloneSubAssetsImpl(object instance, IIDMap<TID> parentIDMap, TID assetID)
        {
            using var serializerRef = m_deps.AcquireSerializerRef();
            var serializer = serializerRef.Get();

            using var instanceIDMapRef = m_deps.AcquireIDMapRef(parentIDMap);
            var instanceIDMap = instanceIDMapRef.Get();

            await SerializeObjectAsync(instance, serializer, instanceIDMap, assetID);
            while (serializer.CopyToDeserializationQueue()) ;

            foreach (var kvp in instanceIDMap.ObjectToID.ToArray())
            {
                if (!HasInstantiableType(kvp.Key))
                {
                    instanceIDMap.Remove(kvp.Value);
                }
            }

            await DeserializeObjectAsync(serializer, instanceIDMap);
        }

        private async Task CloneSubAssetsAsyncImpl(object instance, object rootAsset, IAssetMap<TID> rootAssetMap)
        {
            if (!rootAssetMap.TryGetIDMapByRootAsset(rootAsset, out var idMap))
            {
                throw new KeyNotFoundException($"Can't find id map for asset {rootAsset}");
            }

            var assetID = this.GetAssetID(rootAsset);

            idMap.Rollback();
            try
            {
                await CloneSubAssetsImpl(instance, m_idMap, assetID);
            }
            finally
            {
                idMap.Commit();
            }
        }

        private Task CloneAllSubAssetsAsyncImpl(object instance)
        {
            return CloneSubAssetsImpl(instance, null, default);
        }

        public bool CanDetach(object instance, bool completely, bool cloneSubAssets)
        {
            var rootAssetMap = m_assetMap;
            if (rootAssetMap.IsRootInstance(instance))
            {
                if (cloneSubAssets && !completely)
                {
                    return CanCloneSubAssets(instance);
                }

                return true;
            }

            if (rootAssetMap.IsInstance(instance))
            {
                object asset = this.GetAssetByInstance(instance);
                bool canDetach = asset != null && rootAssetMap.IsRootInstance(asset);
                if (canDetach && cloneSubAssets && !completely)
                {
                    canDetach = CanCloneSubAssets(instance);
                }

                return canDetach;
            }

            return false;
        }

        public async Task DetachAsync(object instance, bool completely, bool cloneSubAssets)
        {
            using (await LockAsync())
            {
                if (completely)
                {
                    DetachImpl(instance, true);
                    if (cloneSubAssets)
                    {
                        await CloneAllSubAssetsAsyncImpl(instance);
                    }
                }
                else
                {
                    if (cloneSubAssets && CanCloneSubAssets(instance))
                    {
                        var assetMap = m_assetMap;
                        if (!assetMap.TryGetAssetByInstance(instance, out object asset) || !assetMap.IsRootAsset(asset))
                        {
                            throw new KeyNotFoundException($"Can't find root asset by instance {instance}");
                        }

                        DetachImpl(instance, false);
                        await CloneSubAssetsAsyncImpl(instance, asset, assetMap);
                    }
                    else
                    {
                        DetachImpl(instance, false);
                    }
                }
            }
        }

        private void DetachImpl(object instance, bool completely)
        {
            DetachImpl(instance, completely, unconditionally: false, m_assetMap);
        }

        private void DetachImpl(object instance, bool completely, bool unconditionally, IAssetMap<TID> rootAssetMap)
        {
            bool isRootInstance = m_assetMap.IsRootInstance(instance);
            bool isChildInstance = m_assetMap.IsInstance(instance) && !isRootInstance;

            if (!isRootInstance && !isChildInstance && !completely)
            {
                return;
            }

            if (isChildInstance)
            {
                object childInstance = instance;
                object rootAsset = this.GetAssetByInstance(childInstance);
                isRootInstance = m_assetMap.IsRootInstance(rootAsset);
                instance = rootAsset;
            }

            if ((!completely || !unconditionally) && !isRootInstance)
            {
                if (rootAssetMap.IsInstance(instance))
                {
                    throw new ArgumentException($"Unable to detach {instance}");
                }
            }

            if (completely)
            {
                static void RemoveInstance(object obj, IAssetMap<TID> assetMap)
                {
                    if (assetMap.IsRootInstance(obj))
                    {
                        assetMap.RemoveInstance(obj);
                    }
                }

                if (IsObjectDestoryed(instance))
                {
                    if (rootAssetMap.IsRootInstance(instance))
                    {
                        rootAssetMap.RemoveInstance(instance);
                    }
                }
                else
                {
                    using var enumRef = m_deps.AcquireEnumeratorRef(instance, obj => obj != null);
                    Enumerate(enumRef.Get(), rootAssetMap, RemoveInstance);
                }
            }
            else
            {
                if (!isRootInstance)
                {
                    return;
                }

                object rootInstance = instance;
                if (!rootAssetMap.TryGetAssetByInstance(rootInstance, out object rootAsset) || !rootAssetMap.IsRootAsset(rootAsset))
                {
                    throw new KeyNotFoundException($"Can't find root asset by instance {rootInstance}");
                }

                if (!rootAssetMap.TryGetAssetToInstanceMapByRootInstance(rootInstance, out var assetToInstanceMap))
                {
                    throw new KeyNotFoundException($"Can't find assetToInstanceMap by instance {rootInstance}");
                }

                using var assetMapRef = m_deps.AcquireAssetMapRef(rootAssetMap);
                var assetMap = assetMapRef.Get();

                MapInstances(rootAsset, assetToInstanceMap, detachSourceInstances: false, assetMap);

                foreach (var kvp in assetToInstanceMap)
                {
                    if (assetMap.IsDirty(kvp.Key))
                    {
                        assetMap.SetDirty(kvp.Value);
                    }
                }

                assetMap.Remove(rootInstance, waitForCommit: true);
                assetMap.Commit();
            }
        }

        public async Task SetDirtyAsync(object instance)
        {
            using (await LockAsync())
            {
                SetDirtyImpl(instance);
            }
        }

        protected void SetDirtyImpl(object instance)
        {
            if (IsInstanceRoot(instance))
            {
                if (m_assetMap.TryGetAssetToInstanceMapByRootInstance(instance, out var assetToInstance))
                {
                    foreach (var kvp in assetToInstance)
                    {
                        m_assetMap.SetDirty(kvp.Value);
                    }
                }
            }

            m_assetMap.SetDirty(instance);
        }

        public async Task ClearDirtyAsync(object instance)
        {
            using (await LockAsync())
            {
                ClearDirtyImpl(instance);
            }
        }

        protected void ClearDirtyImpl(object instance)
        {
            if (IsInstanceRoot(instance))
            {
                if (m_assetMap.TryGetAssetToInstanceMapByRootInstance(instance, out var assetToInstance))
                {
                    foreach (var kvp in assetToInstance)
                    {
                        m_assetMap.ClearDirty(kvp.Value);
                    }
                }
            }

            m_assetMap.ClearDirty(instance);
        }

        public bool IsDirty(object instance)
        {
            return IsDirty(instance, m_assetMap);
        }

        private bool IsDirty(object instance, IAssetMap<TID> assetMap)
        {
            return assetMap.IsDirty(instance);
        }

        private bool IsDirtyByDefault(object instance, IAssetMap<TID> assetMap)
        {
            if (assetMap.IsInstance(instance))
            {
                object rootRepresentation = GetObjectRootRepresentation(instance);
                if (rootRepresentation != null && assetMap.IsRootInstance(rootRepresentation))
                {
                    return GetObjectParent(instance) != null;
                }
            }

            return false;
        }

        protected virtual bool IsEqualToAsset(object instance, IAssetMap<TID> assetMap)
        {
            return true;
        }

        [Obsolete]
        protected virtual bool IsInstantiableObject(object obj)
        {
            return obj != null && IsInstantiableType(obj.GetType());
        }

        protected abstract bool IsInstantiableType(Type type);

        protected virtual bool HasInstantiableType(object obj)
        {
            return obj != null && IsInstantiableType(obj.GetType());
        }

        protected abstract bool IsInstantiableAssetType(Type type);

        protected virtual bool HasInstantiableAssetType(object obj)
        {
            return obj != null && IsInstantiableAssetType(obj.GetType());
        }

        protected abstract object InstantiateObject(object obj, object parent);

        protected abstract void SetObjectParent(object obj, object parent);

        protected abstract object GetObjectParent(object obj);

        protected virtual void SetObjectName(object obj, string name)
        {
        }

        protected virtual string GetObjectName(object obj)
        {
            return null;
        }

        protected virtual object GetObjectRootRepresentation(object obj, int mask = 0)
        {
            return obj;
        }

        protected virtual bool IsObjectDestoryed(object obj)
        {
            return obj == null || obj is NullObject;
        }

        protected virtual bool IsObjectHidden(object obj)
        {
            return false;
        }

        protected virtual void DestroyObject(object obj, bool immediate = false)
        {
            if (obj != null)
            {
                ClearDirtyImpl(obj);
            }

            if (obj != null)
            {
                ClearDontDestroyFlag(obj);
            }
        }

        public async Task RegisterExternalAssetLoaderAsync(string loaderID, IExternalAssetLoader loader)
        {
            using (await LockAsync())
            {
                RegisterExternalAssetLoaderImpl(loaderID, loader);
            }
        }

        protected void RegisterExternalAssetLoaderImpl(string loaderID, IExternalAssetLoader loader)
        {
            m_loaderIDToExternalLoader[loaderID] = new AssetLoaderAdapter<TID>(loader);
        }

        public async Task ClearExternalAssetLoadersAsync()
        {
            using (await LockAsync())
            {
                ClearExternalAssetLoadersImpl();
            }
        }

        protected void ClearExternalAssetLoadersImpl()
        {
            m_loaderIDToExternalLoader.Clear();
        }

        public bool TryGetExternalLoader(string externalLoaderID, out IExternalAssetLoader loader)
        {
            bool result = TryGetExternalLoader(externalLoaderID, out AssetLoaderAdapter<TID> adapter);
            loader = adapter;
            return result;
        }

        protected bool TryGetExternalLoader(string externalLoaderID, out AssetLoaderAdapter<TID> loader)
        {
            return m_loaderIDToExternalLoader.TryGetValue(externalLoaderID, out loader);
        }

        public async Task ImportExternalAssetAsync(string externalAssetKey, string externalLoaderID, TFID metaFileID, byte[] thumbnail = null, TFID thumbnailID = default, TFID dataFileID = default, TID parentID = default)
        {
            using (await LockAsync())
            {
                await ImportExternalAssetAsyncImpl(externalAssetKey, externalLoaderID, metaFileID, thumbnail, thumbnailID, dataFileID, parentID);
            }
        }

        protected async Task ImportExternalAssetAsyncImpl(string externalAssetKey, string externalLoaderID, TFID metaFileID, byte[] thumbnail = null, TFID thumbnailID = default, TFID dataFileID = default, TID parentID = default)
        {
            if (!TryGetExternalLoader(externalLoaderID, out AssetLoaderAdapter<TID> loader))
            {
                throw new ArgumentException($"loader {externalLoaderID} not found", "externalLoaderID");
            }

            var tempRoot = m_deps.AssetsRoot;
            var externalAsset = await loader.LoadAsync(externalAssetKey, tempRoot, null);
            await ImportExternalAssetAsyncImpl(m_idMap.NullID, externalAsset, externalAssetKey, externalLoaderID, metaFileID, thumbnail, thumbnailID, dataFileID, parentID);
        }

        public async Task ImportExternalAssetAsync(object externalAsset, string externalAssetKey, string externalLoaderID, TFID metaFileID, byte[] thumbnail = null, TFID thumbnailID = default, TFID dataFileID = default, TID parentID = default)
        {
            using (await LockAsync())
            {
                await ImportExternalAssetAsyncImpl(m_idMap.NullID, externalAsset, externalAssetKey, externalLoaderID, metaFileID, thumbnail, thumbnailID, dataFileID, parentID);
            }
        }

        public async Task ImportExternalAssetAsync(TID externalAssetID, object externalAsset, string externalAssetKey, string externalLoaderID, TFID metaFileID, byte[] thumbnail = null, TFID thumbnailID = default, TFID dataFileID = default, TID parentID = default)
        {
            using (await LockAsync())
            {
                await ImportExternalAssetAsyncImpl(externalAssetID, externalAsset, externalAssetKey, externalLoaderID, metaFileID, thumbnail, thumbnailID, dataFileID, parentID);
            }
        }

        protected async Task ImportExternalAssetAsyncImpl(TID externalAssetID, object externalAsset, string externalAssetKey, string externalLoaderID, TFID metaFileID, byte[] thumbnail = null, TFID thumbnailID = default, TFID dataFileID = default, TID parentID = default)
        {
            if (!TryGetExternalLoader(externalLoaderID, out AssetLoaderAdapter<TID> loader))
            {
                throw new ArgumentException($"loader {externalLoaderID} not found", "externalLoaderID");
            }

            if (IsExternalAssetRoot(externalAsset))
            {
                Debug.Log("External asset already imported, creating instance");
                externalAsset = loader.Instantiate(externalAssetKey, m_deps.AssetsRoot);
            }

            if (Equals(externalAssetID, m_idMap.NullID))
            {
                if (!m_idMap.TryGetID(externalAsset, out externalAssetID))
                {
                    if (!loader.TryGetID(externalAsset, out externalAssetID))
                    {
                        externalAssetID = m_idMap.CreateID();
                    }
                }
            }

            RegisterExternalAsset(externalAssetID, externalAsset, false);
            try
            {
                await CreateAssetAsyncImpl(externalAsset, metaFileID, thumbnail, thumbnailID, dataFileID, parentID, externalLoaderID, externalAssetKey);
            }
            catch (Exception)
            {
                UnregisterExternalAsset(externalAsset);
                loader.Release(externalAsset);
                throw;
            }
        }

        public bool IsExternalAssetRoot(TID assetID)
        {
            return TryGetMeta(assetID, out TMeta meta) && IsExternalAssetRoot(in meta);
        }

        protected bool IsExternalAssetRoot(in TMeta meta)
        {
            return !string.IsNullOrEmpty(meta.LoaderID);
        }

        public bool IsExternalAssetRoot(object obj)
        {
            return m_externalAssetToAssetID.ContainsKey(obj);
        }

        public bool IsExternalAsset(TID assetID)
        {
            if (IsExternalAssetRoot(assetID))
            {
                return true;
            }

            if (m_idMap.TryGetObject(assetID, out object asset) && m_assetMap.TryGetRootAssetByAsset(asset, out object rootAsset))
            {
                return IsExternalAssetRoot(rootAsset);
            }

            return false;
        }

        public bool IsExternalAsset(object obj)
        {
            if (IsExternalAssetRoot(obj))
            {
                return true;
            }

            if (m_assetMap != null && m_assetMap.TryGetRootAssetByAsset(obj, out object rootAsset))
            {
                return IsExternalAssetRoot(rootAsset);
            }

            return false;
        }

        public bool TryGetExternalAsset(TID assetID, out object externalAsset)
        {
            return m_assetIDToExternalAsset.TryGetValue(assetID, out externalAsset);
        }

        public bool TryGetExternalAssetID(object obj, out TID assetID)
        {
            return m_externalAssetToAssetID.TryGetValue(obj, out assetID);
        }

        public async Task RegisterExternalAssetsAsync(IDictionary<TID, object> externalAssets)
        {
            using (await LockAsync())
            {
                RegisterExternalAssetsImpl(externalAssets);
            }
        }

        protected void RegisterExternalAssetsImpl(IDictionary<TID, object> externalAssets)
        {
            foreach (var kvp in externalAssets)
            {
                RegisterExternalAsset(kvp.Key, kvp.Value);
            }
        }

        protected void RegisterExternalAsset(TID assetID, object externalAsset)
        {
            RegisterExternalAsset(assetID, externalAsset, true);
        }

        private void RegisterExternalAsset(TID assetID, object externalAsset, bool addToIDMap)
        {
            m_assetIDToExternalAsset.Add(assetID, externalAsset);
            m_externalAssetToAssetID.Add(externalAsset, assetID);

            if (IsProjectLoaded && addToIDMap)
            {
                m_idMap.AddObject(externalAsset, assetID);
            }
        }

        public async Task UnregisterExternalAssetAsync(params TID[] assetIDs)
        {
            using (await LockAsync())
            {
                UnregisterExternalAssetsImpl(assetIDs);
            }
        }

        protected void UnregisterExternalAssetsImpl(params TID[] assetIDs)
        {
            foreach (var assetID in assetIDs)
            {
                UnregisterExternalAsset(assetID);
            }
        }

        protected void UnregisterExternalAsset(TID assetID)
        {
            if (m_assetIDToExternalAsset.TryGetValue(assetID, out object externalAsset))
            {
                m_assetIDToExternalAsset.Remove(assetID);
                m_externalAssetToAssetID.Remove(externalAsset);

                if (IsProjectLoaded)
                {
                    m_idMap.Remove(assetID);
                }
            }
            else
            {
                Debug.LogWarning($"Resource {assetID} already unregistered");
            }
        }

        public async Task UnregisterExternalAssetsAsync(params object[] externalAssets)
        {
            using (await LockAsync())
            {
                UnregisterExternalAssetsImpl(externalAssets);
            }
        }

        protected void UnregisterExternalAssetsImpl(params object[] externalAssets)
        {
            foreach (object externalAsset in externalAssets)
            {
                UnregisterExternalAsset(externalAsset);
            }
        }

        protected void UnregisterExternalAsset(object externalAsset)
        {
            if (m_externalAssetToAssetID.TryGetValue(externalAsset, out TID assetID))
            {
                m_assetIDToExternalAsset.Remove(assetID);
                m_externalAssetToAssetID.Remove(externalAsset);

                if (IsProjectLoaded)
                {
                    m_idMap.Remove(assetID);
                }
            }
            else
            {
                if (externalAsset != null)
                {
                    Debug.LogWarning($"Resource {externalAsset.GetType()} {externalAsset} already unregistered");
                }
            }
        }

        public async Task ClearExternalAssetsAsync()
        {
            using (await LockAsync())
            {
                ClearExternalAssetsImpl();
            }
        }

        protected void ClearExternalAssetsImpl()
        {
            if (IsProjectLoaded)
            {
                foreach (var assetID in m_assetIDToExternalAsset.Keys)
                {
                    m_idMap.Remove(assetID);
                }
            }

            m_externalAssetToAssetID.Clear();
            m_assetIDToExternalAsset.Clear();
        }

        private void GetDependencies(TID assetID, HashSet<TID> dependencies, bool recursive)
        {
            if (!TryGetMeta(assetID, out TMeta meta))
            {
                return;
            }

            if (meta.OutboundDependencies == null)
            {
                return;
            }

            foreach (var id in meta.OutboundDependencies)
            {
                if (dependencies.Add(id) && recursive)
                {
                    GetDependencies(id, dependencies, recursive);
                }
            }
        }

        public async Task ExportAssetsAsync(TID[] assetIDs, TFID outFileID, bool includeDependencies = true)
        {
            using (await LockAsync())
            {
                await ExportAssetsImpl(assetIDs, outFileID);
            }
        }

        private Task ExportAssetsImpl(TID[] assetIDs, TFID outFileID)
        {
            if (!IsProjectLoaded)
            {
                throw new InvalidOperationException("Load project before exporting assets");
            }

            var idHs = new HashSet<TID>();
            for (int i = 0; i < assetIDs.Length; ++i)
            {
                var id = assetIDs[i];
                if (IsFolder(id))
                {
                    foreach (var childID in this.GetChildren(id, recursive: true))
                    {
                        if (!IsFolder(childID))
                        {
                            if (idHs.Add(childID))
                            {
                                GetDependencies(childID, idHs, recursive: true);
                            }
                        }
                    }
                }
                else
                {
                    if (idHs.Add(id))
                    {
                        GetDependencies(id, idHs, recursive: true);
                    }
                }
            }

            List<TFID> fileIDs = new List<TFID>();
            foreach (TID id in idHs)
            {
                if (!TryGetMeta(id, out TMeta meta))
                {
                    continue;
                }

                var metaFileID = GetMetaFileID(meta);
                if (!Equals(default, metaFileID))
                {
                    fileIDs.Add(metaFileID);
                }

                var dataFileID = GetDataFileID(meta);
                if (!Equals(default, dataFileID))
                {
                    fileIDs.Add(dataFileID);
                }

                var thumbFileID = GetThumbnailFileID(meta);
                if (!Equals(default, thumbFileID))
                {
                    fileIDs.Add(thumbFileID);
                }
            }

            var dataLayer = m_deps.DataLayer;
            return dataLayer.CompressZipAsync(RootFID, fileIDs.ToArray(), null, outFileID);
        }

        public async Task ImportAssetsAsync(TFID fileID, bool reloadProject)
        {
            using (await LockAsync())
            {
                await ImportAssetAsyncImpl(fileID, reloadProject);
            }
        }

        private async Task ImportAssetAsyncImpl(TFID fileID, bool reloadProject)
        {
            var projectFID = RootFID;
            var dataLayer = m_deps.DataLayer;
            TFID[] importedFiles = await dataLayer.UncompressZipAsync(fileID, null, projectFID);
            for (int i = 0; i < importedFiles.Length; i++)
            {
                var importedFileID = NormalizeFileID(importedFiles[i]);
                if (!IsMetaFileID(importedFileID))
                {
                    continue;
                }

                TMeta importedMeta = await DeserializeAsync(importedFileID, m_deps.DataLayer, m_deps.Serializer);
                if (TryGetMeta(importedMeta.ID, out TMeta existingMeta))
                {
                    var existingFileID = NormalizeFileID(GetMetaFileID(existingMeta));
                    if (!Equals(importedFileID, existingFileID))
                    {
                        await dataLayer.DeleteAsync(GetMetaFileID(in existingMeta));
                        await dataLayer.DeleteAsync(GetDataFileID(in existingMeta));
                        await dataLayer.DeleteAsync(GetThumbnailFileID(in existingMeta));
                    }
                }
            }

            if (reloadProject)
            {
                await UnloadProjectAsyncImpl(true);
                await LoadProjectAsyncImpl(projectFID);
            }
        }

        protected virtual TMeta CreateMeta(TID id, int typeId, TFID dataFileID, TFID thumbnailID, TFID metaFileID, object asset)
        {
            TMeta meta = new TMeta();
            meta.ID = id;
            meta.TypeID = typeId;
            meta.InboundDependencies = new HashSet<TID>();

            SetDataFileID(ref meta, dataFileID);
            SetThumbnailFileID(ref meta, thumbnailID);
            SetMetaFileID(ref meta, metaFileID);
            return meta;
        }

        [Obsolete]
        protected virtual TMeta CreateMeta(TID id, int typeId, TFID dataFileID, TFID metaFileID)
        {
            TMeta meta = new TMeta();
            meta.ID = id;
            meta.TypeID = typeId;

            SetDataFileID(ref meta, dataFileID);
            SetMetaFileID(ref meta, metaFileID);
            return meta;
        }

        protected virtual TFID NormalizeFileID(TFID fileID)
        {
            return fileID;
        }

        protected virtual bool IsMetaFileID(TFID fileID)
        {
            return false;
        }

        protected virtual TFID GetMetaFileID(in TMeta meta)
        {
            return NormalizeFileID(meta.FileID);
        }

        protected virtual void SetMetaFileID(ref TMeta meta, TFID fileID)
        {
            meta.FileID = NormalizeFileID(fileID);
        }

        protected virtual TFID GetDataFileID(in TMeta meta)
        {
            return NormalizeFileID(meta.DataFileID);
        }

        protected virtual void SetDataFileID(ref TMeta meta, TFID dataFileID)
        {
            meta.DataFileID = NormalizeFileID(dataFileID);
        }

        protected virtual TFID GetThumbnailFileID(in TMeta meta)
        {
            return NormalizeFileID(meta.ThumbnailFileID);
        }

        protected virtual void SetThumbnailFileID(ref TMeta meta, TFID thumbnailFileID)
        {
            meta.ThumbnailFileID = NormalizeFileID(thumbnailFileID);
        }

        private async Task SerializeDataAsync(Stream stream, ISurrogatesSerializer<TID> serializer)
        {
            await TaskUtils.Run(async () =>
            {
                try
                {
                    await serializer.SerializeToStream(stream);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            });
        }

        protected async Task<TID> SerializeToStreamAsyncImpl(object obj, IIDMap<TID> idMap, Stream stream)
        {
            using var serializeRef = m_deps.AcquireSerializerRef();
            var serializer = serializeRef.Get();

            TID assetID = default;
            TryGetAssetID(obj, out assetID);

#if SINGLE_THREADED
            await SerializeObjectAsync(obj, serializer, idMap, assetID);
            var serializeDataTask = SerializeDataAsync(stream, serializer);
#else
            var serializeDataTask = SerializeDataAsync(stream, serializer);
            await SerializeObjectAsync(obj, serializer, idMap, assetID);
#endif
            await serializeDataTask;
            idMap.TryGetID(obj, out TID id);
            return id;
        }

        protected async Task<TID> SerializeToStreamAsyncImpl(object obj, Stream stream)
        {
            using var idMapRef = m_deps.AcquireIDMapRef(m_idMap);
            var idMap = idMapRef.Get();
            //idMap.IsReadOnly = false;
            return await SerializeToStreamAsyncImpl(obj, idMap, stream);
        }

        public async Task<TID> SerializeAsync(object obj, Stream stream)
        {
            using (await LockAsync())
            {
                return await SerializeToStreamAsyncImpl(obj, stream);
            }
        }

        private Task BeginDeserialize(Stream stream, ISurrogatesSerializer<TID> serializer)
        {
            return TaskUtils.Run(async () =>
            {
                await serializer.DeserializeFromStream(stream);
            });
        }

        protected async Task<object> DeserializeFromStreamAsyncImpl(TID id, IIDMap<TID> idMap, Stream stream)
        {
            using var serializerRef = m_deps.AcquireSerializerRef();
            using var contextRef = m_deps.AcquireContextRef();
            //using var assetMapRef = m_deps.AcquireAssetMapRef(m_assetMap);

            var workloadCtrl = m_deps.WorkloadController;
            var serializer = serializerRef.Get();
            var context = contextRef.Get();
            //var assetMap = assetMapRef.Get();

            context.IDMap = idMap;
            context.ShaderUtil = m_deps.ShaderUtil;
            context.TypeMap = m_deps.TypeMap;
            context.TempRoot = m_deps.AssetsRoot;

            var deserializeTask = BeginDeserialize(stream, serializer);

            await DeserializeObjectAsync(serializer, context, null, deserializeTask);

            return context.IDMap.GetObject<object>(id);
        }

        protected async Task<object> DeserializeFromStreamAsyncImpl(TID id, Stream stream)
        {
            using var idMapRef = m_deps.AcquireIDMapRef(m_idMap);
            var idMap = idMapRef.Get();
            //idMap.IsReadOnly = false;

            return await DeserializeFromStreamAsyncImpl(id, idMap, stream);
        }

        public async Task<object> DeserializeAsync(TID id, Stream stream)
        {
            using (await LockAsync())
            {
                return await DeserializeFromStreamAsyncImpl(id, stream);
            }
        }
    }

    public interface IAssetDatabaseInternalUtils<TID, TFID>
        where TID : IEquatable<TID>
        where TFID : IEquatable<TFID>
    {
        IModuleDependencies<TID, TFID> Deps
        {
            get;
        }

        IIDMap<TID> RootIDMap
        {
            get;
        }

        IAssetMap<TID> RootAssetMap
        {
            get;
        }

        Task FillIDMapAsync(object obj, IIDMap<TID> objIDMap, IIDMap<TID> outIDMap);
        Dictionary<object, object> GetObjectMap(object source, object target);
        bool AreEqual(object source, object target, IReadOnlyDictionary<object, object> sourceToTargetMap);
        Task<Dictionary<object, object>> CopyObjectAsync(object source, IReadOnlyDictionary<object, object> sourceToTargetMap, ICollection<object> sourceMarkedAsDestroyed = null, bool strict = false);
        Task<HashSet<object>> PropagateChangesAsync(object obj, IAssetMap<TID> assetMap, bool strict = false);
    }

    /// <summary>
    /// The asset database does not support multiple threads, so a simple bool variable is sufficient.
    /// </summary>
    public class RuntimeAssetDatabaseLock
    {
        private class LockReleaser : IDisposable
        {
            private RuntimeAssetDatabaseLock m_asyncLock;
            private bool m_disposed;

            public LockReleaser(RuntimeAssetDatabaseLock asyncLock)
            {
                m_asyncLock = asyncLock;
            }

            public void Reset()
            {
                m_disposed = false;
            }

            public void Dispose()
            {
                if (!m_disposed)
                {
                    m_disposed = true;
                    m_asyncLock.m_lock = false;
                }
            }
        }

        private bool m_lock;
        private LockReleaser m_releaser;

        public async Task<IDisposable> LockAsync()
        {
            while (m_lock)
            {
                await Task.Yield();
            }

            m_lock = true;

            // re-use, don't create garbage
            if (m_releaser == null)
            {
                m_releaser = new LockReleaser(this);
            }
            else
            {
                m_releaser.Reset();
            }

            return m_releaser;
        }
    }
}