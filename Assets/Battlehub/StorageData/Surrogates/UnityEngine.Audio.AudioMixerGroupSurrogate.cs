using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.Audio
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Audio.AudioMixerGroup), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class AudioMixerGroupSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 4;
        const int _TYPE_INDEX = 4152;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::System.String name { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.HideFlags hideFlags { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Audio.AudioMixerGroup)obj;
            id = idmap.GetOrCreateID(o);
            name = o.name;
            hideFlags = o.hideFlags;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            global::UnityEngine.Audio.AudioMixerGroup o = idmap.GetObject<global::UnityEngine.Audio.AudioMixerGroup>(id); 
            if (o == null)
            {
                // #warning There is no parameterless constructor. If necessary, you can add code here to create an instance of global::UnityEngine.Audio.AudioMixerGroup.
                // idmap.AddObject(o, id);
                return default;
            }
            o.name = name;
            o.hideFlags = hideFlags;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
