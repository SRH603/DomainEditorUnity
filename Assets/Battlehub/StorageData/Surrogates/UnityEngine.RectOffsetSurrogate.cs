using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.RectOffset), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class RectOffsetSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 6;
        const int _TYPE_INDEX = 4114;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::System.Int32 left { get; set; }

        [ProtoMember(4)]
        public global::System.Int32 right { get; set; }

        [ProtoMember(5)]
        public global::System.Int32 top { get; set; }

        [ProtoMember(6)]
        public global::System.Int32 bottom { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.RectOffset)obj;
            id = idmap.GetOrCreateID(o);
            left = o.left;
            right = o.right;
            top = o.top;
            bottom = o.bottom;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<global::UnityEngine.RectOffset>(id);
            o.left = left;
            o.right = right;
            o.top = top;
            o.bottom = bottom;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
