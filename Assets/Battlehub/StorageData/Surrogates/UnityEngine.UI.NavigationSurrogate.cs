using ProtoBuf;
using System;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.Navigation), _PROPERTY_INDEX, _TYPE_INDEX)]
    public struct NavigationSurrogate<TID> : IValueTypeSurrogate<global::UnityEngine.UI.Navigation, TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 8;
        const int _TYPE_INDEX = 4124;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public global::UnityEngine.UI.Navigation.Mode mode { get; set; }

        [ProtoMember(3)]
        public global::System.Boolean wrapAround { get; set; }

        [ProtoMember(4)]
        public TID selectOnUp { get; set; }

        [ProtoMember(5)]
        public TID selectOnDown { get; set; }

        [ProtoMember(6)]
        public TID selectOnLeft { get; set; }

        [ProtoMember(7)]
        public TID selectOnRight { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public void Serialize(in global::UnityEngine.UI.Navigation o, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            mode = o.mode;
            wrapAround = o.wrapAround;
            selectOnUp = idmap.GetOrCreateID(o.selectOnUp);
            selectOnDown = idmap.GetOrCreateID(o.selectOnDown);
            selectOnLeft = idmap.GetOrCreateID(o.selectOnLeft);
            selectOnRight = idmap.GetOrCreateID(o.selectOnRight);
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        }

        public global::UnityEngine.UI.Navigation Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.UI.Navigation();
            o.mode = mode;
            o.wrapAround = wrapAround;
            o.selectOnUp = idmap.GetObject<global::UnityEngine.UI.Selectable>(selectOnUp);
            o.selectOnDown = idmap.GetObject<global::UnityEngine.UI.Selectable>(selectOnDown);
            o.selectOnLeft = idmap.GetObject<global::UnityEngine.UI.Selectable>(selectOnLeft);
            o.selectOnRight = idmap.GetObject<global::UnityEngine.UI.Selectable>(selectOnRight);
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return o;
        }
    }
}
