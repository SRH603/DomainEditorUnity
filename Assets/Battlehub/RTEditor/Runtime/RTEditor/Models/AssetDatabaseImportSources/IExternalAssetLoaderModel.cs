using System;
using System.Threading.Tasks;

namespace Battlehub.RTEditor.Models
{
    public interface IExternalAssetLoaderModel 
    {
        string LoaderID
        {
            get;
        }

        Task<object> LoadAsync(string key, object root, IProgress<float> progress = null);

        void Release(object obj);
    }

}
