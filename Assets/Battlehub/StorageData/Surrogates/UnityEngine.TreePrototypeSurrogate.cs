using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.TreePrototype), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates: false)]
    public struct TreePrototypeSurrogate<TID> : IValueTypeSurrogate<global::UnityEngine.TreePrototype, TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 5;
        const int _TYPE_INDEX = 142;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID prefab { get; set; }

        [ProtoMember(4)]
        public global::System.Single bendFactor { get; set; }

        [ProtoMember(5)]
        public global::System.Int32 navMeshLod { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public void Serialize(in global::UnityEngine.TreePrototype obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.TreePrototype)obj;
            id = idmap.GetOrCreateID(o);
            prefab = idmap.GetOrCreateID(o.prefab);
            bendFactor = o.bendFactor;
            navMeshLod = o.navMeshLod;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE
        }

        public global::UnityEngine.TreePrototype Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<global::UnityEngine.TreePrototype>(id);
            o.prefab = idmap.GetObject<global::UnityEngine.GameObject>(prefab);
            o.bendFactor = bendFactor;
            o.navMeshLod = navMeshLod;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return o;
        }
    }
}
