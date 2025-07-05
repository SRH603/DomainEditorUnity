using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.TerrainLayer), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class TerrainLayerSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 17;
        const int _TYPE_INDEX = 138;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID diffuseTexture { get; set; }

        [ProtoMember(4)]
        public TID normalMapTexture { get; set; }

        [ProtoMember(5)]
        public TID maskMapTexture { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Vector2 tileSize { get; set; }

        [ProtoMember(7)]
        public global::UnityEngine.Vector2 tileOffset { get; set; }

        [ProtoMember(8)]
        public global::UnityEngine.Color specular { get; set; }

        [ProtoMember(9)]
        public global::System.Single metallic { get; set; }

        [ProtoMember(10)]
        public global::System.Single smoothness { get; set; }

        [ProtoMember(11)]
        public global::System.Single normalScale { get; set; }

        [ProtoMember(12)]
        public global::UnityEngine.Vector4 diffuseRemapMin { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.Vector4 diffuseRemapMax { get; set; }

        [ProtoMember(14)]
        public global::UnityEngine.Vector4 maskMapRemapMin { get; set; }

        [ProtoMember(15)]
        public global::UnityEngine.Vector4 maskMapRemapMax { get; set; }

        [ProtoMember(16)]
        public global::System.String name { get; set; }

        [ProtoMember(17)]
        public global::UnityEngine.HideFlags hideFlags { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.TerrainLayer)obj;
            id = idmap.GetOrCreateID(o);
            diffuseTexture = idmap.GetOrCreateID(o.diffuseTexture);
            normalMapTexture = idmap.GetOrCreateID(o.normalMapTexture);
            maskMapTexture = idmap.GetOrCreateID(o.maskMapTexture);
            tileSize = o.tileSize;
            tileOffset = o.tileOffset;
            specular = o.specular;
            metallic = o.metallic;
            smoothness = o.smoothness;
            normalScale = o.normalScale;
            diffuseRemapMin = o.diffuseRemapMin;
            diffuseRemapMax = o.diffuseRemapMax;
            maskMapRemapMin = o.maskMapRemapMin;
            maskMapRemapMax = o.maskMapRemapMax;
            name = o.name;
            hideFlags = o.hideFlags;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<global::UnityEngine.TerrainLayer>(id);
            o.diffuseTexture = idmap.GetObject<global::UnityEngine.Texture2D>(diffuseTexture);
            o.normalMapTexture = idmap.GetObject<global::UnityEngine.Texture2D>(normalMapTexture);
            o.maskMapTexture = idmap.GetObject<global::UnityEngine.Texture2D>(maskMapTexture);
            o.tileSize = tileSize;
            o.tileOffset = tileOffset;
            o.specular = specular;
            o.metallic = metallic;
            o.smoothness = smoothness;
            o.normalScale = normalScale;
            o.diffuseRemapMin = diffuseRemapMin;
            o.diffuseRemapMax = diffuseRemapMax;
            o.maskMapRemapMin = maskMapRemapMin;
            o.maskMapRemapMax = maskMapRemapMax;
            o.name = name;
            o.hideFlags = hideFlags;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
