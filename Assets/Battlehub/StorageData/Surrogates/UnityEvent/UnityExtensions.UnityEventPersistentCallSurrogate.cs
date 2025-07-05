using ProtoBuf;
using System;
using System.Threading.Tasks;
using Battlehub.Storage.UnityExtensions;

namespace Battlehub.Storage.Surrogates.UnityExtensions
{
    [ProtoContract]
    [Surrogate(typeof(UnityEventPersistentCall), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates: false)]
    public class UnityEventPersistentCallSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {
        const int _PROPERTY_INDEX = 7;
        const int _TYPE_INDEX = 192;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(3)]
        public UnityEventArgumentsCacheSurrogate<TID> ArgumentsCache { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.Events.UnityEventCallState CallState { get; set; }

        [ProtoMember(5)]
        public string MethodName { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Events.PersistentListenerMode Mode { get; set; }

        [ProtoMember(7)]
        public TID Target { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public async ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (UnityEventPersistentCall)obj;
            if (o.ArgumentsCache != null)
            {
                ArgumentsCache = new UnityEventArgumentsCacheSurrogate<TID>();
                await ArgumentsCache.Serialize(o.ArgumentsCache, ctx);
            }

            Target = idmap.GetOrCreateID(o.Target);
            CallState = o.CallState;
            MethodName = o.MethodName;
            Mode = o.Mode;

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE
        }

        public async ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = UnityEventPersistentCall.CreateNew();

            if (ArgumentsCache != null)
            {
                var argumentsCache = await ArgumentsCache.Deserialize(ctx);
                o.ArgumentsCache = (UnityEventArgumentsCache)argumentsCache;
            }

            o.Target = idmap.GetObject<global::UnityEngine.Object>(Target);
            o.CallState = CallState;
            o.MethodName = MethodName;
            o.Mode = Mode;

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return o;
        }
    }
}
