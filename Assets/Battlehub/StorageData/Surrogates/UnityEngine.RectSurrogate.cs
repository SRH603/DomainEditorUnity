using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Rect), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct RectSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 14;
        const int _TYPE_INDEX = 131;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::System.Single x { get; set; }

        [ProtoMember(3)]
        public global::System.Single y { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.Vector2 position { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Vector2 center { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Vector2 min { get; set; }

        [ProtoMember(7)]
        public global::UnityEngine.Vector2 max { get; set; }

        [ProtoMember(8)]
        public global::System.Single width { get; set; }

        [ProtoMember(9)]
        public global::System.Single height { get; set; }

        [ProtoMember(10)]
        public global::UnityEngine.Vector2 size { get; set; }

        [ProtoMember(11)]
        public global::System.Single xMin { get; set; }

        [ProtoMember(12)]
        public global::System.Single yMin { get; set; }

        [ProtoMember(13)]
        public global::System.Single xMax { get; set; }

        [ProtoMember(14)]
        public global::System.Single yMax { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Rect)obj;
            x = o.x;
            y = o.y;
            position = o.position;
            center = o.center;
            min = o.min;
            max = o.max;
            width = o.width;
            height = o.height;
            size = o.size;
            xMin = o.xMin;
            yMin = o.yMin;
            xMax = o.xMax;
            yMax = o.yMax;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.Rect();
            o.x = x;
            o.y = y;
            o.position = position;
            o.center = center;
            o.min = min;
            o.max = max;
            o.width = width;
            o.height = height;
            o.size = size;
            o.xMin = xMin;
            o.yMin = yMin;
            o.xMax = xMax;
            o.yMax = yMax;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
