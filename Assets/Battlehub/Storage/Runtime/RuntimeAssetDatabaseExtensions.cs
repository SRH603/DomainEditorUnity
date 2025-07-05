using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Battlehub.Storage
{
    public static class RuntimeAssetDatabaseExtensions
    {
        public static IReadOnlyCollection<Type> GetSerializableTypes(this IAssetDatabase<Guid, string> _)
        {
            return RuntimeAssetDatabase.Deps.TypeMap.Types;
        }

        public static string GetParent(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            var parentID = assetDatabase.GetParent(meta.ID);
            if (parentID == default)
            {
                return default;
            }
            var parentMeta = assetDatabase.GetMeta(parentID);
            return parentMeta.FileID;
        }

        public static IReadOnlyList<string> GetChildren(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            var children = assetDatabase.GetChildren(meta.ID);
            var result = new List<string>();
            for (int i = 0; i < children.Count; ++i)
            {
                var childMeta = assetDatabase.GetMeta(children[i]);
                result.Add(childMeta.FileID);
            }
            return result;
        }

        public static bool IsFolder(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.IsFolder(meta.ID);
        }

        public static Type GetAssetType(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.GetAssetType(meta.ID);
        }

        public static Type GetAssetType(this IAssetDatabase<Guid, string> assetDatabase, object asset)
        {
            var meta = assetDatabase.GetMeta(asset);
            return assetDatabase.GetAssetType(meta.ID);
        }

        public static Guid GetAssetTypeIDByType(this IAssetDatabase<Guid, string> _, Type type)
        {
            var deps = RuntimeAssetDatabase.Deps;
            if(!deps.TypeMap.TryGetID(type, out int id))
            {
                return default;
            }

            byte[] bytes = new byte[16];
            BitConverter.GetBytes(id).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static Type GetAssetTypeByTypeID(this IAssetDatabase<Guid, string> _, Guid typeID)
        {
            var deps = RuntimeAssetDatabase.Deps;
            var id = BitConverter.ToInt32(typeID.ToByteArray(), 0);
            if (!deps.TypeMap.TryGetType(id, out var type))
            {
                return default;
            }

            return type;
        }

        public static bool IsLoaded(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            if (!assetDatabase.TryGetMeta(fileID, out var meta))
            {
                return false;
            }

            return assetDatabase.IsLoaded(meta.ID);
        }

        public static Task LoadAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.LoadAssetAsync(meta.ID);
        }

        public static Task LoadThumbnailAsync(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.LoadThumbnailAsync(meta.ID);
        }

        public static Task SaveThumbnailAsync(this IAssetDatabase<Guid, string> assetDatabase, string fileID, byte[] thumbnail)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.SaveThumbnailAsync(meta.ID, thumbnail);
        }

        public static Task SaveAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, string fileID, byte[] thumbnail = null)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.SaveAssetAsync(meta.ID, thumbnail);
        }

        public static Task SaveAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, object asset, byte[] thumbnail = null)
        {
            var meta = assetDatabase.GetMeta(asset);
            return assetDatabase.SaveAssetAsync(meta.ID, thumbnail);
        }

        public static Task RenameAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, string fileID, string name)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.RenameAssetAsync(meta.ID, name);
        }

        public static Task RenameAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, object asset, string name)
        {
            var meta = assetDatabase.GetMeta(asset);
            return assetDatabase.RenameAssetAsync(meta.ID, name);
        }

        public static Task MoveAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, string fileID, string newFileID, string newDataFileID = default, string newThumbnailID = default, string newParentID = default)
        {
            var meta = assetDatabase.GetMeta(fileID);
            var parentMeta = newParentID != null ?
                assetDatabase.GetMeta(newParentID) :
                new Meta<Guid, string> { ID = default };

            return assetDatabase.MoveAssetAsync(meta.ID, newFileID, newDataFileID, newThumbnailID, parentMeta.ID);
        }

        public static Task MoveAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, object asset, string newFileID, string newDataFileID = default, string newThumbnailID = default, string newParentID = default)
        {
            var meta = assetDatabase.GetMeta(asset);
            var parentMeta = newParentID != null ?
                assetDatabase.GetMeta(newParentID) :
                new Meta<Guid, string> { ID = default };

            return assetDatabase.MoveAssetAsync(meta.ID, newFileID, newDataFileID, newThumbnailID, parentMeta.ID);
        }

        public static Task UnloadAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, string fileID, bool destroy = false)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.UnloadAssetAsync(meta.ID, destroy);
        }

        public static Task UnloadAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, object asset, bool destroy = false)
        {
            var meta = assetDatabase.GetMeta(asset);
            return assetDatabase.UnloadAssetAsync(meta.ID, destroy);
        }

        public static Task DeleteAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.DeleteAssetAsync(meta.ID);
        }

        public static Task DeleteAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, object asset)
        {
            var meta = assetDatabase.GetMeta(asset);
            return assetDatabase.DeleteAssetAsync(meta.ID);
        }

        public static Task DeleteFolderAsync(this IAssetDatabase<Guid, string> assetDatabase, Guid folderID)
        {
            var meta = assetDatabase.GetMeta(folderID);
            return assetDatabase.DeleteFolderAsync(meta.FileID);
        }

        public static Task UnloadThumbnailAsync(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.UnloadThumbnailAsync(meta.ID);
        }

        public static Task UnloadThumbnailAsync(this IAssetDatabase<Guid, string> assetDatabase, object asset)
        {
            var meta = assetDatabase.GetMeta(asset);
            return assetDatabase.UnloadThumbnailAsync(meta.ID);
        }

        public static object GetAsset(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.GetAsset(meta.ID);
        }

        public static T GetAsset<T>(this IAssetDatabase<Guid, string> assetDatabase, Guid assetID)
        {
            return (T)assetDatabase.GetAsset(assetID);
        }

        public static T GetAsset<T>(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return (T)assetDatabase.GetAsset(meta.ID);
        }

        public static Guid GetAssetIDByFileID(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            return assetDatabase.TryGetMeta(fileID, out var meta) ? meta.ID : Guid.Empty;
        }

        public static bool TryGetAssetByInstance<T>(this IAssetDatabase<Guid, string> assetDatabase, T instance, out T asset)
        {
            bool result = assetDatabase.TryGetAssetByInstance(instance, out object obj);
            asset = (T)obj;
            return result;
        }

        public static IReadOnlyCollection<object> GetInstances(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.GetInstances(meta.ID);
        }

        public static IReadOnlyCollection<object> GetInstances(this IAssetDatabase<Guid, string> assetDatabase, object asset)
        {
            var meta = assetDatabase.GetMeta(asset);
            return assetDatabase.GetInstances(meta.ID);
        }

        public static IReadOnlyCollection<T> GetInstances<T>(this IAssetDatabase<Guid, string> assetDatabase, Guid assetID)
        {
            var meta = assetDatabase.GetMeta(assetID);
            return assetDatabase.GetInstances(meta.ID).Cast<T>().ToArray();
        }

        public static IReadOnlyCollection<T> GetInstances<T>(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.GetInstances(meta.ID).Cast<T>().ToArray();
        }

        public static bool CanInstantiateAsset(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.CanInstantiateAsset(meta.ID);
        }

        public static bool CanInstantiateAsset(this IAssetDatabase<Guid, string> assetDatabase, object asset)
        {
            var meta = assetDatabase.GetMeta(asset);
            return assetDatabase.CanInstantiateAsset(meta.ID);
        }

        public static Task<object> InstantiateAssetAsync(this IAssetDatabase<Guid, string> assetDatabase, string fileID, object parent = null, bool detachInstance = false)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.InstantiateAssetAsync(meta.ID, parent, detachInstance);
        }

        public async static Task<T> InstantiateAssetAsync<T>(this IAssetDatabase<Guid, string> assetDatabase, Guid assetID, object parent = null, bool detachInstance = false)
        {
            var meta = assetDatabase.GetMeta(assetID);
            var instance = await assetDatabase.InstantiateAssetAsync(meta.ID, parent, detachInstance);
            return (T)instance;
        }

        public async static Task<T> InstantiateAssetAsync<T>(this IAssetDatabase<Guid, string> assetDatabase, string fileID, object parent = null, bool detachInstance = false)
        {
            var meta = assetDatabase.GetMeta(fileID);
            var instance = await assetDatabase.InstantiateAssetAsync(meta.ID, parent, detachInstance);
            return (T)instance;
        }

        public class IgnoreCaseComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                if (x == null || y == null)
                {
                    if (x == null && y == null)
                    {
                        return true;
                    }

                    return false;
                }

                return x.Equals(y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }

        public static readonly IgnoreCaseComparer s_ignoreCaseComparer = new IgnoreCaseComparer();

        private static string GetUniqueName(string[] names, string desiredName)
        {
            desiredName = desiredName.Trim(' ');
            if (!names.Contains(desiredName, s_ignoreCaseComparer))
            {
                return desiredName;
            }

            string ext = Path.GetExtension(desiredName);
            if (ext.Length > 0)
            {
                desiredName = desiredName.Substring(0, desiredName.Length - ext.Length);
            }

            string baseName = desiredName.TrimEnd(' ');  // Trim any trailing spaces
            int number = 1;

            // Check if the baseName ends with a number
            Match match = Regex.Match(baseName, @"^(.*?)(\d+)$");
            if (match.Success && int.TryParse(match.Groups[2].Value, out int parsedNumber))
            {
                baseName = match.Groups[1].Value.TrimEnd();
                number = parsedNumber + 1;
            }

            string uniqueName;
            do
            {
                uniqueName = $"{baseName} {number}{ext}";
                number++;
            }
            while (names.Contains(uniqueName, s_ignoreCaseComparer));

            return uniqueName;
        }

        public static string NormalizePath(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            if (string.IsNullOrWhiteSpace(fileID))
            {
                return fileID;
            }

            fileID = PathUtils.NormalizePath(fileID);

            if (Path.IsPathRooted(fileID))
            {
                return fileID;
            }

            string rootFID = assetDatabase.GetMeta(assetDatabase.RootID).FileID;
            if (fileID.StartsWith($"{rootFID}/") || string.Compare(fileID, rootFID, ignoreCase:true) == 0)
            {
                return fileID;
            }

            return $"{rootFID}/{fileID}";
        }

        public static string GetUniqueFileID(this IAssetDatabase<Guid, string> assetDatabase, string fileID)
        {
            string dir = Path.GetDirectoryName(fileID);
            if (string.IsNullOrEmpty(dir))
            {
                return assetDatabase.GetUniqueFileID(assetDatabase.RootID, fileID);
            }

            if(!assetDatabase.TryGetMeta(dir, out var meta))
            {
                return fileID;
            }

            return assetDatabase.GetUniqueFileID(meta.ID, Path.GetFileName(fileID));
        }

        public static string GetUniqueFileID(this IAssetDatabase<Guid, string> assetDatabase, Guid parentID, string desiredName)
        {
            var current = assetDatabase.GetMeta(parentID);
            var children = assetDatabase.GetChildren(parentID);
            desiredName = PathUtils.NormalizePath(Path.Combine(current.FileID, desiredName));
            if (children.Count == 0)
            {
                return $"{desiredName}";
            }

            string[] existingFileIDs = new string[children.Count];
            for (int i = 0; i < children.Count; ++i)
            {
                var meta = assetDatabase.GetMeta(children[i]);
                existingFileIDs[i] = PathUtils.NormalizePath(meta.FileID);
            }

            return $"{GetUniqueName(existingFileIDs, desiredName)}";
        }

        private static readonly AssetNamesComparer s_assetNameWithNumberComparer = new AssetNamesComparer();

        public static IEnumerable<TID> GetChildren<TID, TFID>(this IAssetDatabase<TID, TFID> assetDatabase, TID parentID, bool sortByName)
           where TID : IEquatable<TID>
           where TFID : IEquatable<TFID>
        {
            var children = assetDatabase.GetChildren(parentID);
            if (sortByName)
            {
                var folders = children.Where(childID => assetDatabase.IsFolder(childID)).OrderBy(childID => assetDatabase.GetMeta(childID).Name, s_assetNameWithNumberComparer);
                var assets = children.Where(childID => !assetDatabase.IsFolder(childID)).OrderBy(childID => assetDatabase.GetMeta(childID).Name, s_assetNameWithNumberComparer);
                return folders.Union(assets);
            }

            return children;            
        }

        public static Task DuplicateAssetAsync(this IAssetDatabase assetDatabase, string fileID, string newFileID)
        {
            var meta = assetDatabase.GetMeta(fileID);
            return assetDatabase.DuplicateAssetAsync(meta.ID, newFileID);
        }


        public static async Task<T> DeserializeAsync<T>(this IAssetDatabaseSerializer<Guid> serializer, Guid id, Stream stream)
        {
            return (T)await serializer.DeserializeAsync(id, stream);
        }

        public static void RegisterDynamicTypes(this IAssetDatabase _, params Type[] types)
        {
            SerializerExtensionUtil.RegisterDynamicTypes(types);
        }

        public static void UnregisterDynamicTypes(this IAssetDatabase _, params Type[] types)
        {
            SerializerExtensionUtil.UnregisterDynamicTypes(types);
        }

        [Obsolete("Use RegisterDynamicTypes")]
        public static void RegisterDynamicType(this IAssetDatabase _, Type type)
        {
            SerializerExtensionUtil.RegisterDynamicType(type);
        }

        [Obsolete("Use UnregisterDynamicTypes")]
        public static void UnregisterDynamicType(this IAssetDatabase _, Type type)
        {
            SerializerExtensionUtil.UnregisterDynamicType(type);
        }
    }
}