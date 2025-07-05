using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class ResourcesImportSourceModel : MonoBehaviour, IImportSourceModel
    {
        [SerializeField]
        private bool m_isEnabled = true;

        public bool IsEnabled
        {
            get { return m_isEnabled; }
        }

        public int SortIndex
        {
            get { return 20; }
        }

        public string DisplayName
        {
            get { return IOC.Resolve<ILocalization>().GetString("ID_RTE_ResourcesImportSourceModel", "Resources"); }
        }

        [SerializeField]
        private string m_loaderID = nameof(ResourcesLoaderModel);
        public string LoaderID
        {
            get { return m_loaderID; }
        }

        [SerializeField]
        private ImportGroup[] m_groups = new[]
        {
            new ImportGroup(
                key: "Importable",
                name: "Default")
        };

        [SerializeField]
        private ImportAsset[] m_assets = new[]
        {
            new ImportAsset(
                groupKey: "Importable",
                key: "Samples/SampleAsset",
                name: "SampleAsset")
        };

        public void SetGroups(ImportGroup[] groups)
        {
            m_groups = groups;
        }

        public void SetAssets(ImportAsset[] assets)
        {
            m_assets = assets;
        }

        public Task<IImportGroup[]> GetGroupsAsync()
        {
            IImportGroup[] groups = m_groups;
            return Task.FromResult(groups);
        }

        public Task<IImportAsset[]> GetAssetsAsync(string groupKey)
        {
            IImportAsset[] result = ImportAsset.BuildTree(groupKey, m_assets);
            return Task.FromResult(result);
        }
    }
}
