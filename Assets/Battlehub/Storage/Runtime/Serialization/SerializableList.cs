using ProtoBuf;
using System.Collections.Generic;

namespace Battlehub.Storage
{
    /// <summary>
    /// Protobuf has no concept of "empty/null" lists. This structure is necessary to avoid losing this information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ProtoContract]
    //[MessagePackObject]
    public struct SerializableList<T>
    {
        [ProtoMember(1)]
        public bool IsNull { get; set; }

        [ProtoMember(2)]
        public List<T> Data { get; set; }

        private SerializableList(List<T> data)
        {
            IsNull = data == null;
            Data = data;
        }

        private List<T> SafeGet()
        {
            if (IsNull)
            {
                return null;
            }

            if (Data == null)
            {
                return new List<T>();
            }

            return Data;
        }

        public static implicit operator List<T>(SerializableList<T> t) => t.SafeGet();

        public static implicit operator SerializableList<T>(List<T> t) => new SerializableList<T>(t);
    }
}

