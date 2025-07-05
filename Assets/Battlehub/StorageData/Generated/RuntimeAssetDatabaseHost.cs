using UnityEngine;

namespace Battlehub.Storage
{
    [DefaultExecutionOrder(-100)]
    public class RuntimeAssetDatabaseHost : MonoBehaviour
    {
        private ModuleDependencies m_deps;
        private RuntimeAssetDatabase m_assetDatabase;

        [SerializeField]
        private ExternalAssetList[] m_externalAssets;
        public ExternalAssetList[] ExternalAssets
        {
            get { return m_externalAssets; }
            set { m_externalAssets = value; }
        }

        private void Awake()
        {
            m_deps = new ModuleDependencies(gameObject);
            m_assetDatabase = new RuntimeAssetDatabase(m_deps, m_externalAssets);     
        }

        private void OnDestroy()
        {
            m_assetDatabase.Dispose();
            m_deps.Dispose();
        }
    }
}
