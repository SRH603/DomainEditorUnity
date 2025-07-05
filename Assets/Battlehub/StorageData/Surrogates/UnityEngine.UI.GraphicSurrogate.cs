using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.Graphic), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class GraphicSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 8;
        const int _TYPE_INDEX = 4129;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.Color color { get; set; }

        [ProtoMember(5)]
        public global::System.Boolean raycastTarget { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Vector4 raycastPadding { get; set; }

        [ProtoMember(7)]
        public TID material { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.Graphic)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
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

            var o = idmap.GetComponent<global::UnityEngine.UI.Graphic, TID>(id, gameObjectId);
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
