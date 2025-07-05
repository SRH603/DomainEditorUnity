using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityExtensions
{
    [ProtoContract]
    [Surrogate(typeof(global::Battlehub.Storage.UnityExtensions.UnityEventArgumentsCache), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class UnityEventArgumentsCacheSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 8;
        const int _TYPE_INDEX = 191;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(3)]
        public global::System.Boolean BoolArgument { get; set; }

        [ProtoMember(4)]
        public global::System.Single FloatArgument { get; set; }

        [ProtoMember(5)]
        public global::System.Int32 IntArgument { get; set; }

        [ProtoMember(6)]
        public global::System.String StringArgument { get; set; }

        [ProtoMember(7)]
        public TID ObjectArgument { get; set; }

        [ProtoMember(8)]
        public global::System.String ObjectArgumentAssemblyTypeName { get; set; }


        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::Battlehub.Storage.UnityExtensions.UnityEventArgumentsCache)obj;
            BoolArgument = o.BoolArgument;
            FloatArgument = o.FloatArgument;
            IntArgument = o.IntArgument;
            StringArgument = o.StringArgument;
            ObjectArgument = idmap.GetOrCreateID(o.ObjectArgument);
            ObjectArgumentAssemblyTypeName = o.ObjectArgumentAssemblyTypeName;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::Battlehub.Storage.UnityExtensions.UnityEventArgumentsCache();

            o.BoolArgument = BoolArgument;
            o.FloatArgument = FloatArgument;
            o.IntArgument = IntArgument;
            o.StringArgument = StringArgument;
            o.ObjectArgument = idmap.GetObject<global::UnityEngine.Object>(ObjectArgument);
            o.ObjectArgumentAssemblyTypeName = ObjectArgumentAssemblyTypeName;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
