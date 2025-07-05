using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.RaycastHit), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct RaycastHitSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 5;
        const int _TYPE_INDEX = 129;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::UnityEngine.Vector3 point { get; set; }

        [ProtoMember(3)]
        public global::UnityEngine.Vector3 normal { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.Vector3 barycentricCoordinate { get; set; }

        [ProtoMember(5)]
        public global::System.Single distance { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.RaycastHit)obj;
            point = o.point;
            normal = o.normal;
            barycentricCoordinate = o.barycentricCoordinate;
            distance = o.distance;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.RaycastHit();
            o.point = point;
            o.normal = normal;
            o.barycentricCoordinate = barycentricCoordinate;
            o.distance = distance;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
