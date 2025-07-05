using ProtoBuf;

namespace Battlehub.Storage
{
    /// <summary>
    /// Protobuf has no concept of "empty/null", or arrays. This structure is necessary to avoid losing this information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ProtoContract]
    //[MessagePackObject]
    public struct SerializableArray<T>
    {
        [ProtoMember(1)]
        public bool IsNull { get; set; }

        [ProtoMember(2)]
        public T[] Data { get; set; }

        private SerializableArray(T[] data)
        {
            IsNull = data == null;
            Data = data;
        }

        private T[] SafeGet()
        {
            if (IsNull)
            {
                return null;
            }

            if (Data == null)
            {
                return new T[0];
            }

            return Data;
        }

        public static implicit operator T[](SerializableArray<T> t) => t.SafeGet();

        public static implicit operator SerializableArray<T>(T[] t) => new SerializableArray<T>(t);
    }
}

