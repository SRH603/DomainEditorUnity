using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Color32), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct Color32Surrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 5;
        const int _TYPE_INDEX = 116;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::System.Byte r { get; set; }

        [ProtoMember(3)]
        public global::System.Byte g { get; set; }

        [ProtoMember(4)]
        public global::System.Byte b { get; set; }

        [ProtoMember(5)]
        public global::System.Byte a { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Color32)obj;
            r = o.r;
            g = o.g;
            b = o.b;
            a = o.a;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.Color32();
            o.r = r;
            o.g = g;
            o.b = b;
            o.a = a;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
