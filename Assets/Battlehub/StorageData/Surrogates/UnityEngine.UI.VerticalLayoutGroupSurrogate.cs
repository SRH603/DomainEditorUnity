using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.VerticalLayoutGroup), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class VerticalLayoutGroupSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 14;
        const int _TYPE_INDEX = 4115;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::System.Single spacing { get; set; }

        [ProtoMember(5)]
        public global::System.Boolean childForceExpandWidth { get; set; }

        [ProtoMember(6)]
        public global::System.Boolean childForceExpandHeight { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean childControlWidth { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean childControlHeight { get; set; }

        [ProtoMember(9)]
        public global::System.Boolean childScaleWidth { get; set; }

        [ProtoMember(10)]
        public global::System.Boolean childScaleHeight { get; set; }

        [ProtoMember(11)]
        public global::System.Boolean reverseArrangement { get; set; }

        [ProtoMember(12)]
        public TID padding { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.TextAnchor childAlignment { get; set; }

        [ProtoMember(14)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.VerticalLayoutGroup)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            spacing = o.spacing;
            childForceExpandWidth = o.childForceExpandWidth;
            childForceExpandHeight = o.childForceExpandHeight;
            childControlWidth = o.childControlWidth;
            childControlHeight = o.childControlHeight;
            childScaleWidth = o.childScaleWidth;
            childScaleHeight = o.childScaleHeight;
            reverseArrangement = o.reverseArrangement;
            padding = idmap.GetOrCreateID(o.padding);
            childAlignment = o.childAlignment;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.UI.VerticalLayoutGroup, TID>(id, gameObjectId);
            o.spacing = spacing;
            o.childForceExpandWidth = childForceExpandWidth;
            o.childForceExpandHeight = childForceExpandHeight;
            o.childControlWidth = childControlWidth;
            o.childControlHeight = childControlHeight;
            o.childScaleWidth = childScaleWidth;
            o.childScaleHeight = childScaleHeight;
            o.reverseArrangement = reverseArrangement;
            o.padding = idmap.GetObject<global::UnityEngine.RectOffset>(padding);
            o.childAlignment = childAlignment;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
