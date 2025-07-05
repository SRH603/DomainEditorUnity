using System;
using System.Threading.Tasks;
#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif
namespace Battlehub.Storage
{
    using UnityObject = UnityEngine.Object;

    public class AddressablesLoader : IExternalAssetLoader
    {
#if UNITY_ADDRESSABLES
        public async Task<object> LoadAsync(string key, object root, IProgress<float> progress = null)
        {

            var ao = Addressables.LoadAssetAsync<UnityObject>(key);
            while (!ao.IsDone)
            {
                await Task.Yield();
                if (progress != null)
                {
                    progress.Report(ao.PercentComplete);
                }
            }
            return ao.Result;
        }

        public void Release(object obj)
        {
            Addressables.Release(obj);
        }
#else
        public Task<object> LoadAsync(string key, object root, IProgress<float> progress = null)
        {
            throw new InvalidOperationException("Install com.unity.addressables using Package Manager");
        }

        public void Release(object obj)
        {
        }
#endif
    }
}

