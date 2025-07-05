using ProtoBuf;
using System;
using System.Threading.Tasks;


#if UNITY_6000_0_OR_NEWER
using UnityPhysicsMaterial = UnityEngine.PhysicsMaterial;
//using UnityPhysicsMaterialCombine = UnityEngine.PhysicsMaterialCombine;
#else
using UnityPhysicsMaterial = UnityEngine.PhysicMaterial;
//using UnityPhysicsMaterialCombine = UnityEngine.PhysicMaterialCombine;
#endif

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(UnityPhysicsMaterial), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class PhysicMaterialSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 9;
        const int _TYPE_INDEX = 124;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::System.Single bounciness { get; set; }

        [ProtoMember(4)]
        public global::System.Single dynamicFriction { get; set; }

        [ProtoMember(5)]
        public global::System.Single staticFriction { get; set; }

        //[ProtoMember(6)]
        //public UnityPhysicsMaterialCombine frictionCombine { get; set; }

        //[ProtoMember(7)]
        //public UnityPhysicsMaterialCombine bounceCombine { get; set; }

        [ProtoMember(8)]
        public global::System.String name { get; set; }

        [ProtoMember(9)]
        public global::UnityEngine.HideFlags hideFlags { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (UnityPhysicsMaterial)obj;
            id = idmap.GetOrCreateID(o);
            bounciness = o.bounciness;
            dynamicFriction = o.dynamicFriction;
            staticFriction = o.staticFriction;
            //frictionCombine = o.frictionCombine;
            //bounceCombine = o.bounceCombine;
            name = o.name;
            hideFlags = o.hideFlags;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<UnityPhysicsMaterial>(id);
            o.bounciness = bounciness;
            o.dynamicFriction = dynamicFriction;
            o.staticFriction = staticFriction;
            //o.frictionCombine = frictionCombine;
            //o.bounceCombine = bounceCombine;
            o.name = name;
            o.hideFlags = hideFlags;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
