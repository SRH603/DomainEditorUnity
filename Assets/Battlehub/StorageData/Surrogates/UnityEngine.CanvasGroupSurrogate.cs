using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.CanvasGroup), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class CanvasGroupSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 8;
        const int _TYPE_INDEX = 151;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::System.Single alpha { get; set; }

        [ProtoMember(5)]
        public global::System.Boolean interactable { get; set; }

        [ProtoMember(6)]
        public global::System.Boolean blocksRaycasts { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean ignoreParentGroups { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.CanvasGroup)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            alpha = o.alpha;
            interactable = o.interactable;
            blocksRaycasts = o.blocksRaycasts;
            ignoreParentGroups = o.ignoreParentGroups;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.CanvasGroup, TID>(id, gameObjectId);
            o.alpha = alpha;
            o.interactable = interactable;
            o.blocksRaycasts = blocksRaycasts;
            o.ignoreParentGroups = ignoreParentGroups;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
