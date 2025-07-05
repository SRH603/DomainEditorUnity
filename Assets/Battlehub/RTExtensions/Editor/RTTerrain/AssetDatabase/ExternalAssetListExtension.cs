using Battlehub.RTCommon;
using Battlehub.Storage;
using System;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class ExternalAssetListExtension
    {
        [InitializeOnLoadMethod]
        public static void Register()
        {
            string packageContentPath = BHRoot.PackageRuntimeContentPath;

            var assets = ExternalAssetListGen.ProjectAssets;
            assets.Add(($"{packageContentPath}/RTTerrain/Resources/Tree/RTT_DefaultTree.fbx", typeof(Mesh)), new Guid("b1121e4e-3627-40e9-a309-e81140ea5c7a"));

            var res = ExternalAssetListGen.ProjectResources;
            res.Add(("Tree/RTT_DefaultTree", typeof(GameObject)), new Guid("517645b6-feca-44ec-9913-daadbf28602f"));
      
            if(RenderPipelineInfo.Type == RPType.URP)
            {
                res.Add(("Tree/Materials/RTT_DefaultTreeBark_URP", typeof(Material)), new Guid("a5c14895-de07-414e-9567-cbd3526f9bc2"));
                res.Add(("Tree/Materials/RTT_DefaultTreeBranches_URP", typeof(Material)), new Guid("d8c1144f-59aa-48f1-8dd9-92567a505817"));
            }
            else
            {
                res.Add(("Tree/Materials/RTT_DefaultTreeBark", typeof(Material)), new Guid("a5c14895-de07-414e-9567-cbd3526f9bc2"));
                res.Add(("Tree/Materials/RTT_DefaultTreeBranches", typeof(Material)), new Guid("d8c1144f-59aa-48f1-8dd9-92567a505817"));
            }
            
        }
    }
}


