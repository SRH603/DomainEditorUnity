using ProtoBuf;
using System;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.SpriteState), _PROPERTY_INDEX, _TYPE_INDEX)]
    public struct SpriteStateSurrogate<TID> : IValueTypeSurrogate<global::UnityEngine.UI.SpriteState, TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 6;
        const int _TYPE_INDEX = 4127;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID highlightedSprite { get; set; }

        [ProtoMember(3)]
        public TID pressedSprite { get; set; }

        [ProtoMember(4)]
        public TID selectedSprite { get; set; }

        [ProtoMember(5)]
        public TID disabledSprite { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public void Serialize(in global::UnityEngine.UI.SpriteState o, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            highlightedSprite = idmap.GetOrCreateID(o.highlightedSprite);
            pressedSprite = idmap.GetOrCreateID(o.pressedSprite);
            selectedSprite = idmap.GetOrCreateID(o.selectedSprite);
            disabledSprite = idmap.GetOrCreateID(o.disabledSprite);
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        }

        public global::UnityEngine.UI.SpriteState Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.UI.SpriteState();
            o.highlightedSprite = idmap.GetObject<global::UnityEngine.Sprite>(highlightedSprite);
            o.pressedSprite = idmap.GetObject<global::UnityEngine.Sprite>(pressedSprite);
            o.selectedSprite = idmap.GetObject<global::UnityEngine.Sprite>(selectedSprite);
            o.disabledSprite = idmap.GetObject<global::UnityEngine.Sprite>(disabledSprite);
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return o;
        }
    }
}
