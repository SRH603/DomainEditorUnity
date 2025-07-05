using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.AnimationTriggers), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class AnimationTriggersSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 7;
        const int _TYPE_INDEX = 4128;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::System.String normalTrigger { get; set; }

        [ProtoMember(4)]
        public global::System.String highlightedTrigger { get; set; }

        [ProtoMember(5)]
        public global::System.String pressedTrigger { get; set; }

        [ProtoMember(6)]
        public global::System.String selectedTrigger { get; set; }

        [ProtoMember(7)]
        public global::System.String disabledTrigger { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.AnimationTriggers)obj;
            id = idmap.GetOrCreateID(o);
            normalTrigger = o.normalTrigger;
            highlightedTrigger = o.highlightedTrigger;
            pressedTrigger = o.pressedTrigger;
            selectedTrigger = o.selectedTrigger;
            disabledTrigger = o.disabledTrigger;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<global::UnityEngine.UI.AnimationTriggers>(id);
            o.normalTrigger = normalTrigger;
            o.highlightedTrigger = highlightedTrigger;
            o.pressedTrigger = pressedTrigger;
            o.selectedTrigger = selectedTrigger;
            o.disabledTrigger = disabledTrigger;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
