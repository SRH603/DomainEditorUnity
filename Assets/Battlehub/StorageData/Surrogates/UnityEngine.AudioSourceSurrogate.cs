using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.AudioSource), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class AudioSourceSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {
        const int _PROPERTY_INDEX = 31;
        const int _TYPE_INDEX = 4149;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::System.Single volume { get; set; }

        [ProtoMember(5)]
        public global::System.Single pitch { get; set; }

        [ProtoMember(6)]
        public global::System.Single time { get; set; }

        [ProtoMember(7)]
        public global::System.Int32 timeSamples { get; set; }

        [ProtoMember(8)]
        public TID clip { get; set; }

        [ProtoMember(9)]
        public TID outputAudioMixerGroup { get; set; }

        [ProtoMember(11)]
        public global::System.Boolean loop { get; set; }

        [ProtoMember(12)]
        public global::System.Boolean ignoreListenerVolume { get; set; }

        [ProtoMember(13)]
        public global::System.Boolean playOnAwake { get; set; }

        [ProtoMember(14)]
        public global::System.Boolean ignoreListenerPause { get; set; }

        [ProtoMember(15)]
        public global::UnityEngine.AudioVelocityUpdateMode velocityUpdateMode { get; set; }

        [ProtoMember(16)]
        public global::System.Single panStereo { get; set; }

        [ProtoMember(17)]
        public global::System.Single spatialBlend { get; set; }

        [ProtoMember(18)]
        public global::System.Boolean spatialize { get; set; }

        [ProtoMember(19)]
        public global::System.Boolean spatializePostEffects { get; set; }

        [ProtoMember(20)]
        public global::System.Single reverbZoneMix { get; set; }

        [ProtoMember(21)]
        public global::System.Boolean bypassEffects { get; set; }

        [ProtoMember(22)]
        public global::System.Boolean bypassListenerEffects { get; set; }

        [ProtoMember(23)]
        public global::System.Boolean bypassReverbZones { get; set; }

        [ProtoMember(24)]
        public global::System.Single dopplerLevel { get; set; }

        [ProtoMember(25)]
        public global::System.Single spread { get; set; }

        [ProtoMember(26)]
        public global::System.Int32 priority { get; set; }

        [ProtoMember(27)]
        public global::System.Boolean mute { get; set; }

        [ProtoMember(28)]
        public global::System.Single minDistance { get; set; }

        [ProtoMember(29)]
        public global::System.Single maxDistance { get; set; }

        [ProtoMember(30)]
        public global::UnityEngine.AudioRolloffMode rolloffMode { get; set; }

        [ProtoMember(31)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.AudioSource)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            volume = o.volume;
            pitch = o.pitch;
            time = o.time;
            timeSamples = o.timeSamples;
            clip = idmap.GetOrCreateID(o.clip);
            outputAudioMixerGroup = idmap.GetOrCreateID(o.outputAudioMixerGroup);

            loop = o.loop;
            ignoreListenerVolume = o.ignoreListenerVolume;
            playOnAwake = o.playOnAwake;
            ignoreListenerPause = o.ignoreListenerPause;
            velocityUpdateMode = o.velocityUpdateMode;
            panStereo = o.panStereo;
            spatialBlend = o.spatialBlend;
            spatialize = o.spatialize;
            spatializePostEffects = o.spatializePostEffects;
            reverbZoneMix = o.reverbZoneMix;
            bypassEffects = o.bypassEffects;
            bypassListenerEffects = o.bypassListenerEffects;
            bypassReverbZones = o.bypassReverbZones;
            dopplerLevel = o.dopplerLevel;
            spread = o.spread;
            priority = o.priority;
            mute = o.mute;
            minDistance = o.minDistance;
            maxDistance = o.maxDistance;
            rolloffMode = o.rolloffMode;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.AudioSource, TID>(id, gameObjectId);
            o.volume = volume;
            o.pitch = pitch;
            o.time = time;
            o.timeSamples = timeSamples;
            o.clip = idmap.GetObject<global::UnityEngine.AudioClip>(clip);
            o.outputAudioMixerGroup = idmap.GetObject<global::UnityEngine.Audio.AudioMixerGroup>(outputAudioMixerGroup);

            o.loop = loop;
            o.ignoreListenerVolume = ignoreListenerVolume;
            o.playOnAwake = playOnAwake;
            o.ignoreListenerPause = ignoreListenerPause;
            o.velocityUpdateMode = velocityUpdateMode;
            o.panStereo = panStereo;
            o.spatialBlend = spatialBlend;
            o.spatialize = spatialize;
            o.spatializePostEffects = spatializePostEffects;
            o.reverbZoneMix = reverbZoneMix;
            o.bypassEffects = bypassEffects;
            o.bypassListenerEffects = bypassListenerEffects;
            o.bypassReverbZones = bypassReverbZones;
            o.dopplerLevel = dopplerLevel;
            o.spread = spread;
            o.priority = priority;
            o.mute = mute;
            o.minDistance = minDistance;
            o.maxDistance = maxDistance;
            o.rolloffMode = rolloffMode;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
