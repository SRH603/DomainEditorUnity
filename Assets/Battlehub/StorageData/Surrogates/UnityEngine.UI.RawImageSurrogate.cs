using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.RawImage), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class RawImageSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 13;
        const int _TYPE_INDEX = 4111;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public TID texture { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Rect uvRect { get; set; }

        //[ProtoMember(6)]
        public TID onCullStateChanged { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean maskable { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean isMaskingGraphic { get; set; }

        [ProtoMember(9)]
        public global::UnityEngine.Color color { get; set; }

        [ProtoMember(10)]
        public global::System.Boolean raycastTarget { get; set; }

        [ProtoMember(11)]
        public global::UnityEngine.Vector4 raycastPadding { get; set; }

        [ProtoMember(12)]
        public TID material { get; set; }

        [ProtoMember(13)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.RawImage)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            texture = idmap.GetOrCreateID(o.texture);
            uvRect = o.uvRect;
            // onCullStateChanged = idmap.GetOrCreateID(o.onCullStateChanged);
            maskable = o.maskable;
            isMaskingGraphic = o.isMaskingGraphic;
            color = o.color;
            raycastTarget = o.raycastTarget;
            raycastPadding = o.raycastPadding;
            material = idmap.GetOrCreateID(o.material);
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.UI.RawImage, TID>(id, gameObjectId);
            o.texture = idmap.GetObject<global::UnityEngine.Texture>(texture);
            o.uvRect = uvRect;
            // o.onCullStateChanged = idmap.GetObject<global::UnityEngine.UI.MaskableGraphic.CullStateChangedEvent>(onCullStateChanged);
            o.maskable = maskable;
            o.isMaskingGraphic = isMaskingGraphic;
            o.color = color;
            o.raycastTarget = raycastTarget;
            o.raycastPadding = raycastPadding;
            o.material = idmap.GetObject<global::UnityEngine.Material>(material);
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
