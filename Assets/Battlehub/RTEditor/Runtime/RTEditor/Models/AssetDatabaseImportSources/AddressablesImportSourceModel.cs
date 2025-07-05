using Battlehub.RTCommon;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ADDRESSABLES
using System.Collections.Generic;
using System.IO;
using UnityEngine.AddressableAssets;
#endif

namespace Battlehub.RTEditor.Models
{
    public class AddressablesImportSourceModel : MonoBehaviour, IImportSourceModel
    {
#if UNITY_ADDRESSABLES
        [SerializeField]
        private bool m_isEnabled = true;
#endif
        public bool IsEnabled
        {
            get
            {
#if UNITY_ADDRESSABLES
                return m_isEnabled;
#else
                return false;
#endif
            }
        }

        public int SortIndex
        {
            get { return 10; }
        }

        public string DisplayName
        {
            get { return IOC.Resolve<ILocalization>().GetString("ID_RTE_AddressablesImportSourceModel", "Addressables"); }
        }

        [SerializeField]
        private string m_loaderID = nameof(AddressablesLoaderModel);
        public string LoaderID
        {
            get { return m_loaderID; }
        }

        [SerializeField]
        private ImportGroup[] m_groups = new[]
        {
            new ImportGroup
            {
                Name = "Default",
                Key = "default"
            }
        };

        public void SetGroups(ImportGroup[] groups)
        {
            m_groups = groups;
        }

        public Task<IImportGroup[]> GetGroupsAsync()
        {
            IImportGroup[] groups = m_groups;
            return Task.FromResult(groups);
        }

        public async Task<IImportAsset[]> GetAssetsAsync(string groupKey)
        {
#if UNITY_ADDRESSABLES
            var loadResourceLocationsHandle = Addressables.LoadResourceLocationsAsync(groupKey, typeof(Object));
            await loadResourceLocationsHandle.Task;               

            var assets = new List<ImportAsset>();
            foreach (var location in loadResourceLocationsHandle.Result)
            {
                assets.Add(new ImportAsset(groupKey, location.PrimaryKey.ToString(), Path.GetFileNameWithoutExtension(location.PrimaryKey.ToString())));
            }

            return ImportAsset.BuildTree(groupKey, assets);
#else
            await Task.Yield();
            throw new System.NotImplementedException("Import com.unity.addressables to use this function");
#endif

        }
    }

}
