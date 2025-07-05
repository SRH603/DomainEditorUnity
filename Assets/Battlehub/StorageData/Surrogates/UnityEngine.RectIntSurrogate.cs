using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.RectInt), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct RectIntSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 13;
        const int _TYPE_INDEX = 132;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::System.Int32 x { get; set; }

        [ProtoMember(3)]
        public global::System.Int32 y { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.Vector2Int min { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Vector2Int max { get; set; }

        [ProtoMember(6)]
        public global::System.Int32 width { get; set; }

        [ProtoMember(7)]
        public global::System.Int32 height { get; set; }

        [ProtoMember(8)]
        public global::System.Int32 xMin { get; set; }

        [ProtoMember(9)]
        public global::System.Int32 yMin { get; set; }

        [ProtoMember(10)]
        public global::System.Int32 xMax { get; set; }

        [ProtoMember(11)]
        public global::System.Int32 yMax { get; set; }

        [ProtoMember(12)]
        public global::UnityEngine.Vector2Int position { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.Vector2Int size { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.RectInt)obj;
            x = o.x;
            y = o.y;
            min = o.min;
            max = o.max;
            width = o.width;
            height = o.height;
            xMin = o.xMin;
            yMin = o.yMin;
            xMax = o.xMax;
            yMax = o.yMax;
            position = o.position;
            size = o.size;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.RectInt();
            o.x = x;
            o.y = y;
            o.min = min;
            o.max = max;
            o.width = width;
            o.height = height;
            o.xMin = xMin;
            o.yMin = yMin;
            o.xMax = xMax;
            o.yMax = yMax;
            o.position = position;
            o.size = size;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
