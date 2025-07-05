using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.Dropdown), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class DropdownSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 21;
        const int _TYPE_INDEX = 4133;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public TID captionText { get; set; }

        [ProtoMember(5)]
        public TID captionImage { get; set; }

        [ProtoMember(6)]
        public TID itemText { get; set; }

        [ProtoMember(7)]
        public TID itemImage { get; set; }

        [ProtoMember(8)]
        public global::System.Collections.Generic.List<TID> options { get; set; }

        [ProtoMember(9)]
        public TID onValueChanged { get; set; }

        [ProtoMember(10)]
        public global::System.Single alphaFadeSpeed { get; set; }

        [ProtoMember(11)]
        public global::System.Int32 value { get; set; }

        [ProtoMember(12)]
        public global::Battlehub.Storage.Surrogates.UnityEngine.UI.NavigationSurrogate<TID> navigation { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.UI.Selectable.Transition transition { get; set; }

        [ProtoMember(14)]
        public global::UnityEngine.UI.ColorBlock colors { get; set; }

        [ProtoMember(15)]
        public global::Battlehub.Storage.Surrogates.UnityEngine.UI.SpriteStateSurrogate<TID> spriteState { get; set; }

        [ProtoMember(16)]
        public TID animationTriggers { get; set; }

        [ProtoMember(17)]
        public TID targetGraphic { get; set; }

        [ProtoMember(18)]
        public global::System.Boolean interactable { get; set; }

        [ProtoMember(19)]
        public TID image { get; set; }

        [ProtoMember(20)]
        public global::System.Boolean enabled { get; set; }

        [ProtoMember(21)]
        public TID template { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.Dropdown)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            captionText = idmap.GetOrCreateID(o.captionText);
            captionImage = idmap.GetOrCreateID(o.captionImage);
            itemText = idmap.GetOrCreateID(o.itemText);
            itemImage = idmap.GetOrCreateID(o.itemImage);
            options = idmap.GetOrCreateIDs(o.options);
            onValueChanged = idmap.GetOrCreateID(o.onValueChanged);
            alphaFadeSpeed = o.alphaFadeSpeed;
            value = o.value;
            navigation.Serialize(o.navigation, ctx);
            transition = o.transition;
            colors = o.colors;
            spriteState.Serialize(o.spriteState, ctx);
            animationTriggers = idmap.GetOrCreateID(o.animationTriggers);
            targetGraphic = idmap.GetOrCreateID(o.targetGraphic);
            interactable = o.interactable;
            image = idmap.GetOrCreateID(o.image);
            enabled = o.enabled;
            template = idmap.GetOrCreateID(o.template);
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.UI.Dropdown, TID>(id, gameObjectId);
            o.captionText = idmap.GetObject<global::UnityEngine.UI.Text>(captionText);
            o.captionImage = idmap.GetObject<global::UnityEngine.UI.Image>(captionImage);
            o.itemText = idmap.GetObject<global::UnityEngine.UI.Text>(itemText);
            o.itemImage = idmap.GetObject<global::UnityEngine.UI.Image>(itemImage);
            o.options = idmap.GetObjects<global::UnityEngine.UI.Dropdown.OptionData, TID>(options);
            o.onValueChanged = idmap.GetObject<global::UnityEngine.UI.Dropdown.DropdownEvent>(onValueChanged);
            o.alphaFadeSpeed = alphaFadeSpeed;
            o.value = value;
            o.navigation = navigation.Deserialize(ctx);
            o.transition = transition;
            o.colors = colors;
            o.spriteState = spriteState.Deserialize(ctx);
            o.animationTriggers = idmap.GetObject<global::UnityEngine.UI.AnimationTriggers>(animationTriggers);
            o.targetGraphic = idmap.GetObject<global::UnityEngine.UI.Graphic>(targetGraphic);
            o.interactable = interactable;
            o.image = idmap.GetObject<global::UnityEngine.UI.Image>(image);
            o.enabled = enabled;
            o.template = idmap.GetObject<global::UnityEngine.RectTransform>(template);
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
