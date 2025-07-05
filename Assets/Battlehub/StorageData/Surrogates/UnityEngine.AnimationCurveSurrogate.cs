using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.AnimationCurve), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class AnimationCurveSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 5;
        const int _TYPE_INDEX = 4101;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::Battlehub.Storage.SerializableArray<global::UnityEngine.Keyframe> keys { get; set; }

        //[ProtoMember(4)]
        public global::UnityEngine.WrapMode preWrapMode { get; set; }

        //[ProtoMember(5)]
        public global::UnityEngine.WrapMode postWrapMode { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.AnimationCurve)obj;
            id = idmap.GetOrCreateID(o);
            keys = o.keys;

            //preWrapMode = o.preWrapMode;
            //postWrapMode = o.postWrapMode;

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<global::UnityEngine.AnimationCurve>(id);
            o.keys = keys;

            // o.preWrapMode = preWrapMode;
            // o.postWrapMode = postWrapMode;

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
