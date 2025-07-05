using System.Collections;

namespace Battlehub.Storage
{
    public sealed class RuntimeAssetEnumerable : IEnumerable
    {
        private object m_asset;

        public RuntimeAssetEnumerable(object asset)
        {
            m_asset = asset;
        }

        public IEnumerator GetEnumerator()
        {
            return new RuntimeAssetEnumerator(m_asset);
        }
    }
}