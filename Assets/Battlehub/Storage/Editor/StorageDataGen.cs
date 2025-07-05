using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
namespace Battlehub.Storage
{
    public class StorageDataGen : MonoBehaviour
    {
        private static bool IsTypeModelBuildScheduled
        {
            get { return EditorPrefs.GetBool($"{typeof(StorageDataGen).FullName}.{nameof(IsTypeModelBuildScheduled)}", false); }
            set { EditorPrefs.SetBool($"{typeof(StorageDataGen).FullName}.{nameof(IsTypeModelBuildScheduled)}", value); }
        }

        [MenuItem("Tools/Runtime Asset Database/Build All", priority = 0)]
        public static void Generate()
        {
            EditorUtility.DisplayProgressBar("Build All", "Creating Data", 0.0f);
            try
            {
                StorageSerializerGen.Generate();
                ObjectEnumeratorFactoryGen.Generate();
                ModuleDependenciesGen.Generate();
                AssetDatabaseHostGen.Generate();
                RTShaderProfilesGen.Generate();
                ExternalAssetListGen.GenerateBuiltInAssetsList();

                IsTypeModelBuildScheduled = true;

                TypeModelBuilder.DeleteTypeModel();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                var generatedDataFolder = AssetDatabase.LoadAssetAtPath(StoragePath.GeneratedDataFolder, typeof(Object));
                Selection.activeObject = generatedDataFolder;
                EditorGUIUtility.PingObject(Selection.activeObject);

                EditorUtility.RequestScriptReload();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }   
        }


        [MenuItem("Tools/Runtime Asset Database/Clean", priority = 0)]
        public static void Clean()
        {
            if (!EditorUtility.DisplayDialog("Clean", $"Delete scripts and assets from Assets/Battlehub/StorageData/Generated? {System.Environment.NewLine} {System.Environment.NewLine}You can restore deleted scripts and assets using the Build All command.", "Delete", "Cancel"))
            {
                return;
            }

            string dir = StoragePath.GeneratedDataFolder;
            Directory.Delete(dir, true);
            AssetDatabaseHostGen.GenerateEmpty();
            ModuleDependenciesGen.GenerateEmpty();
            StorageSerializerGen.GenerateEmpty();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }

        [DidReloadScripts]
        public static void ScriptsReloaded()
        {
            if (IsTypeModelBuildScheduled)
            {
                IsTypeModelBuildScheduled = false;

                EditorUtility.DisplayProgressBar("Build All", "Building TypeModel", 0.5f);
                try
                {
                    TypeModelBuilder.BuildTypeModel();
                    
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }
    }
}
