using Battlehub.Storage.Enumerators.UnityEngine.Events;
using Battlehub.Storage.Surrogates.UnityEngine.Events;
using Battlehub.Storage.Surrogates.UnityExtensions;
using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.ScrollRect.ScrollRectEvent), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates: false, enumeratorType: typeof(UnityEventBaseEnumerator))]
    public class ScrollRectEventSurrogate<TID> : UnityEventBaseSurrogate<TID, global::UnityEngine.UI.ScrollRect.ScrollRectEvent> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 3;
        const int _TYPE_INDEX = 4139;

        [ProtoMember(2)]
        public override TID id { get; set; }

        [ProtoMember(3)]
        public override UnityEventPersistentCallSurrogate<TID>[] surrogates { get; set; }
    }
}
