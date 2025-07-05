using Battlehub.Storage;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public class ResourcesLoaderModel : MonoBehaviour, IExternalAssetLoaderModel
    {
        private ResourcesLoader m_externalLoader = new ResourcesLoader();

        public string LoaderID
        {
            get { return nameof(ResourcesLoaderModel); }
        }

        public Task<object> LoadAsync(string key, object root, IProgress<float> progress = null)
        {
            return m_externalLoader.LoadAsync(key, root, progress);
        }

        public void Release(object obj)
        {
            m_externalLoader.Release(obj);
        }
    }
}
