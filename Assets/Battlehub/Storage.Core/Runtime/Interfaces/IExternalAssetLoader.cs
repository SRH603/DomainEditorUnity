using System;
using System.Threading.Tasks;

namespace Battlehub.Storage
{
    public interface IExternalIDMap<TID>
    {
        bool TryGetID(object obj, out TID id);
    }

    public interface IExternalAssetLoader 
    {
        Task<object> LoadAsync(string key, object root, IProgress<float> progress = null);
        
        void Release(object obj);
    }
}

