
using System;

namespace Battlehub.Storage
{
    public sealed class RuntimeAssetEnumerator : AssetEnumerator<Guid, string>
    {
        public RuntimeAssetEnumerator(object obj) : base(RuntimeAssetDatabase.Deps, obj) { }
    }
}
