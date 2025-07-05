using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public struct ID : IEquatable<ID>
    {
        private Guid m_id;
        private object m_ref;

        public static ID Empty = new ID(Guid.Empty);

        public static ID NewID()
        {
            return new ID(Guid.NewGuid());
        }

        public ID(Guid id)
        {
            m_id = id;
            m_ref = null;
        }

        public ID(object objRef)
        {
            m_id = Guid.Empty;
            m_ref = objRef;
        }

        public static bool operator ==(ID a, ID b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ID a, ID b)
        {
            return !a.Equals(b);
        }

        public static implicit operator Guid(ID id)
        {
            return id.Guid;
        }

        public static implicit operator ID(Guid id)
        {
            return new ID(id);
        }

        internal Guid Guid
        {
            get { return m_id; }
        }

        internal object Ref
        {
            get { return m_ref; }
        }

        public bool Equals(ID other)
        {
            return m_id == other.m_id && m_ref == other.m_ref;
        }

        public override bool Equals(object obj)
        {
            return obj is ID other && Equals(other);
        }

        public override int GetHashCode()
        {
            if (m_ref != null)
            {
                return m_ref.GetHashCode();
            }

            return m_id.GetHashCode();
        }

        public override string ToString()
        {
            if (m_ref != null)
            {
                return m_ref.ToString();
            }
            return m_id.ToString();
        }
    }

    public class AssetSelectionEventArgs
    {
        public ID[] SelectedAssets
        {
            get;
            private set;
        }

        public ID[] UnselectedAssets
        {
            get;
            private set;
        }

        public AssetSelectionEventArgs(ID[] selectedAssets, ID[] unselectedAssets)
        {
            SelectedAssets = selectedAssets;
            UnselectedAssets = unselectedAssets;
        }
    }


    public class InstanceEventArgs
    {
        public GameObject Instance
        {
            get;
            private set;
        }

        public InstanceEventArgs(GameObject instance)
        {
            Instance = instance;
        }
    }

    public class InstancesEventArgs
    {
        public GameObject[] Instances
        {
            get;
            private set;
        }

        public InstancesEventArgs(GameObject[] instances)
        {
            Instances = instances;
        }
    }

    public class AssetEventArgs
    {
        public ID AssetID
        {
            get;
            private set;
        }

        public AssetEventArgs(ID assetID)
        {
            AssetID = assetID;
        }
    }

    public class CreateFolderEventArgs
    {
        public ID AssetID
        {
            get;
            private set;
        }

        public CreateFolderEventArgs(ID assetID)
        {
            AssetID = assetID;
        }
    }


    public class BeforeCreateAssetEventArgs
    {
        public object Object
        {
            get;
            private set;
        }

        public string Path
        {
            get;
            private set;
        }

        public BeforeCreateAssetEventArgs(object obj, string path)
        {
            Object = obj;
            Path = path;
        }
    }

    public class CreateAssetEventArgs
    {
        public ID AssetID
        {
            get;
            private set;
        }

        public ID OverwrittenAssetID
        {
            get;
            private set;
        }

        private Texture2D m_thumbnail;
        public Texture2D Thumbnail
        {
            get
            {
                if (m_thumbnail == null)
                {
                    return null;
                }

                var thumbnail = UnityEngine.Object.Instantiate(m_thumbnail);
                thumbnail.name = $"Thumbnail {AssetID}";
                return thumbnail;
            }
        }

        public CreateAssetEventArgs(ID assetID, ID overwrittenAssetID, Texture2D texture)
        {
            AssetID = assetID;
            OverwrittenAssetID = overwrittenAssetID;
            m_thumbnail = texture;
        }
    }


    public class SaveAssetEventArgs
    {
        public ID AssetID
        {
            get;
            private set;
        }


        private Texture2D m_thumbnail;
        public Texture2D Thumbnail
        {
            get
            {
                if (m_thumbnail == null)
                {
                    return null;
                }

                var thumbnail = UnityEngine.Object.Instantiate(m_thumbnail);
                thumbnail.name = $"Thumbnail {AssetID}";
                return thumbnail;
            }
        }

        public SaveAssetEventArgs(ID assetID, Texture2D texture)
        {
            AssetID = assetID;
            m_thumbnail = texture;
        }
    }

    public class MoveAssetsEventArgs
    {
        public IReadOnlyList<ID> ParentID
        {
            get;
            private set;
        }

        public IReadOnlyList<ID> AssetID
        {
            get;
            private set;
        }

        public IReadOnlyList<IReadOnlyList<ID>> ChildrenID
        {
            get;
            private set;
        }

        public IReadOnlyList<string> NewPath
        {
            get;
            private set;
        }

        public MoveAssetsEventArgs(IReadOnlyList<ID> assetID, IReadOnlyList<ID> parentID, IReadOnlyList<IReadOnlyList<ID>> childrenID, IReadOnlyList<string> newPath)
        {

            AssetID = assetID;
            ParentID = parentID;
            ChildrenID = childrenID;
            NewPath = newPath;
        }
    }

    public class DuplicateAssetsEventArgs
    {
        public IReadOnlyList<ID> SourceAssetID
        {
            get;
            private set;
        }

        public IReadOnlyList<ID> AssetID
        {
            get;
            private set;
        }


        public DuplicateAssetsEventArgs(IReadOnlyList<ID> sourceAssetID, IReadOnlyList<ID> assetID)
        {
            SourceAssetID = sourceAssetID;
            AssetID = assetID;
        }
    }

    public class DeleteAssetsEventArgs
    {
        public IReadOnlyList<ID> AssetID
        {
            get;
            private set;
        }

        public IReadOnlyList<ID> ParentID
        {
            get;
            private set;
        }

        public IReadOnlyList<IReadOnlyList<ID>> ChildrenID
        {
            get;
            private set;
        }

        public DeleteAssetsEventArgs(IReadOnlyList<ID> assetID, IReadOnlyList<ID> parentID, IReadOnlyList<IReadOnlyList<ID>> childrenID)
        {
            ParentID = parentID;
            AssetID = assetID;
            ChildrenID = childrenID;
        }
    }

    public class InstantiateAssetsResult
    {
        public GameObject Instance
        {
            get { return Instances != null && Instances.Length > 0 ? Instances[0] : null; }
        }

        public GameObject[] Instances
        {
            get;
            private set;
        }

        public bool IsCyclicNestingDetected
        {
            get;
            private set;
        }

        public InstantiateAssetsResult(GameObject[] instances, bool isCyclicNestingDetected)
        {
            Instances = instances;
            IsCyclicNestingDetected = isCyclicNestingDetected;
        }
    }

    public static class IAssetDatabaseModelExt
    {
        public static bool IsScene(this IAssetDatabaseModel assetDatabase, string path) => assetDatabase.IsScene(assetDatabase.GetAssetID(path));

        public static bool IsPrefab(this IAssetDatabaseModel assetDatabase, string path) => assetDatabase.IsPrefab(assetDatabase.GetAssetID(path));

        public static bool IsPrefabVariant(this IAssetDatabaseModel assetDatabase, string path) => assetDatabase.IsPrefabVariant(assetDatabase.GetAssetID(path));

        public static bool IsExternalAsset(this IAssetDatabaseModel assetDatabase, string path) => assetDatabase.IsExternalAsset(assetDatabase.GetAssetID(path));

        public static bool IsFolder(this IAssetDatabaseModel assetDatabase, string path) => assetDatabase.IsFolder(assetDatabase.GetAssetID(path));

        public static bool Exists(this IAssetDatabaseModel assetDatabase, string path) => assetDatabase.Exists(assetDatabase.GetAssetID(path));

        public static string GetPath(this IAssetDatabaseModel assetDatabase, ID folderId, object obj, string name)
        {
            string displayName = Path.GetFileNameWithoutExtension(name);
            string ext = assetDatabase.GetExt(obj);
            return $"{assetDatabase.GetPath(folderId)}/{displayName}{ext}";
        }

        public static string GetPath(this IAssetDatabaseModel assetDatabase, ID folderId, string name)
        {
            return $"{assetDatabase.GetPath(folderId)}/{name}";
        }

        public static string GetPath(this IAssetDatabaseModel assetDatabase, ID folderId, string name, string ext)
        {
            return $"{assetDatabase.GetPath(folderId)}/{name}{ext}";
        }

        public static string GetRootFolderPath(this IAssetDatabaseModel assetDatabase)
        {
            return assetDatabase.GetPath(assetDatabase.RootFolderID);
        }

        public static string GetRootFolderPath(this IAssetDatabaseModel assetDatabase, string name)
        {
            return $"{assetDatabase.GetRootFolderPath()}/{name}";
        }

        public static string GetCurrentFolderPath(this IAssetDatabaseModel assetDatabase)
        {
            return assetDatabase.GetPath(assetDatabase.CurrentFolderID);
        }

        public static string GetCurrentFolderPath(this IAssetDatabaseModel assetDatabase, string name)
        {
            return $"{assetDatabase.GetCurrentFolderPath()}/{name}";
        }

        public static string GetCurrentFolderPath(this IAssetDatabaseModel assetDatabase, string name, string ext)
        {
            return $"{assetDatabase.GetCurrentFolderPath()}/{name}{ext}";
        }

        public static string GetCurrentScenePath(this IAssetDatabaseModel assetDatabase)
        {
            return assetDatabase.GetPath(assetDatabase.CurrentSceneID);
        }

        public static string GetSceneExt(this IAssetDatabaseModel assetDatabase)
        {
            return assetDatabase.GetExt(assetDatabase.CurrentScene);
        }

        public static string GetExtByID(this IAssetDatabaseModel assetDatabase, ID id)
        {
            string path = assetDatabase.GetPath(id);
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return Path.GetExtension(path).ToLower();
        }


        public static string GetUniquePath(this IAssetDatabaseModel assetDatabase, ID folderId, object obj, string desiredName)
        {
            string displayName = Path.GetFileNameWithoutExtension(desiredName);
            string ext = assetDatabase.GetExt(obj);
            return assetDatabase.GetUniquePath(folderId, $"{displayName}{ext}");
        }

        public static void AddExternalAssetLoader(this IAssetDatabaseModel assetDatabase, IExternalAssetLoaderModel loader)
        {
            assetDatabase.AddExternalAssetLoader(loader.LoaderID, loader);
        }

        public static void RemoveExternalAssetLoader(this IAssetDatabaseModel assetDatabase, IExternalAssetLoaderModel loader)
        {
            assetDatabase.RemoveExternalAssetLoader(loader.LoaderID);
        }

        public static async Task<T> LoadAssetAsync<T>(this IAssetDatabaseModel assetDatabase, ID id)
        {
            return (T)await assetDatabase.LoadAssetAsync(id);
        }

        public static async Task<T> LoadAssetAsync<T>(this IAssetDatabaseModel assetDatabase, string path)
        {
            return (T)await assetDatabase.LoadAssetAsync(path);
        }

        public static Task<object> LoadAssetAsync(this IAssetDatabaseModel assetDatabase, string path)
        {
            ID id = assetDatabase.GetAssetID(path);
            if (id == ID.Empty)
            {
                throw new ArgumentException($"asset {path} not found ");
            }

            return assetDatabase.LoadAssetAsync(id);
        }

        public static Task OpenSceneAsync(this IAssetDatabaseModel assetDatabase, string path)
        {
            string ext = assetDatabase.GetSceneExt();
            if (!path.EndsWith(ext))
            {
                path = $"{path}{ext}";
            }

            return assetDatabase.OpenAssetAsync(path);
        }

        public static Task OpenSceneAsync(this IAssetDatabaseModel assetDatabase, ID sceneID)
        {
            return assetDatabase.OpenAssetAsync(sceneID);
        }

        public static Task OpenAssetAsync(this IAssetDatabaseModel assetDatabase, string path)
        {
            ID id = assetDatabase.GetAssetID(path);
            if (id == ID.Empty)
            {
                throw new ArgumentException($"asset {path} not found ");
            }

            return assetDatabase.OpenAssetAsync(id);
        }

        public static Task MoveAssetAsync(this IAssetDatabaseModel assetDatabase, string assetPath, string targetAssetPath)
        {
            return assetDatabase.MoveAssetAsync(assetDatabase.GetAssetID(assetPath), targetAssetPath);
        }

        public static Task MoveAssetAsync(this IAssetDatabaseModel assetDatabase, ID assetID, string targetAssetPath)
        {
            return assetDatabase.MoveAssetsAsync(new[] { assetID }, new[] { targetAssetPath });
        }

        public static Task MoveAssetAsync(this IAssetDatabaseModel assetDatabase, ID assetID, ID targetFolderID)
        {
            return assetDatabase.MoveAssetsAsync(new[] { assetID }, targetFolderID);
        }

        public static async Task MoveAssetsAsync(this IAssetDatabaseModel assetDatabase, IEnumerable<ID> ids, ID targetFolderID)
        {
            var assetIDs = assetDatabase.ExcludeDescendants(ids).ToArray();
            var targetPath = new List<string>();

            foreach (var id in assetIDs)
            {
                string name = assetDatabase.GetName(id);
                targetPath.Add(assetDatabase.GetUniquePath(targetFolderID, name));
            }

            await assetDatabase.MoveAssetsAsync(assetIDs, targetPath);
        }

        public static Task DuplicateAssetAsync(this IAssetDatabaseModel assetDatabase, ID assetID)
        {
            return assetDatabase.DuplicateAssetsAsync(new[] { assetID });
        }

        public static Task DuplicateAssetAsync(this IAssetDatabaseModel assetDatabase, string path)
        {
            return assetDatabase.DuplicateAssetsAsync(new[] { path });
        }

        public static Task DuplicateAssetsAsync(this IAssetDatabaseModel assetDatabase, IReadOnlyList<string> paths)
        {
            return assetDatabase.DuplicateAssetsAsync(paths.Select(p => assetDatabase.GetAssetID(p)).Where(id => id != ID.Empty).ToArray());
        }

        public static Task DuplicateAssetsAsync(this IAssetDatabaseModel assetDatabase, IEnumerable<ID> ids)
        {
            var assetIDs = assetDatabase.ExcludeDescendants(ids).ToArray();
            var path = new string[assetIDs.Length];
            for (int i = 0; i < assetIDs.Length; ++i)
            {
                var id = assetIDs[i];
                var parentID = assetDatabase.GetParent(id);
                path[i] = assetDatabase.GetUniquePath(parentID, assetDatabase.GetName(id));
            }
            return assetDatabase.DuplicateAssetsAsync(assetIDs, path);
        }

        public static Task DuplicateAssetAsync(this IAssetDatabaseModel assetDatabase, ID assetID, string toPath)
        {
            return assetDatabase.DuplicateAssetsAsync(new[] { assetID }, new[] { toPath });
        }

        public static Task DeleteFolderAsync(this IAssetDatabaseModel assetDatabase, ID assetID)
        {
            return assetDatabase.DeleteAssetAsync(assetID);
        }

        public static Task DeleteAssetAsync(this IAssetDatabaseModel assetDatabase, ID assetID)
        {
            return assetDatabase.DeleteAssetsAsync(new[] { assetID });
        }

        public static Task DeleteAssetAsync(this IAssetDatabaseModel assetDatabase, string path)
        {
            return assetDatabase.DeleteAssetsAsync(new[] { path });
        }

        public static Task DeleteAssetsAsync(this IAssetDatabaseModel assetDatabase, IReadOnlyList<string> paths)
        {
            return assetDatabase.DeleteAssetsAsync(paths.Select(p => assetDatabase.GetAssetID(p)).Where(id => id != ID.Empty).ToArray());
        }

        public static IEnumerable<ID> ExcludeDescendants(this IAssetDatabaseModel assetDatabase, IEnumerable<ID> ids)
        {
            var result = new List<ID>();
            var hs = new HashSet<ID>(ids);

            foreach (var id in ids)
            {
                var parentID = id;
                bool add = true;
                while (parentID != ID.Empty)
                {
                    parentID = assetDatabase.GetParent(parentID);
                    if (hs.Contains(parentID))
                    {
                        add = false;
                        break;
                    }
                }

                if (add)
                {
                    result.Add(id);
                }
            }

            return result;
        }

        public static Task<InstantiateAssetsResult> InstantiateAssetAsync(this IAssetDatabaseModel assetDatabase, ID id)
        {
            return assetDatabase.InstantiateAssetsAsync(new[] { id });
        }

        public static Task<InstantiateAssetsResult> InstantiateAssetAsync(this IAssetDatabaseModel assetDatabase, string path)
        {
            return assetDatabase.InstantiateAssetsAsync(new[] { path });
        }

        public static Task<InstantiateAssetsResult> InstantiateAssetsAsync(this IAssetDatabaseModel assetDatabase, IReadOnlyList<string> paths)
        {
            return assetDatabase.InstantiateAssetsAsync(paths.Select(p => assetDatabase.GetAssetID(p)).Where(id => id != ID.Empty).ToArray());
        }

        public static Task ReleaseAsync(this IAssetDatabaseModel assetDatabase, GameObject instance)
        {
            return assetDatabase.ReleaseAsync(new[] { instance });
        }
    }

    public struct ExtractSubAssetOptions
    {
        public bool IncludeExisting;
        public bool IncludeExternal;
        public bool IncludeNonSerializable;
    }

    public interface IAssetDatabaseModel
    {
        event EventHandler BeforeLoadProject;

        event EventHandler LoadProject;

        event EventHandler BeforeUnloadProject;

        event EventHandler UnloadProject;

        event EventHandler BeforeReloadProject;

        event EventHandler ReloadProject;

        event EventHandler ChangeCurrentFolder;

        event EventHandler<AssetSelectionEventArgs> ChangeAssetSelection;

        event EventHandler<CreateFolderEventArgs> CreateFolder;

        event EventHandler<BeforeCreateAssetEventArgs> BeforeCreateAsset;

        event EventHandler<CreateAssetEventArgs> CreateAsset;

        event EventHandler<SaveAssetEventArgs> SaveAsset;

        event EventHandler InitializeNewScene;

        event EventHandler<SaveAssetEventArgs> UpdateAssetThumbnail;

        event EventHandler<MoveAssetsEventArgs> BeforeMoveAssets;

        event EventHandler<MoveAssetsEventArgs> MoveAssets;

        event EventHandler<DuplicateAssetsEventArgs> DuplicateAssets;

        event EventHandler<DeleteAssetsEventArgs> BeforeDeleteAssets;

        event EventHandler<DeleteAssetsEventArgs> DeleteAssets;

        event EventHandler<AssetEventArgs> BeforeOpenAsset;

        event EventHandler<AssetEventArgs> OpenAsset;

        event EventHandler<AssetEventArgs> BeforeOpenPrefab;

        event EventHandler<AssetEventArgs> OpenPrefab;

        event EventHandler<AssetEventArgs> BeforeClosePrefab;

        event EventHandler<AssetEventArgs> ClosePrefab;

        event EventHandler<AssetEventArgs> BeforeOpenScene;

        event EventHandler<AssetEventArgs> OpenScene;

        event EventHandler<InstancesEventArgs> InstantiateAssets;

        event EventHandler<InstancesEventArgs> Detach;

        event EventHandler<InstanceEventArgs> SetDirty;

        event EventHandler<InstancesEventArgs> Duplicate;

        event EventHandler<InstancesEventArgs> Release;

        event EventHandler<InstanceEventArgs> BeforeApplyChanges;

        event EventHandler<InstanceEventArgs> ApplyChanges;

        event EventHandler<InstanceEventArgs> BeforeApplyChangesToBase;

        event EventHandler<InstanceEventArgs> ApplyChangesToBase;

        event EventHandler<InstanceEventArgs> BeforeRevertChangesToBase;

        event EventHandler<InstanceEventArgs> RevertChangesToBase;

        bool CanSaveScene { get; }

        bool CanInitializeNewScene { get; }

        GameObject CurrentScene { get; }

        ID CurrentSceneID { get; }

        bool CanCreatePrefab(object obj);

        bool CanCreatePrefabVariant(object obj);

        bool CanCreatePrefabVariant(ID id);

        bool CanSelectPrefab(object obj);

        bool CanOpenPrefab(object obj);

        bool CanClosePrefab { get; }

        void AddOpenableAssetExt(string ext);

        void RemoveOpenableAssetExt(string ext);

        bool CanOpenAsset(ID assetID);

        bool CanEditAsset(ID assertID);

        bool CanInstantiateAsset(ID assetID);

        bool CanDuplicateAsset(ID assetID);

        bool CanDetach(object[] instances);

        bool CanDuplicate(object[] instances);

        bool CanRelease(object[] instances);

        bool CanApplyChanges(object instance);

        bool CanApplyToBase(object instance);

        bool CanRevertToBase(object instance);

        GameObject CurrentHierarchyParent { get; set; }

        GameObject CurrentPrefab { get; }

        string ProjectID { get; }

        bool IsProjectLoaded { get; }

        ID RootFolderID { get; }

        string LibraryRootFolder { get; }

        string GetFolderInLibrary(ID assetID);

        ID CurrentFolderID { get; set; }

        ID[] SelectedAssets { get; set; }

        void AddRuntimeSerializableTypes(Type[] types, Guid[] typeIDs);

        void AddRuntimeSerializableTypes(params Type[] types);

        void RemoveRuntimeSerializableTypes(params Type[] types);

        [Obsolete("Use AddRuntimeSerializableTypes")]
        void AddRuntimeSerializableType(Type type, Guid typeID = default);

        [Obsolete("Use RemoveRuntimeSerializableType")]
        void RemoveRuntimeSerializableType(Type type);

        void SetRuntimeTypeResolver(Func<string, Type> resolveType);

        void AddExtension(IAssetDatabaseProjectExtension extension);

        void RemoveExtension(IAssetDatabaseProjectExtension extension);

        void AddExternalAssetLoader(string loaderID, IExternalAssetLoaderModel loader);

        void RemoveExternalAssetLoader(string loaderID);

        IReadOnlyList<IImportSourceModel> ImportSources { get; }

        void AddImportSource(IImportSourceModel importSource);

        void RemoveImportSource(IImportSourceModel importSource);

        Task LoadProjectAsync(string projectID, string version = null);

        Task UnloadProjectAsync();

        Task ReloadProjectAsync();

        bool IsPrefabOperationAllowed(object instance);

        bool IsAssetRoot(object obj);

        bool IsAsset(object obj);

        bool IsInstanceOfAssetVariant(object obj);

        bool IsInstanceOfAssetVariantRef(object obj);

        bool IsInstanceRoot(object obj);

        bool IsInstanceRootRef(object obj);

        bool IsInstance(object obj);

        bool IsDirtyObject(object obj);

        bool IsAddedObject(object obj);

        bool HasChanges(object instance, object instanceRootOpenedForEditing);

        bool IsScene(ID id);

        bool IsPrefab(ID id);

        bool IsPrefabVariant(ID id);

        bool IsExternalAsset(ID id);

        bool IsExternalAsset(object obj);

        bool IsFolder(ID id);

        bool Exists(ID id);

        Type GetType(ID id);

        ID GetTypeID(Type type);

        object CreateObjectOfType(Type type);

        string GetName(ID id);

        string GetDisplayName(ID id);

        string GetName(string path);

        string GetDisplayName(string path);

        bool IsValidName(string name);

        ID GetAssetID(string path);

        ID GetAssetID(object asset);

        ID GetSubAssetID(object subAsset);

        ID GetAssetIDByInstance(object instance);

        IEnumerable<GameObject> GetInstancesByAssetID(ID assetID);

        bool IsLoaded(ID id);

        object GetAsset(ID id);

        object GetAssetByInstance(object instance);

        bool IsRawData(ID id);

        T GetRawData<T>(ID id);

        void SetRawData<T>(ID id, T data);

        Task<object> LoadAssetAsync(ID id);

        string GetPath(ID id);

        string GetExt(object obj);

        string GetUniquePath(string path);

        string GetUniquePath(ID folderId, string desiredName);

        byte[] GetThumbnailData(ID id);

        Task<byte[]> LoadThumbnailDataAsync(ID id);

        ID GetParent(ID id);

        bool HasChildren(ID id);

        IEnumerable<ID> GetChildren(ID id, bool sortByName = true, bool recursive = false, string searchPattern = null);

        Task<IEnumerable<object>> ExtractSubAssetsAsync(object asset, ExtractSubAssetOptions options = default);

        Task DontDestroySubAssetsAsync(object obj);

        Task<ID> CreateFolderAsync(string path);

        Task<ID> CreateAssetAsync(object obj, string path, bool variant = false, bool extractSubassets = false);

        Task<ID> ImportExternalAssetAsync(ID folderID, object key, string loaderID, string desiredName);

        Task<ID> ImportExternalAssetAsync(ID folderID, ID assetID, object key, string loaderID, string desiredName);

        Task ExportAssetsAsync(ID[] assets, Stream ostream, bool includeDependencies = true);
        
        Task ImportAssetsAsync(Stream istream);

        Task InitializeNewSceneAsync();

        Task UnloadAllAndClearSceneAsync();

        Task SaveAssetAsync(ID assetID);

        Task UpdateThumbnailAsync(ID asetID);

        Task MoveAssetsAsync(IReadOnlyList<ID> assetIDs, IReadOnlyList<string> toPaths);

        Task DuplicateAssetsAsync(IReadOnlyList<ID> assetIDs, IReadOnlyList<string> toPaths);

        Task DeleteAssetsAsync(IReadOnlyList<ID> assetIDs);

        Task SelectPrefabAsync(GameObject instance);

        Task OpenPrefabAsync(GameObject instance);

        Task ClosePrefabAsync();

        Task OpenAssetAsync(ID assetID);

        bool IsCyclicNesting(GameObject instance, Transform parent);

        Task<InstantiateAssetsResult> InstantiateAssetsAsync(ID[] assetIDs, Transform parent = null);

        Task DetachAsync(GameObject[] instances, bool completely, bool cloneSubAssets = false);

        Task SetDirtyAsync(object instance);

        Task DuplicateAsync(GameObject[] instances);

        Task ReleaseAsync(GameObject[] instances);

        bool IsCyclicNestingAfterApplyingChanges(GameObject instance, bool toBase);

        Task ApplyChangesAsync(GameObject instance);

        Task ApplyToBaseAsync(GameObject instance);

        Task RevertToBaseAsync(GameObject instance);

        // Serializer

        Task<byte[]> SerializeAsync(object asset);

        Task<object> DeserializeAsync(byte[] data, object target = null);

        // KeyValue Storage

        Task<T> GetValueAsync<T>(string key);

        Task SetValueAsync<T>(string key, T obj);

        Task DeleteValueAsync<T>(string key);
    }
}
