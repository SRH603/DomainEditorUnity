using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public interface IImportGroup
    {
        public string Key
        {
            get;
        }

        public string Name
        {
            get;
        }
    }

    [Serializable]
    public class ImportGroup : IImportGroup
    {
        [SerializeField]
        private string m_key;

        public string Key 
        {
            get { return m_key; }
            set { m_key = value; }
        }

        [SerializeField]
        private string m_name;
        public string Name 
        { 
            get { return m_name; }
            set { m_name = value; }
        }

        public ImportGroup()
        {
        }

        public ImportGroup(string key, string name)
        {
            m_key = key;
            m_name = name;
        }
    }

    public interface IImportAsset
    {
        string GroupKey
        {
            get;
        }

        object Key
        {
            get;
        }

        string Name
        {
            get;
        }

        Texture Thumbnail
        {
            get;
            set;
        }

        bool IsFolder
        {
            get;
        }

        IImportAsset Self
        {
            get;
        }

        IImportAsset Parent
        {
            get;
        }

        IList<IImportAsset> Children
        {
            get;
        }
    }

    public static class IImportAssetExtensions
    {
        public static IImportAsset[] Flatten(this IImportAsset asset, bool excludeFolders, bool excludeAssets = false)
        {
            var items = new List<IImportAsset>();
            Foreach(asset, item =>
            {
                if (excludeFolders && item.IsFolder)
                {
                    return;
                }

                if (excludeAssets && !item.IsFolder)
                {
                    return;
                }

                items.Add(item);
            });
            return items.ToArray();
        }

        public static void Foreach(this IImportAsset item, Action<IImportAsset> callback)
        {
            if (item == null)
            {
                return;
            }

            callback(item);

            if (item.Children != null)
            {
                for (int i = 0; i < item.Children.Count; ++i)
                {
                    IImportAsset child = item.Children[i];
                    Foreach(child, callback);
                }
            }
        }
    }

    [Serializable]
    public class ImportAsset : INotifyPropertyChanged, IImportAsset
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [SerializeField]
        private string m_groupKey;
        public string GroupKey
        { 
            get { return m_groupKey; }
            set { m_groupKey = value; }
        }

        [SerializeField]
        private string m_key;
        public virtual object Key 
        { 
            get { return m_key; }
        }

        [SerializeField]
        private string m_name;
        public string Name 
        { 
            get { return m_name; }
            set { m_name = value; }
        }


        [NonSerialized]
        private Texture m_thumbnail;
        public Texture Thumbnail
        {
            get { return m_thumbnail; }
            set 
            {
                if (m_thumbnail != value)
                {
                    m_thumbnail = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Thumbnail)));
                }
            }
        }

        public ImportAsset()
        {
        }

        public ImportAsset(string groupKey, string key, string name, IImportAsset parent = null)
        {
            m_groupKey = groupKey;
            m_key = key;
            m_name = name;
            m_parent = parent;
        }

        public enum ImportStatus
        {
            None,
            New,
            Conflict,
            Overwrite
        }

        private ImportStatus m_status;
        public ImportStatus Status
        {
            get { return m_status; }
            set { m_status = value; }
        }

        public bool IsFolder
        {
            get { return Children != null; }
        }

        public IImportAsset Self
        {
            get { return this; }
        }

        [NonSerialized]
        private IImportAsset m_parent;
        public IImportAsset Parent
        {
            get { return m_parent; }
            set { m_parent = value; }
        }

        [NonSerialized]
        private IList<IImportAsset> m_children;
        public IList<IImportAsset> Children
        {
            get { return m_children; }
            set { m_children = value; }
        }

        public static ImportAsset[] BuildTree(string groupKey, IList<ImportAsset> assets)
        {
            var rootAssets = new HashSet<ImportAsset>();
            var pathToAsset = new Dictionary<string, ImportAsset>();

            for (int i = 0; i < assets.Count; ++i)
            {
                var asset = assets[i];
                if (asset.GroupKey != groupKey)
                {
                    continue;
                }

                pathToAsset.Add(asset.Key.ToString(), asset);

                while (true)
                {
                    string dirName = Path.GetDirectoryName(asset.Key.ToString()); 
                    if (string.IsNullOrEmpty(dirName))
                    {
                        rootAssets.Add(asset);
                        break;
                    }
                    else
                    {
                        if (!pathToAsset.TryGetValue(dirName, out var folder))
                        {
                            folder = new ImportAsset(groupKey, dirName, Path.GetFileName(dirName))
                            {
                                Children = new List<IImportAsset>()
                            };
                            pathToAsset.Add(dirName, folder);
                        }

                        if (folder.Children.Contains(asset))
                        {
                            break;
                        }

                        folder.Children.Add(asset);
                        asset.Parent = folder;
                        asset = folder;
                    }
                }
            }

            return rootAssets.ToArray();
        }
    }


    public interface IImportSourceModel
    {
        bool IsEnabled { get; }

        int SortIndex { get; }

        string DisplayName { get; }

        string LoaderID { get; }

        Task<IImportGroup[]> GetGroupsAsync();

        Task<IImportAsset[]> GetAssetsAsync(string groupKey);
    }
}
