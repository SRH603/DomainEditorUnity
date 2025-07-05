using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.LayoutElement), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class LayoutElementSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 12;
        const int _TYPE_INDEX = 4112;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::System.Boolean ignoreLayout { get; set; }

        [ProtoMember(5)]
        public global::System.Single minWidth { get; set; }

        [ProtoMember(6)]
        public global::System.Single minHeight { get; set; }

        [ProtoMember(7)]
        public global::System.Single preferredWidth { get; set; }

        [ProtoMember(8)]
        public global::System.Single preferredHeight { get; set; }

        [ProtoMember(9)]
        public global::System.Single flexibleWidth { get; set; }

        [ProtoMember(10)]
        public global::System.Single flexibleHeight { get; set; }

        [ProtoMember(11)]
        public global::System.Int32 layoutPriority { get; set; }

        [ProtoMember(12)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.LayoutElement)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            ignoreLayout = o.ignoreLayout;
            minWidth = o.minWidth;
            minHeight = o.minHeight;
            preferredWidth = o.preferredWidth;
            preferredHeight = o.preferredHeight;
            flexibleWidth = o.flexibleWidth;
            flexibleHeight = o.flexibleHeight;
            layoutPriority = o.layoutPriority;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.UI.LayoutElement, TID>(id, gameObjectId);
            o.ignoreLayout = ignoreLayout;
            o.minWidth = minWidth;
            o.minHeight = minHeight;
            o.preferredWidth = preferredWidth;
            o.preferredHeight = preferredHeight;
            o.flexibleWidth = flexibleWidth;
            o.flexibleHeight = flexibleHeight;
            o.layoutPriority = layoutPriority;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
