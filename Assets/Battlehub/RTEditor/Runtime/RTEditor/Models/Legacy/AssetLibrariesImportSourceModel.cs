using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public class AssetLibrariesImportSourceModel : MonoBehaviour, IImportSourceModel
    {
        private class AssetLibraryImportAsset : ImportAsset
        {
            public override object Key
            {
                get;
            }

            public AssetLibraryImportAsset(string groupKey, ProjectItem key)
            {
                Key = key;
                Name = key.Name;
                GroupKey = groupKey;
            }


        }

        [SerializeField]
        private bool m_isBuitIn = true;

        [SerializeField]
        private bool m_isEnabled = true;

        public bool IsEnabled
        {
            get { return m_isEnabled; }
        }

        [SerializeField]
        private int m_sortIndex = 100;
        public int SortIndex
        {
            get { return m_sortIndex; }
        }

        public string DisplayName
        {
            get 
            {
                var lc = IOC.Resolve<ILocalization>();
                return m_isBuitIn ?
                    lc.GetString("ID_RTEditor_AssetLibSelectDialog_BuiltIn", "Built-in") :
                    lc.GetString("ID_RTEditor_AssetLibSelectDialog_External", "External");
            }
        }

        public string LoaderID
        {
            get { return string.Empty; }
        }

        private IProjectAsync m_project;

        private void Awake()
        {
            m_project = IOC.Resolve<IProjectAsync>();
        }

        private void OnDestroy()
        {
            m_project = null;
        }

        public async Task<IImportGroup[]> GetGroupsAsync()
        {
            string[] libraries =
                m_isBuitIn ?
                await m_project.GetStaticAssetLibrariesAsync() :
                await m_project.GetAssetBundlesAsync();

            return libraries.Distinct().Select(lib => new ImportGroup(lib, lib)).ToArray();
        }

        public async Task<IImportAsset[]> GetAssetsAsync(string groupKey)
        {
            var rootItem = await m_project.Safe.LoadImportItemsAsync(groupKey, m_isBuitIn);
            var rootImportAsset = new AssetLibraryImportAsset(groupKey, rootItem);

            var importAssets = new List<AssetLibraryImportAsset>();
            if (rootItem != null && rootItem.Children != null && rootItem.Children.Where(ProjectItemPassesFilter).Count() > 0)
            {
                BuildTree(groupKey, rootItem, rootImportAsset, importAssets);

                var editor = IOC.Resolve<IRuntimeEditor>();
               
                await CreateThumbnailsAsync(importAssets, editor.ThumbnailUtil);

                m_project.UnloadImportItems(rootItem);
            }

            var result = importAssets.Where(importAsset => importAsset.Parent == rootImportAsset).ToArray();
            for (int i = 0; i < result.Length; ++i)
            {
                result[i].Parent = null;
            }

            return result;
        }

        private async Task CreateThumbnailsAsync(List<AssetLibraryImportAsset> importAssets, IAssetThumbnailUtil thumbnailUtil)
        {
            foreach(var importAsset in importAssets) 
            {
                await CreateThumbnailAsync(importAsset, thumbnailUtil);
            }
        }

        private async Task CreateThumbnailAsync(AssetLibraryImportAsset importAsset, IAssetThumbnailUtil thumbnailUtil)
        {
            var importAssetItem = importAsset.Key as ImportAssetItem;
            if (importAssetItem == null)
            {
                return;
            }

            if (importAssetItem.Object != null)
            {
                var thumbnail = await thumbnailUtil.CreateThumbnailAsync(importAssetItem.Object);
                var thumbnailBytes = await thumbnailUtil.EncodeToPngAsync(thumbnail);

                importAsset.Thumbnail = thumbnail;
                importAssetItem.SetPreview(thumbnailBytes != null ? thumbnailBytes : new byte[0]);
            }
        }

        private void BuildTree(string groupKey, ProjectItem projectItem, AssetLibraryImportAsset importAsset, List<AssetLibraryImportAsset> importAssets)
        {
            if(projectItem.Children != null && projectItem.Children.Count > 0)
            {
                importAsset.Children = new List<IImportAsset>();

                for (int i = 0; i <  projectItem.Children.Count; i++) 
                {
                    int status = (int)ImportStatus.None;
                    if (projectItem is ImportAssetItem)
                    {
                        var importAssetItem = (ImportAssetItem)projectItem;
                        status = (int)importAssetItem.Status;
                    }
                    
                    var childProjectItem = projectItem.Children[i];
                    var childImportAsset = new AssetLibraryImportAsset(groupKey, childProjectItem)
                    {
                       Parent = importAsset,
                       Status = (ImportAsset.ImportStatus)status,
                    };
                    BuildTree(groupKey, childProjectItem, childImportAsset, importAssets);
                    importAsset.Children.Add(childImportAsset);
                    importAssets.Add(childImportAsset);
                }
            }
        }

        protected virtual bool ProjectItemPassesFilter(ProjectItem p)
        {
            return true;
        }

    }

}
