using Battlehub.Storage;
using System;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class ExternalAssetListExtension
    {
        [InitializeOnLoadMethod]
        public static void Extend()
        {
            var res = ExternalAssetListGen.ProjectResources;
            res.Add(("Textures/GridBox_Default", typeof(Texture2D)), new Guid("25bb4051-61c0-4897-b0df-aa43d7f62e0e"));
            res.Add(("Materials/ProBuilderDefault", typeof(Material)), new Guid("08414d0a-39d2-4429-ba7d-3f100af40fbd"));
        }
    }
}


