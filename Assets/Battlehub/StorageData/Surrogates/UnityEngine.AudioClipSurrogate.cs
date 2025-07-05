using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.AudioClip), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class AudioClipSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 4;
        const int _TYPE_INDEX = 4151;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::System.String name { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.HideFlags hideFlags { get; set; }

        [ProtoMember(5)]
        public float[] data;

        [ProtoMember(6)]
        public int lengthSamples;

        [ProtoMember(7)]
        public int channels;

        [ProtoMember(8)]
        public int frequency;

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.AudioClip)obj;
            id = idmap.GetOrCreateID(o);
            name = o.name;
            hideFlags = o.hideFlags;

            lengthSamples = o.samples;
            channels = o.channels;
            frequency = o.frequency;

            data = new float[o.samples * o.channels];
            o.GetData(data, 0);
            
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            global::UnityEngine.AudioClip o = idmap.GetObject<global::UnityEngine.AudioClip>(id); 
            if (o == null)
            {
                o = global::UnityEngine.AudioClip.Create(name, lengthSamples, channels, frequency, false);
                idmap.AddObject(o, id);
            }
            o.name = name;
            o.hideFlags = hideFlags;
            o.SetData(data, 0);
            if (!o.preloadAudioData)
            {
                o.LoadAudioData();
            }
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
