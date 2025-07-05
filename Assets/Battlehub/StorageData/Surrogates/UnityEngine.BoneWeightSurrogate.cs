using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.BoneWeight), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct BoneWeightSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 9;
        const int _TYPE_INDEX = 110;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::System.Single weight0 { get; set; }

        [ProtoMember(3)]
        public global::System.Single weight1 { get; set; }

        [ProtoMember(4)]
        public global::System.Single weight2 { get; set; }

        [ProtoMember(5)]
        public global::System.Single weight3 { get; set; }

        [ProtoMember(6)]
        public global::System.Int32 boneIndex0 { get; set; }

        [ProtoMember(7)]
        public global::System.Int32 boneIndex1 { get; set; }

        [ProtoMember(8)]
        public global::System.Int32 boneIndex2 { get; set; }

        [ProtoMember(9)]
        public global::System.Int32 boneIndex3 { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.BoneWeight)obj;
            weight0 = o.weight0;
            weight1 = o.weight1;
            weight2 = o.weight2;
            weight3 = o.weight3;
            boneIndex0 = o.boneIndex0;
            boneIndex1 = o.boneIndex1;
            boneIndex2 = o.boneIndex2;
            boneIndex3 = o.boneIndex3;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.BoneWeight();
            o.weight0 = weight0;
            o.weight1 = weight1;
            o.weight2 = weight2;
            o.weight3 = weight3;
            o.boneIndex0 = boneIndex0;
            o.boneIndex1 = boneIndex1;
            o.boneIndex2 = boneIndex2;
            o.boneIndex3 = boneIndex3;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
