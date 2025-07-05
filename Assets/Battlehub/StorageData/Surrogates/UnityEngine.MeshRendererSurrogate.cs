using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.MeshRenderer), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class MeshRendererSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 9;
        const int _TYPE_INDEX = 106;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::System.Boolean enabled { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Rendering.ShadowCastingMode shadowCastingMode { get; set; }

        [ProtoMember(6)]
        public global::System.Boolean receiveShadows { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean forceRenderingOff { get; set; }

        [ProtoMember(8)]
        public global::System.UInt32 renderingLayerMask { get; set; }

        [ProtoMember(9)]
        public TID[] sharedMaterials { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.MeshRenderer)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            enabled = o.enabled;
            shadowCastingMode = o.shadowCastingMode;
            receiveShadows = o.receiveShadows;
            forceRenderingOff = o.forceRenderingOff;
            renderingLayerMask = o.renderingLayerMask;
            sharedMaterials = idmap.GetOrCreateIDs(o.sharedMaterials);
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.MeshRenderer, TID>(id, gameObjectId);
            o.enabled = enabled;
            o.shadowCastingMode = shadowCastingMode;
            o.receiveShadows = receiveShadows;
            o.forceRenderingOff = forceRenderingOff;
            o.renderingLayerMask = renderingLayerMask;            
            o.sharedMaterials = idmap.GetObjects<global::UnityEngine.Material, TID>(sharedMaterials);
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
