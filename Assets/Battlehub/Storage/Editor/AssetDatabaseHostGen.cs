using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Battlehub.Storage
{
    public class AssetDatabaseHostGen
    {
        [MenuItem("Tools/Runtime Asset Database/Create Host", priority = 1)]
        public static void Create()
        {
            var assetDatabaseHostTypeName = Type.GetType(RuntimeAssetDatabase.AssetDatabaseHostTypeName);
            if (assetDatabaseHostTypeName == null)
            {
                Debug.LogError("Cannot find script Battlehub.Storage.RuntimeAssetDatabaseHost. Click Tools->Runtime Asset Database->Build All");
                return;
            }

#if UNITY_2023_1_OR_NEWER
            if (UnityEngine.Object.FindAnyObjectByType(assetDatabaseHostTypeName, FindObjectsInactive.Exclude) != null)
#else
            if (UnityEngine.Object.FindObjectsOfType(assetDatabaseHostTypeName).Length > 0)
#endif
            {
                Debug.LogWarning("Battlehub.Storage.RuntimeAssetDatabaseHost already exists");
                return;
            }

            GameObject assetDatabaseHost = new GameObject("RuntimeAssetDatabaseHost");
            var host = assetDatabaseHost.AddComponent(assetDatabaseHostTypeName);

            var externalAssetList = AssetDatabase.LoadAssetAtPath<ExternalAssetList>(ExternalAssetListGen.DefaultPath);
            if (externalAssetList != null)
            {
                var externalAssetsProperty = host.GetType().GetProperty("ExternalAssets");
                if (externalAssetsProperty != null && externalAssetsProperty.GetSetMethod() != null)
                {
                    externalAssetsProperty.SetValue(host, new ExternalAssetList[] { externalAssetList });
                }
            }
        }

        private const string m_template =
@"using UnityEngine;

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
";


        private const string m_emptyTemplate =
@"using UnityEngine;

namespace Battlehub.Storage
{
    [DefaultExecutionOrder(-100)]
    public class RuntimeAssetDatabaseHost : MonoBehaviour
    {
        [SerializeField]
        private ExternalAssetList[] m_externalAssets;
        public ExternalAssetList[] ExternalAssets
        {
            get { return m_externalAssets; }
            set { m_externalAssets = value; }
        }
    }
}
";
        public static void Generate()
        {
            string dir = $"{StoragePath.GeneratedDataFolder}";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = $"{dir}/RuntimeAssetDatabaseHost.cs";

            File.WriteAllText(path, m_template);
        }

        public static void GenerateEmpty()
        {
            string dir = $"{StoragePath.GeneratedDataFolder}";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string path = $"{dir}/RuntimeAssetDatabaseHost.cs";

            File.WriteAllText(path, m_emptyTemplate);
        }
    }
}