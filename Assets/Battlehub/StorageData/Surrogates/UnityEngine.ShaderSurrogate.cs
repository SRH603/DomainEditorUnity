using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Shader), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class ShaderSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 5;
        const int _TYPE_INDEX = 135;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::System.Int32 maximumLOD { get; set; }

        [ProtoMember(4)]
        public global::System.String name { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.HideFlags hideFlags { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Shader)obj;
            id = idmap.GetOrCreateID(o);
            maximumLOD = o.maximumLOD;
            name = o.name;
            hideFlags = o.hideFlags;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            global::UnityEngine.Shader o = idmap.GetObject<global::UnityEngine.Shader>(id); 
            if (o == null)
            {
                idmap.AddObject(o, id);
                return default;
            }
            o.maximumLOD = maximumLOD;
            o.name = name;
            o.hideFlags = hideFlags;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
