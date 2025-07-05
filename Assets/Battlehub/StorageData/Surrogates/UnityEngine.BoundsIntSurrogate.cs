using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.BoundsInt), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct BoundsIntSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 14;
        const int _TYPE_INDEX = 111;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::System.Int32 x { get; set; }

        [ProtoMember(3)]
        public global::System.Int32 y { get; set; }

        [ProtoMember(4)]
        public global::System.Int32 z { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Vector3Int min { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Vector3Int max { get; set; }

        [ProtoMember(7)]
        public global::System.Int32 xMin { get; set; }

        [ProtoMember(8)]
        public global::System.Int32 yMin { get; set; }

        [ProtoMember(9)]
        public global::System.Int32 zMin { get; set; }

        [ProtoMember(10)]
        public global::System.Int32 xMax { get; set; }

        [ProtoMember(11)]
        public global::System.Int32 yMax { get; set; }

        [ProtoMember(12)]
        public global::System.Int32 zMax { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.Vector3Int position { get; set; }

        [ProtoMember(14)]
        public global::UnityEngine.Vector3Int size { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.BoundsInt)obj;
            x = o.x;
            y = o.y;
            z = o.z;
            min = o.min;
            max = o.max;
            xMin = o.xMin;
            yMin = o.yMin;
            zMin = o.zMin;
            xMax = o.xMax;
            yMax = o.yMax;
            zMax = o.zMax;
            position = o.position;
            size = o.size;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.BoundsInt();
            o.x = x;
            o.y = y;
            o.z = z;
            o.min = min;
            o.max = max;
            o.xMin = xMin;
            o.yMin = yMin;
            o.zMin = zMin;
            o.xMax = xMax;
            o.yMax = yMax;
            o.zMax = zMax;
            o.position = position;
            o.size = size;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
