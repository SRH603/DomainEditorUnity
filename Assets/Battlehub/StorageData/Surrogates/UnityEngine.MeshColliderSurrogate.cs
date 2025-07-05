using ProtoBuf;
using System;
using System.Threading.Tasks;

#if UNITY_6000_0_OR_NEWER
using UnityPhysicsMaterial = UnityEngine.PhysicsMaterial;
#else
using UnityPhysicsMaterial = UnityEngine.PhysicMaterial;
#endif

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.MeshCollider), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class MeshColliderSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 11;
        const int _TYPE_INDEX = 108;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public TID sharedMesh { get; set; }

        [ProtoMember(5)]
        public global::System.Boolean convex { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.MeshColliderCookingOptions cookingOptions { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean enabled { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean isTrigger { get; set; }

        [ProtoMember(9)]
        public global::System.Single contactOffset { get; set; }

        [ProtoMember(10)]
        public TID sharedMaterial { get; set; }

        //[ProtoMember(11)]
        public TID material { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.MeshCollider)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            sharedMesh = idmap.GetOrCreateID(o.sharedMesh);
            convex = o.convex;
            cookingOptions = o.cookingOptions;
            enabled = o.enabled;
            isTrigger = o.isTrigger;
            contactOffset = o.contactOffset;
            sharedMaterial = idmap.GetOrCreateID(o.sharedMaterial);
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.MeshCollider, TID>(id, gameObjectId);
            o.sharedMesh = idmap.GetObject<global::UnityEngine.Mesh>(sharedMesh);
            o.convex = convex;
            o.cookingOptions = cookingOptions;
            o.enabled = enabled;
            o.isTrigger = isTrigger;
            o.contactOffset = contactOffset;
            o.sharedMaterial = idmap.GetObject<UnityPhysicsMaterial>(sharedMaterial);
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
