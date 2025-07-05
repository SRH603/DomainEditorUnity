using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace Battlehub.Storage
{
    public class ExternalAssetListGen 
    {
        public static string DefaultPath
        {
            get
            {
                string dir = $"{StoragePath.DataFolder}";
                string path = $"{dir}/ExternalAssetList.asset";
                return path;
            }
        }

        private const string k_urpPackagePath = "Packages/com.unity.render-pipelines.universal";
        private const string k_urpPackageMaterials = k_urpPackagePath + "/Runtime/Materials";

        public readonly static Dictionary<(string Path, Type Type), Guid> UniversalRPResources = new Dictionary<(string, Type), Guid>
        {
            { ($"{k_urpPackageMaterials}/Lit.mat",                  typeof(Material)), new Guid("0b29964a-399f-4450-9fa4-ae85f267f823") },
            { ($"{k_urpPackageMaterials}/ParticlesUnlit.mat",       typeof(Material)), new Guid("9ebdfa5f-a7b3-4dd4-b9f4-61c9c4117ea7") },
            { ($"{k_urpPackageMaterials}/Sprite-Unlit-Default.mat", typeof(Material)), new Guid("d3447f7e-a09a-4df5-890c-a647b50ca103") },
            { ($"{k_urpPackageMaterials}/SpriteMask-Default.mat",   typeof(Material)), new Guid("2c73eafa-5108-4d56-8698-7ec4e2d44251") },
            { ($"{k_urpPackageMaterials}/TerrainLit.mat",           typeof(Material)), new Guid("c70e9f14-b33d-4e6c-a5c9-dc5c1d3d7d5f") },
            { ($"{k_urpPackageMaterials}/Sprite-Lit-Default.mat",   typeof(Material)), new Guid("1f18c172-7d50-401b-8955-d7e5ec89b1b0") },
            { ($"{k_urpPackageMaterials}/Decal.mat",                typeof(Material)), new Guid("108b8eb4-b637-40ad-a926-baceca09b326") },
        };

        public readonly static Dictionary<(string Path, Type Type), Guid> BuiltInRPResources = new Dictionary<(string, Type), Guid>
        {
            { ("Default-Material.mat",         typeof(Material)), new Guid("0b29964a-399f-4450-9fa4-ae85f267f823") },
            { ("Default-Particle.mat",         typeof(Material)), new Guid("9ebdfa5f-a7b3-4dd4-b9f4-61c9c4117ea7") },
            { ("Sprites-Default.mat",          typeof(Material)), new Guid("d3447f7e-a09a-4df5-890c-a647b50ca103") },
            { ("Sprites-Mask.mat",             typeof(Material)), new Guid("2c73eafa-5108-4d56-8698-7ec4e2d44251") },
            { ("Default-Terrain-Standard.mat", typeof(Material)), new Guid("c70e9f14-b33d-4e6c-a5c9-dc5c1d3d7d5f") },
            { ("Default-Skybox.mat",           typeof(Material)), new Guid("5f6fba89-3dd1-449f-a9ca-dd04711d22c3") },
        };

        public readonly static Dictionary<(string Path, Type Type), Guid> BuiltInExtraResources = new Dictionary<(string, Type), Guid>
        {
            { ("Default-Line.mat",                typeof(Material)),  new Guid("7fffe78a-5764-4d26-bf0f-1498f02dc3a9") },
            { ("UI/Skin/Background.psd",          typeof(Sprite)),    new Guid("2e7446b8-daa3-481b-8ee6-88cf37d996c6") },
            { ("UI/Skin/Checkmark.psd",           typeof(Sprite)),    new Guid("7cd1ab8b-366d-4f91-8a66-b65f7b1ad736") },
            { ("UI/Skin/DropdownArrow.psd",       typeof(Sprite)),    new Guid("f506eda3-f89a-478b-a6af-41014567b2f8") },
            { ("UI/Skin/InputFieldBackground.psd",typeof(Sprite)),    new Guid("f93f70ac-d490-40ab-8e7d-9ef98f2a6db9") },
            { ("UI/Skin/Knob.psd",                typeof(Sprite)),    new Guid("6f5d2b76-da03-4609-bb48-680bbd031169") },
            { ("UI/Skin/UIMask.psd",              typeof(Sprite)),    new Guid("a800d318-be4a-4ea5-a4e4-a5f214f09e16") },
            { ("UI/Skin/UISprite.psd",            typeof(Sprite)),    new Guid("dfef2a5b-4d7a-4e03-9053-c77d21542e26") },
            { ("UI/Skin/Background.psd",          typeof(Texture2D)), new Guid("eabf40c4-31c4-4fbd-aa46-392d27494b79") },
            { ("UI/Skin/Checkmark.psd",           typeof(Texture2D)), new Guid("40a5c5fe-7e8f-4378-b4b2-380ccc85fe7c") },
            { ("UI/Skin/DropdownArrow.psd",       typeof(Texture2D)), new Guid("579d1f00-92c3-4798-9ef8-b5d1433f5a5e") },
            { ("UI/Skin/InputFieldBackground.psd",typeof(Texture2D)), new Guid("e400e392-c84d-4644-8baf-83c197173346") },
            { ("UI/Skin/Knob.psd",                typeof(Texture2D)), new Guid("30a3c096-6d97-400c-bca9-929416caa868") },
            { ("UI/Skin/UIMask.psd",              typeof(Texture2D)), new Guid("7720f30b-8c29-4d92-9f16-6f88a66629da") },
            { ("UI/Skin/UISprite.psd",            typeof(Texture2D)), new Guid("b4057401-ca99-4ef4-8188-61dacbb61f4e") },
            { ("Default-Particle.psd",            typeof(Texture2D)), new Guid("bb958e1c-fe9d-4794-87ce-304c736af8e4") },
        };

        public readonly static Dictionary<(string Path, Type Type), Guid> BuiltInResources = new Dictionary<(string, Type), Guid>
        {
            { ("New-Sphere.fbx",                  typeof(Mesh)), new Guid("bb887824-3a6d-4701-9efe-e9712c04b9cb") },
            { ("New-Capsule.fbx",                 typeof(Mesh)), new Guid("11f23d9b-7a42-4e32-9a01-6a2ca5b055ec") },
            { ("New-Cylinder.fbx",                typeof(Mesh)), new Guid("35053dc8-c835-4494-8206-8f044ecc22d7") },
            { ("Cube.fbx",                        typeof(Mesh)), new Guid("a414329d-676b-42bd-83c1-e0846db8daaf") },
            { ("New-Plane.fbx",                   typeof(Mesh)), new Guid("e0dc1910-2d83-40eb-be33-6a0a10b6f3b3") },
            { ("Quad.fbx",                        typeof(Mesh)), new Guid("22476072-3d7a-46f2-bf8e-d5cfbd1bd68d") },
#if UNITY_2022_3_OR_NEWER
            { ("LegacyRuntime.ttf",               typeof(Font)), new Guid("ee08e9ed-c1c6-4305-a1a0-d18c1cb44d6a") },
#else
            { ("Arial.ttf",                       typeof(Font)), new Guid("ee08e9ed-c1c6-4305-a1a0-d18c1cb44d6a") },
#endif
        };

        public readonly static Dictionary<(string Path, Type Type), Guid> ProjectResources = new Dictionary<(string, Type), Guid>();

        public readonly static Dictionary<(string Path, Type Type), Guid > ProjectAssets = new Dictionary<(string, Type), Guid>();

        private static Dictionary<UnityObject, Guid> GetBuiltInAssets()
        {
            Dictionary<UnityObject, Guid> builtInAssets = new Dictionary<UnityObject, Guid>();

            if (RenderPipelineInfo.Type == RPType.URP)
            {
                foreach (var kvp in UniversalRPResources)
                {
                    UnityObject obj = AssetDatabase.LoadAssetAtPath(kvp.Key.Path, kvp.Key.Type);
                    if (obj != null)
                    {
                        builtInAssets.Add(obj, kvp.Value);
                    }
                }
            }
            else
            {
                foreach (var kvp in BuiltInRPResources)
                {
                    UnityObject obj = AssetDatabase.GetBuiltinExtraResource(kvp.Key.Type, kvp.Key.Path);
                    if (obj != null)
                    {
                        builtInAssets.Add(obj, kvp.Value);
                    }
                }
            }

            foreach (var kvp in BuiltInExtraResources)
            {
                UnityObject obj = AssetDatabase.GetBuiltinExtraResource(kvp.Key.Type, kvp.Key.Path);
                if (obj != null)
                {
                    builtInAssets.Add(obj, kvp.Value);
                }
            }

            foreach (var kvp in BuiltInResources)
            {
                UnityObject obj = Resources.GetBuiltinResource(kvp.Key.Type, kvp.Key.Path);
                if (obj != null)
                {
                    builtInAssets.Add(obj, kvp.Value);
                }
            }

            foreach (var kvp in ProjectResources)
            {
                UnityObject obj = Resources.Load(kvp.Key.Path, kvp.Key.Type);
                if (obj != null)
                {
                    builtInAssets.Add(obj, kvp.Value);
                }
            }

            foreach (var kvp in ProjectAssets)
            {
                UnityObject obj = AssetDatabase.LoadAssetAtPath(kvp.Key.Path, kvp.Key.Type);
                if (obj != null)
                {
                    builtInAssets.Add(obj, kvp.Value);
                }
            }

            return builtInAssets;
        }


        public static void GenerateBuiltInAssetsList()
        {
            Dictionary<UnityObject, Guid> builtInAssets = GetBuiltInAssets();

            Generate(builtInAssets, DefaultPath);
        }

        //[MenuItem("Tools/Runtime Asset Database/Create Scene Assets List")]
        public static void GenerateSceneAssetsList()
        {
            GenerateAssetsList(SceneManager.GetActiveScene());
        }

        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";

            foreach (UnityObject obj in Selection.GetFiltered(typeof(UnityObject), UnityEditor.SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        public static ExternalAssetList GenerateAssetsList(params object[] roots)
        {
            var moduleDependenicesType = Type.GetType(RuntimeAssetDatabase.ModuleDependenciesTypeName);
            if (moduleDependenicesType == null)
            {
                Debug.LogError($"Cannot find script {RuntimeAssetDatabase.ModuleDependenciesTypeName}. Click Tools->Runtime Asset Database->Build All");
                return null;
            }

            using var deps = (DefaultModuleDependencies)Activator.CreateInstance(moduleDependenicesType);
            using var assetDatabase = new RuntimeAssetDatabase(deps);
            using var serializerRef = deps.AcquireSerializerRef();

            var builtInAssets = GetBuiltInAssets();
            var activeScene = SceneManager.GetActiveScene();
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Scene Asset List", 
                $"{activeScene.name} Asset List", "asset", 
                "Create a list of assets referenced by this scene", 
                GetSelectedPathOrFallback());
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var assets = new Dictionary<UnityObject, Guid>();
            var assetList = AssetDatabase.LoadAssetAtPath<ExternalAssetList>(path);
            if (assetList != null)
            {
                assets = assetList.Items.ToDictionary(item => item.Asset, item => item.ID);
            }

            foreach (object root in roots)
            {
                foreach (object obj in new RuntimeAssetEnumerable(root))
                {
                    if (obj is not UnityObject)
                    {
                        continue;
                    }

                    if (obj is Component)
                    {
                        continue;
                    }

                    if (obj is GameObject)
                    {
                        continue;
                    }

                    var asset = (UnityObject)obj;
                    if (asset == null)
                    {
                        continue;
                    }

                    if (asset.name == "Default UI Material")
                    {
                        continue;
                    }

                    if (builtInAssets.ContainsKey(asset))
                    {
                        continue;
                    }

                    if (assetDatabase.GetAssetTypeIDByType(asset.GetType()) == Guid.Empty)
                    {
                        continue;
                    }

                    if (assets.ContainsKey(asset))
                    {
                        continue;
                    }

                    assets.Add(asset, Guid.NewGuid());
                }
            }

            return Generate(assets, path);
        }

        public static ExternalAssetList Generate(IDictionary<UnityObject, Guid> assets, string path)
        {
            var items = new List<ExternalAssetListItem>();
            foreach (var kvp in assets)
            {
                items.Add(new ExternalAssetListItem
                {
                    Asset = kvp.Key,
                    ID = kvp.Value
                });
            }

            var assetList = AssetDatabase.LoadAssetAtPath<ExternalAssetList>(path);
            if (assetList == null)
            {
                assetList = ScriptableObject.CreateInstance<ExternalAssetList>();
                AssetDatabase.CreateAsset(assetList, path);
            }

            assetList.Items = items.ToArray();
            EditorUtility.SetDirty(assetList);

            string dir = $"{StoragePath.GeneratedDataFolder}";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            AssetDatabase.SaveAssets();
            assetList.OnValidate();
            return assetList;
        }

    }
}
