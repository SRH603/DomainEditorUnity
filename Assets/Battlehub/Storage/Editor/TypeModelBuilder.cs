using ProtoBuf.Meta;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Battlehub.Storage
{
    public class TypeModelBuilder 
    {
        //[MenuItem("Tools/Runtime Asset Database/Build Type Model")]
        public static void BuildTypeModel()
        {
            string typeModelDll = $"{StoragePath.TypeModel}.dll";
            
            string dir = Path.GetFullPath(StoragePath.GeneratedDataFolder);
            if (!Directory.Exists(Path.GetFullPath(dir)))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(StoragePath.GeneratedDataFolder), Path.GetFileName(StoragePath.GeneratedDataFolder));
            }

            var type = ReflectionHelpers.GetAllTypesImplementingInterface(typeof(DefaultModuleDependencies))
                .Where(t => t.Name == "ModuleDependencies")
                .FirstOrDefault();

            using var deps = (DefaultModuleDependencies)Activator.CreateInstance(type);
            var serializer = deps.AcquireSerializer();
            
            var model = SerializerBase<Guid, string>.RuntimeTypeModel;
            model.Compile(new RuntimeTypeModel.CompilerOptions() { OutputPath = typeModelDll, TypeName = $"Battlehub.Storage.{StoragePath.TypeModel}" });

            deps.ReleaseSerializer(serializer);

            string srcPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("Assets")) + typeModelDll;
            string dstPath = Path.GetFullPath(StoragePath.GeneratedDataFolder + "/" + typeModelDll);
            Debug.LogFormat("Done! Move {0} to {1} ...", srcPath, dstPath);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"<assembly fullname=\"{StoragePath.TypeModel}\" preserve=\"all\"/>");

            const string linkFileTemplate = "<linker>{0}</linker>";
            File.WriteAllText(Path.GetFullPath(StoragePath.GeneratedDataFolder + "/link.xml"), string.Format(linkFileTemplate, sb.ToString()));
            File.Delete(dstPath);
            File.Move(srcPath, dstPath);

            AssetDatabase.Refresh();
        }

        public static void DeleteTypeModel()
        {
            string typeModelDll = $"{StoragePath.TypeModel}.dll";
            string dir = Path.GetFullPath(StoragePath.GeneratedDataFolder);
            File.Delete($"{dir}/{typeModelDll}");
            File.Delete($"{dir}/{typeModelDll}.meta");
            AssetDatabase.Refresh();
        }

    }

}
