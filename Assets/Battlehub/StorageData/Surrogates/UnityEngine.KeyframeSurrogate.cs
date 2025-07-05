using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Keyframe), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct KeyframeSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 8;
        const int _TYPE_INDEX = 4102;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public global::System.Single time { get; set; }

        [ProtoMember(3)]
        public global::System.Single value { get; set; }

        /* Uncomment the ProtoMember attribute if you need to save one of the following fields */

        //[ProtoMember(4)]
        public global::System.Single inTangent { get; set; }

        //[ProtoMember(5)]
        public global::System.Single outTangent { get; set; }

        //[ProtoMember(6)]
        public global::System.Single inWeight { get; set; }

        //[ProtoMember(7)]
        public global::System.Single outWeight { get; set; }

        //[ProtoMember(8)]
        public global::UnityEngine.WeightedMode weightedMode { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Keyframe)obj;
            time = o.time;
            value = o.value;

            //inTangent = o.inTangent;
            //outTangent = o.outTangent;
            //inWeight = o.inWeight;
            //outWeight = o.outWeight;
            //weightedMode = o.weightedMode;

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.Keyframe();
            o.time = time;
            o.value = value;

            //o.inTangent = inTangent;
            //o.outTangent = outTangent;
            //o.inWeight = inWeight;
            //o.outWeight = outWeight;
            //o.weightedMode = weightedMode;

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
