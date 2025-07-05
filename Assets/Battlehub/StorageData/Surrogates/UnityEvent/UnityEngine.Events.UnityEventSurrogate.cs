using Battlehub.Storage.Surrogates.UnityExtensions;
using Battlehub.Storage.UnityExtensions;
using ProtoBuf;
using System;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Battlehub.Storage.Surrogates.UnityEngine.Events
{
    public class UnityEventBaseSurrogate<TID, TEvent> : ISurrogate<TID> 
        where TEvent : UnityEventBase, new()
        where TID : IEquatable<TID> 
    {
        public virtual TID id { get; set; }
        public virtual UnityEventPersistentCallSurrogate<TID>[] surrogates { get; set; }

        public virtual async ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (TEvent)obj;
            var persistentCalls = o.GetPersistentCalls();

            id = idmap.GetOrCreateID(o);
            surrogates = new UnityEventPersistentCallSurrogate<TID>[persistentCalls.Length];

            for (int i = 0; i < persistentCalls.Length; ++i)
            {
                surrogates[i] = new UnityEventPersistentCallSurrogate<TID>();

                await surrogates[i].Serialize(persistentCalls[i], ctx);
            }
        }

        public virtual async ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<TEvent>(id);
            if (surrogates != null)
            {
                var calls = new UnityEventPersistentCall[surrogates.Length];
                for (int i = 0; i < calls.Length; ++i)
                {
                    var call = await surrogates[i].Deserialize(ctx);
                    calls[i] = (UnityEventPersistentCall)call;
                }

                o.SetPersistentCalls(calls);
            }

            return o;
        }
    }

    [ProtoContract]
    [Surrogate(typeof(UnityEvent), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class UnityEventSurrogate<TID> : UnityEventBaseSurrogate<TID, UnityEvent> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 3;
        const int _TYPE_INDEX = 190;

        [ProtoMember(2)]
        public override TID id { get; set; }

        [ProtoMember(3)]
        public override UnityEventPersistentCallSurrogate<TID>[] surrogates { get; set; }
    }
}
