using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.ScrollRect), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class ScrollRectSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 24;
        const int _TYPE_INDEX = 4137;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::System.Boolean horizontal { get; set; }

        [ProtoMember(5)]
        public global::System.Boolean vertical { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.UI.ScrollRect.MovementType movementType { get; set; }

        [ProtoMember(7)]
        public global::System.Single elasticity { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean inertia { get; set; }

        [ProtoMember(9)]
        public global::System.Single decelerationRate { get; set; }

        [ProtoMember(10)]
        public global::System.Single scrollSensitivity { get; set; }

        [ProtoMember(11)]
        public TID horizontalScrollbar { get; set; }

        [ProtoMember(12)]
        public TID verticalScrollbar { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.UI.ScrollRect.ScrollbarVisibility horizontalScrollbarVisibility { get; set; }

        [ProtoMember(14)]
        public global::UnityEngine.UI.ScrollRect.ScrollbarVisibility verticalScrollbarVisibility { get; set; }

        [ProtoMember(15)]
        public global::System.Single horizontalScrollbarSpacing { get; set; }

        [ProtoMember(16)]
        public global::System.Single verticalScrollbarSpacing { get; set; }

        [ProtoMember(17)]
        public TID onValueChanged { get; set; }

        [ProtoMember(18)]
        public global::UnityEngine.Vector2 velocity { get; set; }

        [ProtoMember(19)]
        public global::UnityEngine.Vector2 normalizedPosition { get; set; }

        [ProtoMember(20)]
        public global::System.Single horizontalNormalizedPosition { get; set; }

        [ProtoMember(21)]
        public global::System.Single verticalNormalizedPosition { get; set; }

        [ProtoMember(22)]
        public global::System.Boolean enabled { get; set; }

        [ProtoMember(23)]
        public TID content { get; set; }

        [ProtoMember(24)]
        public TID viewport { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.ScrollRect)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            content = idmap.GetOrCreateID(o.content);
            horizontal = o.horizontal;
            vertical = o.vertical;
            movementType = o.movementType;
            elasticity = o.elasticity;
            inertia = o.inertia;
            decelerationRate = o.decelerationRate;
            scrollSensitivity = o.scrollSensitivity;
            viewport = idmap.GetOrCreateID(o.viewport);
            horizontalScrollbar = idmap.GetOrCreateID(o.horizontalScrollbar);
            verticalScrollbar = idmap.GetOrCreateID(o.verticalScrollbar);
            horizontalScrollbarVisibility = o.horizontalScrollbarVisibility;
            verticalScrollbarVisibility = o.verticalScrollbarVisibility;
            horizontalScrollbarSpacing = o.horizontalScrollbarSpacing;
            verticalScrollbarSpacing = o.verticalScrollbarSpacing;
            onValueChanged = idmap.GetOrCreateID(o.onValueChanged);
            velocity = o.velocity;
            normalizedPosition = o.normalizedPosition;
            horizontalNormalizedPosition = o.horizontalNormalizedPosition;
            verticalNormalizedPosition = o.verticalNormalizedPosition;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.UI.ScrollRect, TID>(id, gameObjectId);
            o.content = idmap.GetObject<global::UnityEngine.RectTransform>(content);
            o.horizontal = horizontal;
            o.vertical = vertical;
            o.movementType = movementType;
            o.elasticity = elasticity;
            o.inertia = inertia;
            o.decelerationRate = decelerationRate;
            o.scrollSensitivity = scrollSensitivity;
            o.viewport = idmap.GetObject<global::UnityEngine.RectTransform>(viewport);
            o.horizontalScrollbar = idmap.GetObject<global::UnityEngine.UI.Scrollbar>(horizontalScrollbar);
            o.verticalScrollbar = idmap.GetObject<global::UnityEngine.UI.Scrollbar>(verticalScrollbar);
            o.horizontalScrollbarVisibility = horizontalScrollbarVisibility;
            o.verticalScrollbarVisibility = verticalScrollbarVisibility;
            o.horizontalScrollbarSpacing = horizontalScrollbarSpacing;
            o.verticalScrollbarSpacing = verticalScrollbarSpacing;
            o.onValueChanged = idmap.GetObject<global::UnityEngine.UI.ScrollRect.ScrollRectEvent>(onValueChanged);
            o.velocity = velocity;
            o.normalizedPosition = normalizedPosition;
            o.horizontalNormalizedPosition = horizontalNormalizedPosition;
            o.verticalNormalizedPosition = verticalNormalizedPosition;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
