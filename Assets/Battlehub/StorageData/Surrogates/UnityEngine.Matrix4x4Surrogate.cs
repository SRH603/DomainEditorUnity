using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Matrix4x4), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct Matrix4x4Surrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 17;
        const int _TYPE_INDEX = 123;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public global::System.Single m00 { get; set; }

        [ProtoMember(3)]
        public global::System.Single m10 { get; set; }

        [ProtoMember(4)]
        public global::System.Single m20 { get; set; }

        [ProtoMember(5)]
        public global::System.Single m30 { get; set; }

        [ProtoMember(6)]
        public global::System.Single m01 { get; set; }

        [ProtoMember(7)]
        public global::System.Single m11 { get; set; }

        [ProtoMember(8)]
        public global::System.Single m21 { get; set; }

        [ProtoMember(9)]
        public global::System.Single m31 { get; set; }

        [ProtoMember(10)]
        public global::System.Single m02 { get; set; }

        [ProtoMember(11)]
        public global::System.Single m12 { get; set; }

        [ProtoMember(12)]
        public global::System.Single m22 { get; set; }

        [ProtoMember(13)]
        public global::System.Single m32 { get; set; }

        [ProtoMember(14)]
        public global::System.Single m03 { get; set; }

        [ProtoMember(15)]
        public global::System.Single m13 { get; set; }

        [ProtoMember(16)]
        public global::System.Single m23 { get; set; }

        [ProtoMember(17)]
        public global::System.Single m33 { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Matrix4x4)obj;
            m00 = o.m00;
            m10 = o.m10;
            m20 = o.m20;
            m30 = o.m30;
            m01 = o.m01;
            m11 = o.m11;
            m21 = o.m21;
            m31 = o.m31;
            m02 = o.m02;
            m12 = o.m12;
            m22 = o.m22;
            m32 = o.m32;
            m03 = o.m03;
            m13 = o.m13;
            m23 = o.m23;
            m33 = o.m33;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.Matrix4x4();
            o.m00 = m00;
            o.m10 = m10;
            o.m20 = m20;
            o.m30 = m30;
            o.m01 = m01;
            o.m11 = m11;
            o.m21 = m21;
            o.m31 = m31;
            o.m02 = m02;
            o.m12 = m12;
            o.m22 = m22;
            o.m32 = m32;
            o.m03 = m03;
            o.m13 = m13;
            o.m23 = m23;
            o.m33 = m33;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
