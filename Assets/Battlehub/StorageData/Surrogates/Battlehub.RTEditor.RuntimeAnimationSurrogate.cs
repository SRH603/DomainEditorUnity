using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.Battlehub.RTEditor
{
    [ProtoContract]
    [Surrogate(typeof(global::Battlehub.RTEditor.RuntimeAnimation), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class RuntimeAnimationSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 8;
        const int _TYPE_INDEX = 4098;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::System.Int32 ClipIndex { get; set; }

        [ProtoMember(5)]
        public global::System.Boolean PlayOnAwake { get; set; }

        [ProtoMember(6)]
        public global::System.Boolean Loop { get; set; }

        [ProtoMember(7)]
        public global::System.Collections.Generic.List<TID> Clips { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::Battlehub.RTEditor.RuntimeAnimation)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);

            Clips = idmap.GetOrCreateIDs(o.Clips);
            ClipIndex = o.ClipIndex;

            PlayOnAwake = o.PlayOnAwake;
            Loop = o.Loop;

            enabled = o.enabled;

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::Battlehub.RTEditor.RuntimeAnimation, TID>(id, gameObjectId);
            o.PlayOnAwake = PlayOnAwake;
            o.Loop = Loop;

            var clips = idmap.GetObjects<global::Battlehub.RTEditor.RuntimeAnimationClip, TID>(Clips);
            o.SetClips(clips, ClipIndex);

            o.enabled = enabled;

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
