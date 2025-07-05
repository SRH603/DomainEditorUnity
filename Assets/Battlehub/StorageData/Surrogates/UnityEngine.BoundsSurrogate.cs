using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Bounds), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct BoundsSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 6;
        const int _TYPE_INDEX = 112;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::UnityEngine.Vector3 center { get; set; }

        [ProtoMember(3)]
        public global::UnityEngine.Vector3 size { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.Vector3 extents { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Vector3 min { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Vector3 max { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Bounds)obj;
            center = o.center;
            size = o.size;
            extents = o.extents;
            min = o.min;
            max = o.max;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.Bounds();
            o.center = center;
            o.size = size;
            o.extents = extents;
            o.min = min;
            o.max = max;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
