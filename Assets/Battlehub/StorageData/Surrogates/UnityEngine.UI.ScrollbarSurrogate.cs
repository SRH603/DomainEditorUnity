using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.Scrollbar), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class ScrollbarSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 18;
        const int _TYPE_INDEX = 4136;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.UI.Scrollbar.Direction direction { get; set; }

        [ProtoMember(5)]
        public global::System.Single value { get; set; }

        [ProtoMember(6)]
        public global::System.Single size { get; set; }

        [ProtoMember(7)]
        public global::System.Int32 numberOfSteps { get; set; }

        [ProtoMember(8)]
        public TID onValueChanged { get; set; }

        [ProtoMember(9)]
        public global::Battlehub.Storage.Surrogates.UnityEngine.UI.NavigationSurrogate<TID> navigation { get; set; }

        [ProtoMember(10)]
        public global::UnityEngine.UI.Selectable.Transition transition { get; set; }

        [ProtoMember(11)]
        public global::UnityEngine.UI.ColorBlock colors { get; set; }

        [ProtoMember(12)]
        public global::Battlehub.Storage.Surrogates.UnityEngine.UI.SpriteStateSurrogate<TID> spriteState { get; set; }

        [ProtoMember(13)]
        public TID animationTriggers { get; set; }

        [ProtoMember(14)]
        public TID targetGraphic { get; set; }

        [ProtoMember(15)]
        public global::System.Boolean interactable { get; set; }

        [ProtoMember(16)]
        public TID image { get; set; }

        [ProtoMember(17)]
        public global::System.Boolean enabled { get; set; }

        [ProtoMember(18)]
        public TID handleRect { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.Scrollbar)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            direction = o.direction;
            value = o.value;
            size = o.size;
            numberOfSteps = o.numberOfSteps;
            onValueChanged = idmap.GetOrCreateID(o.onValueChanged);
            navigation.Serialize(o.navigation, ctx);
            transition = o.transition;
            colors = o.colors;
            spriteState.Serialize(o.spriteState, ctx);
            animationTriggers = idmap.GetOrCreateID(o.animationTriggers);
            targetGraphic = idmap.GetOrCreateID(o.targetGraphic);
            interactable = o.interactable;
            image = idmap.GetOrCreateID(o.image);
            enabled = o.enabled;
            handleRect = idmap.GetOrCreateID(o.handleRect);
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.UI.Scrollbar, TID>(id, gameObjectId);
            o.direction = direction;
            o.value = value;
            o.size = size;
            o.numberOfSteps = numberOfSteps;
            o.onValueChanged = idmap.GetObject<global::UnityEngine.UI.Scrollbar.ScrollEvent>(onValueChanged);
            o.navigation = navigation.Deserialize(ctx);
            o.transition = transition;
            o.colors = colors;
            o.spriteState = spriteState.Deserialize(ctx);
            o.animationTriggers = idmap.GetObject<global::UnityEngine.UI.AnimationTriggers>(animationTriggers);
            o.targetGraphic = idmap.GetObject<global::UnityEngine.UI.Graphic>(targetGraphic);
            o.interactable = interactable;
            o.image = idmap.GetObject<global::UnityEngine.UI.Image>(image);
            o.enabled = enabled;
            o.handleRect = idmap.GetObject<global::UnityEngine.RectTransform>(handleRect);
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
