using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.TreeInstance), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct TreeInstanceSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 8;
        const int _TYPE_INDEX = 141;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::UnityEngine.Vector3 position { get; set; }

        [ProtoMember(3)]
        public global::System.Single widthScale { get; set; }

        [ProtoMember(4)]
        public global::System.Single heightScale { get; set; }

        [ProtoMember(5)]
        public global::System.Single rotation { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Color32 color { get; set; }

        [ProtoMember(7)]
        public global::UnityEngine.Color32 lightmapColor { get; set; }

        [ProtoMember(8)]
        public global::System.Int32 prototypeIndex { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.TreeInstance)obj;
            position = o.position;
            widthScale = o.widthScale;
            heightScale = o.heightScale;
            rotation = o.rotation;
            color = o.color;
            lightmapColor = o.lightmapColor;
            prototypeIndex = o.prototypeIndex;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.TreeInstance();
            o.position = position;
            o.widthScale = widthScale;
            o.heightScale = heightScale;
            o.rotation = rotation;
            o.color = color;
            o.lightmapColor = lightmapColor;
            o.prototypeIndex = prototypeIndex;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
