using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Battlehub.Storage
{
    [ProtoContract]
    public struct ExternalData<TID> : IExternalData<TID>
    {
        [ProtoMember(1)]
        public string ExternalKey { get; set; }

        [ProtoMember(2)]
        public Dictionary<string, TID> ExternalIDs  { get; set; }
    }
}
