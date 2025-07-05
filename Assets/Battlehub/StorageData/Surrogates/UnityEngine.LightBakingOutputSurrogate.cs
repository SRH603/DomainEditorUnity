using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.LightBakingOutput), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct LightBakingOutputSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 6;
        const int _TYPE_INDEX = 121;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::System.Int32 probeOcclusionLightIndex { get; set; }

        [ProtoMember(3)]
        public global::System.Int32 occlusionMaskChannel { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.LightmapBakeType lightmapBakeType { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.MixedLightingMode mixedLightingMode { get; set; }

        [ProtoMember(6)]
        public global::System.Boolean isBaked { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.LightBakingOutput)obj;
            probeOcclusionLightIndex = o.probeOcclusionLightIndex;
            occlusionMaskChannel = o.occlusionMaskChannel;
            lightmapBakeType = o.lightmapBakeType;
            mixedLightingMode = o.mixedLightingMode;
            isBaked = o.isBaked;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.LightBakingOutput();
            o.probeOcclusionLightIndex = probeOcclusionLightIndex;
            o.occlusionMaskChannel = occlusionMaskChannel;
            o.lightmapBakeType = lightmapBakeType;
            o.mixedLightingMode = mixedLightingMode;
            o.isBaked = isBaked;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
