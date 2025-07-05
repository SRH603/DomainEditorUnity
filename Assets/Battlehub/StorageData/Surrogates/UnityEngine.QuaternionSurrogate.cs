using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Quaternion), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct QuaternionSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 6;
        const int _TYPE_INDEX = 126;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::System.Single x { get; set; }

        [ProtoMember(3)]
        public global::System.Single y { get; set; }

        [ProtoMember(4)]
        public global::System.Single z { get; set; }

        [ProtoMember(5)]
        public global::System.Single w { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Vector3 eulerAngles { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Quaternion)obj;
            x = o.x;
            y = o.y;
            z = o.z;
            w = o.w;
            eulerAngles = o.eulerAngles;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.Quaternion();
            o.x = x;
            o.y = y;
            o.z = z;
            o.w = w;
            o.eulerAngles = eulerAngles;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
