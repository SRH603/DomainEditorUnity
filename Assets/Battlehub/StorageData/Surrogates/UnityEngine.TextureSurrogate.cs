using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Texture), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class TextureSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 15;
        const int _TYPE_INDEX = 148;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::System.Int32 width { get; set; }

        [ProtoMember(4)]
        public global::System.Int32 height { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Rendering.TextureDimension dimension { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.TextureWrapMode wrapMode { get; set; }

        [ProtoMember(7)]
        public global::UnityEngine.TextureWrapMode wrapModeU { get; set; }

        [ProtoMember(8)]
        public global::UnityEngine.TextureWrapMode wrapModeV { get; set; }

        [ProtoMember(9)]
        public global::UnityEngine.TextureWrapMode wrapModeW { get; set; }

        [ProtoMember(10)]
        public global::UnityEngine.FilterMode filterMode { get; set; }

        [ProtoMember(11)]
        public global::System.Int32 anisoLevel { get; set; }

        [ProtoMember(12)]
        public global::System.Single mipMapBias { get; set; }

        //[ProtoMember(13)]
        public global::UnityEngine.Hash128 imageContentsHash { get; set; }

        [ProtoMember(14)]
        public global::System.String name { get; set; }

        [ProtoMember(15)]
        public global::UnityEngine.HideFlags hideFlags { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Texture)obj;
            id = idmap.GetOrCreateID(o);
            width = o.width;
            height = o.height;
            dimension = o.dimension;
            wrapMode = o.wrapMode;
            wrapModeU = o.wrapModeU;
            wrapModeV = o.wrapModeV;
            wrapModeW = o.wrapModeW;
            filterMode = o.filterMode;
            anisoLevel = o.anisoLevel;
            mipMapBias = o.mipMapBias;
            //imageContentsHash = o.imageContentsHash;
            name = o.name;
            hideFlags = o.hideFlags;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            global::UnityEngine.Texture o = idmap.GetObject<global::UnityEngine.Texture>(id); 
            if (o == null)
            {
                idmap.AddObject(o, id);
                return default;
            }
            o.width = width;
            o.height = height;
            o.dimension = dimension;
            o.wrapMode = wrapMode;
            o.wrapModeU = wrapModeU;
            o.wrapModeV = wrapModeV;
            o.wrapModeW = wrapModeW;
            o.filterMode = filterMode;
            o.anisoLevel = anisoLevel;
            o.mipMapBias = mipMapBias;
            //o.imageContentsHash = imageContentsHash;
            o.name = name;
            o.hideFlags = hideFlags;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
