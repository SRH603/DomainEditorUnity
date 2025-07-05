using Battlehub.Storage.Enumerators.UnityEngine.Events;
using Battlehub.Storage.Surrogates.UnityEngine.Events;
using Battlehub.Storage.Surrogates.UnityExtensions;
using ProtoBuf;
using System;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.Scrollbar.ScrollEvent), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false, enumeratorType:typeof(UnityEventBaseEnumerator))]
    public class ScrollEventSurrogate<TID> : UnityEventBaseSurrogate<TID, global::UnityEngine.UI.Scrollbar.ScrollEvent> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 3;
        const int _TYPE_INDEX = 4138;

        [ProtoMember(2)]
        public override TID id { get; set; }

        [ProtoMember(3)]
        public override UnityEventPersistentCallSurrogate<TID>[] surrogates { get; set; }
    }
}
