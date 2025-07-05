using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    public struct TextureData
    {
        [ProtoMember(1)]
        public Vector2 Offset
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public Vector2 Scale
        {
            get;
            set;
        }

        public TextureData(Vector2 offset, Vector2 scale)
        {
            Offset = offset;
            Scale = scale;
        }
    }

    [ProtoContract]
    [Surrogate(typeof(Material), propertyIndex: _PROPERTY_INDEX, typeIndex: _TYPE_INDEX, enableUpdates:false)]
    public class MaterialSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {
        const int _PROPERTY_INDEX = 9;
        const int _TYPE_INDEX = 105;

        private static readonly IMaterialUtils s_materialUtils;

        static MaterialSurrogate()
        {
            if (RenderPipelineInfo.Type == RPType.URP)
            {
                s_materialUtils = new UniversalLitMaterialUtils();
            }
            else
            {
                s_materialUtils = new BuiltinStandardMaterialUtils();
            }
        }

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID ID
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public string Name
        {
            get;
            set;
        }

        [ProtoMember(4)]
        public string ShaderName
        {
            get;
            set;
        }

        [ProtoMember(5)]
        public Dictionary<string, TID> Textures
        {
            get;
            set;
        }

        [ProtoMember(6)]
        public Dictionary<string, TextureData> TexturesData
        {
            get;
            set;
        }

        [ProtoMember(7)]
        public Dictionary<string, Color> Colors
        {
            get;
            set;
        }
        
        [ProtoMember(8)]
        public Dictionary<string, Vector4> Vectors
        {
            get;
            set;
        }

        [ProtoMember(9)]
        public Dictionary<string, float> Floats
        {
            get;
            set;
        }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        private void ReadTexture(string propertyName, Material material, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;
            if (Textures == null)
            {
                Textures = new Dictionary<string, TID>();
            }

            if (TexturesData == null)
            {
                TexturesData = new Dictionary<string, TextureData>();
            }

            Texture texture = material.GetTexture(propertyName);
            Textures[propertyName] = idmap.GetOrCreateID(texture);
            TexturesData[propertyName] =
                new TextureData(
                    material.GetTextureOffset(propertyName),
                    material.GetTextureScale(propertyName));
        }

        private void ReadColor(string propertyName, Material material)
        {
            if (Colors == null)
            {
                Colors = new Dictionary<string, Color>();
            }

            Colors[propertyName] = material.GetColor(propertyName);
        }

        private void ReadVector(string propertyName, Material material)
        {
            if (Vectors == null)
            {
                Vectors = new Dictionary<string, Vector4>();
            }

            Vectors[propertyName] = material.GetVector(propertyName);
        }

        private void ReadFloat(string propertyName, Material material)
        {
            if (Floats == null)
            {
                Floats = new Dictionary<string, float>();
            }

            Floats[propertyName] = material.GetFloat(propertyName);
        }

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;
            var shaderUtil = ctx.ShaderUtil;

            Material material = (Material)obj;
            ID = idmap.GetOrCreateID(material);
            Name = material.name;

            Shader shader = material.shader;
            if (shader != null)
            {
                ShaderName = material.shader.name;
                var shaderInfo = shaderUtil.GetShaderInfo(material.shader);
                if (shaderInfo != null)
                {
                    int propertyCount = shaderInfo.PropertyCount;
                    for (int i = 0; i < propertyCount; ++i)
                    {
                        string propertyName = shaderInfo.PropertyNames[i];
                        switch (shaderInfo.PropertyTypes[i])
                        {
                            case RTShaderPropertyType.TexEnv:
                                ReadTexture(propertyName, material, ctx);
                                break;
                            case RTShaderPropertyType.Color:
                                ReadColor(propertyName, material);
                                break;
                            case RTShaderPropertyType.Vector:
                                ReadVector(propertyName, material);
                                break;
                            case RTShaderPropertyType.Float:
                            case RTShaderPropertyType.Range:
                                ReadFloat(propertyName, material);
                                break;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"ShaderInfo not found {ShaderName}");
                }
            }

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            Material material = idmap.GetObject<Material>(ID);
            if (material == null)
            {
                Shader shader = null;
                if (ShaderName != null)
                {
                    shader = Shader.Find(ShaderName);
                }

                if (shader == null)
                {
                    shader = Shader.Find(RenderPipelineInfo.DefaultShaderName);
                    if (ctx.ShaderUtil.GetShaderInfo(shader) == null)
                    {
                        shader = null;
                    }
                }
                    
                if (shader == null)
                {
                    //Debug.LogWarning($"{ShaderName} shader not found");
                    var fallbackShader = Shader.Find("Unlit/Color");
                    if (fallbackShader != null)
                    {
                        material = new Material(fallbackShader);
                        material.name = Name;
                    }
                    else
                    {
                        Debug.LogWarning($"Unlit/Color shader not found");
                    }

                    idmap.AddObject(material, ID);
                    return new ValueTask<object>(material);
                }

                material = new Material(shader);
                idmap.AddObject(material, ID);
            }

            material.name = Name;

            if (Textures != null)
            {
                foreach (var kvp in Textures)
                {
                    var propertyName = kvp.Key;
                    var id = kvp.Value;
                    material.SetTexture(propertyName, idmap.GetObject<Texture>(id));
                }
            }

            if (TexturesData != null)
            {
                foreach (var kvp in TexturesData)
                {
                    var propertyName = kvp.Key;
                    var data = kvp.Value;
                    material.SetTextureOffset(propertyName, data.Offset);
                    material.SetTextureScale(propertyName, data.Scale);
                }
            }

            if (Colors != null)
            {
                foreach (var kvp in Colors)
                {
                    var propertyName = kvp.Key;
                    var data = kvp.Value;
                    material.SetColor(propertyName, data);
                }
            }

            if (Vectors != null)
            {
                foreach (var kvp in Vectors)
                {
                    var propertyName = kvp.Key;
                    var data = kvp.Value;
                    material.SetVector(propertyName, data);
                }
            }

            if (Floats != null)
            {
                foreach (var kvp in Floats)
                {
                    var propertyName = kvp.Key;
                    var data = kvp.Value;
                    material.SetFloat(propertyName, data);
                }
            }

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            s_materialUtils.SetMaterialKeywords(material);

            return new ValueTask<object>(material);
        }
    }
}