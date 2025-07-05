using Battlehub.RTCommon;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class MaterialPalette : MonoBehaviour 
    {
        public List<Material> Materials = new List<Material>();

        public Material GetMaterialWithTexture(Texture2D texture)
        {
            if(Materials == null)
            {
                return null;
            }

            for(int i = 0; i < Materials.Count; ++i)
            {
                Material material = Materials[i];
                if(material != null && material.MainTexture() == texture)
                {
                    return material;
                }
            }

            return null;
        }
    }
}


